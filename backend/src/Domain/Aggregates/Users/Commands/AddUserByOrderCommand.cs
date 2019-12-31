using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Events;
using Domain.Aggregates.Modules;
using Domain.Aggregates.Tracks;
using Domain.Base;
using Domain.Data;
using Domain.ECommerceIntegration.Woocommerce;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tg4.Infrastructure.Email;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Users.Commands
{
    public class AddUserByOrderCommand
    {
        public class Contract : CommandContract<Result>
        {
            public long id { get; set; }
        }

        public class EcommerceResponse
        {
            public OrderItem order { get; set; }
        }

        public class OrderItem
        {
            public long id { get; set; }
            public string status { get; set; }
            public BillingAddressItem billing_address { get; set; }
            public List<LineItem> line_items { get; set; }
            public CustomerItem customer { get; set; }
        }

        public class BillingAddressItem
        {
            public string first_name { get; set; }
            public string last_name { get; set; }
            public string email { get; set; }
            public string persontype { get; set; }
            public string cpf { get; set; }
            public string cnpj { get; set; }
        }

        public class LineItem
        {
            public string name { get; set; }
            public long product_id { get; set; }
        }

        public class CustomerItem
        {
            public long id { get; set; }
        }

        public class NewUser
        {
            public string Username { get; set; }
            public string Password { get; set; }

            public NewUser(string userName, string password)
            {
                Username = userName;
                Password = password;
            }
        }

        public class Handler : IRequestHandler<Contract, Result>
        {
            private readonly UserManager<User> _userManager;
            private readonly IDbContext _db;
            private readonly IMediator _mediator;
            private readonly IConfiguration _configuration;
            private readonly IEmailProvider _provider;

            public Handler(
                IDbContext db,
                UserManager<User> userManager,
                IMediator mediator,
                IEmailProvider provider,
                IConfiguration configuration
            ) {
                _db = db;
                _userManager = userManager;
                _mediator = mediator;
                _provider = provider;
                _configuration = configuration;
            }

            public async Task<Result> Handle(Contract request, CancellationToken cancellationToken)
            {
                string config = _configuration[$"EcommerceIntegration:Active"];

                if (config == null || config != "True")
                    return Result.Ok("Acesso Negado");

                if (request.id <= 0)
                    return Result.Ok("Acesso Negado");

                EcommerceResponse response = await GetEcommerceOrder(request.id, cancellationToken);

                if (response == null)
                    return Result.Ok("Pedido não existe");

                OrderItem order = response.order;
                
                if (String.IsNullOrEmpty(order.billing_address.cpf) &&
                    String.IsNullOrEmpty(order.billing_address.cnpj)) {
                    await CreateErrorLog(
                        "order-no-cpf-cnpj",
                        JsonConvert.SerializeObject(order),
                        cancellationToken
                    );

                    return Result.Ok("CPF/CNPJ não informado");
                }

                if (String.IsNullOrEmpty(order.billing_address.email)) {
                    await CreateErrorLog(
                        "order-no-email",
                        JsonConvert.SerializeObject(order),
                        cancellationToken
                    );

                    return Result.Ok("E-mail do usuário não informado");
                }

                if (order.status == "refunded") {
                    // TODO FAZER TRATAMENTO
                    return Result.Ok();
                }

                if (order.status == "processing")
                {
                    if (order.line_items == null || order.line_items.Count == 0)
                    {
                        await CreateErrorLog(
                            "order-has-no-products",
                            JsonConvert.SerializeObject(order),
                            cancellationToken
                        );

                        return Result.Ok("Produto não encontrado");
                    }

                    var itemsIds = order.line_items.Select(li => li.product_id);

                    var newModules = await GetModuleByEcommerceId(itemsIds, cancellationToken);
                    var newEvents = await GetEventByEcommerceId(itemsIds, cancellationToken);
                    var newTracks = await GetTrackByEcommerceId(itemsIds, cancellationToken);

                    if (newModules.Count == 0 && newEvents.Count == 0 && newTracks.Count == 0)
                    {
                        await CreateErrorLog(
                            "product-not-found-by-ids",
                            JsonConvert.SerializeObject(order),
                            cancellationToken
                        );

                        return Result.Ok("Produto(s) não encontrado(s)");
                    }

                    string cpfCnpj = order.billing_address.persontype == "F" ?
                        order.billing_address.cpf :
                        order.billing_address.cnpj;

                    /// Alterada a validação da existência de usuário ao recomendar aluno
                    /// vindo do eCommerce: anteriormente a validação era feita pelo CPF.
                    User user = await _db.UserCollection.AsQueryable()
                        .Where(x => x.Email.ToLower() == order.billing_address.email.ToLower())
                        .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                    if (user == null)
                    {
                        string userRole = "Student";

                        foreach (var track in newTracks)
                        {
                            var product = track.EcommerceProducts
                                .FirstOrDefault(e => itemsIds.Contains(e.EcommerceId));

                            var usersCount = product == null || product.UsersAmount < 1 ? 1 : product.UsersAmount;

                            if (usersCount > 1) // Fluxo de Gestor
                            {
                                userRole = "BusinessManager";
                                break;
                            }
                        }

                        User newUser = await CreateNewUser(order, cpfCnpj, userRole, cancellationToken);
                        string password = Guid.NewGuid().ToString("d").Substring(1, 6);

                        var result = await _userManager.CreateAsync(newUser, password);
                        if (!result.Succeeded)
                        {
                            await CreateErrorLog(
                                "user-creation",
                                JsonConvert.SerializeObject(result.Errors),
                                cancellationToken
                            );
                            
                            return Result.Ok("Erro ao criar usuário");
                        }

                        user = newUser;
                        user.EmailVerified = true;

                        await SendEmail(user, password, cancellationToken);
                    }

                    if (order.customer.id > 0)
                        user.EcommerceId = order.customer.id;

                    user = VerifyUserCollections(user);

                    foreach (var mod in newModules)
                    {
                        var find = user.ModulesInfo.FirstOrDefault(x => x.Id == mod.Id);
                        if (find == null)
                        {
                            var userProgress = User.UserProgress.Create(
                                mod.Id, 0, 0, mod.Title, mod.ValidFor
                            ).Data;

                            user.ModulesInfo.Add(userProgress);
                        }
                    }

                    foreach (var ev in newEvents)
                    {
                        var find = user.EventsInfo.FirstOrDefault(x => x.Id == ev.Id);
                        if (find == null)
                        {
                            var userProgress = User.UserProgress.Create(
                                ev.Id, 0, 0, ev.Title, null
                            ).Data;

                            user.EventsInfo.Add(userProgress);
                        }
                    }

                    foreach (var track in newTracks)
                    {
                        var product = track.EcommerceProducts
                            .FirstOrDefault(e => itemsIds.Contains(e.EcommerceId));

                        var usersCount = product == null || product.UsersAmount < 1 ? 1 : product.UsersAmount;

                        var find = user.TracksInfo.FirstOrDefault(x => x.Id == track.Id);
                        if (find == null)
                        {
                            var userProgress = User.UserProgress.Create(
                                track.Id, 0, 0, track.Title, track.ValidFor
                            ).Data;

                            user.TracksInfo.Add(userProgress);
                        }

                        if (usersCount > 1) // Fluxo de Gestor
                        {
                            int currentCount = 0;
                            var newUsers = new List<NewUser>();

                            while (currentCount < usersCount)
                            {
                                currentCount++;

                                User newUser = await CreateNewUserByManager(currentCount, user, track, cancellationToken);
                                string password = Guid.NewGuid().ToString("d").Substring(1, 6);

                                var result = await _userManager.CreateAsync(newUser, password);
                                if (!result.Succeeded)
                                {
                                    await CreateErrorLog(
                                        "user-creation",
                                        JsonConvert.SerializeObject(result.Errors),
                                        cancellationToken
                                    );

                                    return Result.Ok("Erro ao criar usuários para gestor: usuario" + currentCount);
                                }

                                newUsers.Add(
                                    new NewUser(newUser.UserName, password)
                                );
                            }

                            await SendManagerEmail(user, track, newUsers, cancellationToken);
                        }
                    }

                    await SavePurchaseHistory(user.Id, newModules, newEvents, newTracks, cancellationToken);

                    await _db.UserCollection.ReplaceOneAsync(
                        t => t.Id == user.Id, user,
                        cancellationToken: cancellationToken
                    );
                }

                return Result.Ok();
            }

            private async Task<string> GetNextRegistrationId(CancellationToken token)
            {
                var userCount = await _db.Database
                    .GetCollection<User>("Users")
                    .EstimatedDocumentCountAsync(cancellationToken: token);

                string prefix = DateTimeOffset.Now.Year.ToString();
                return prefix + (userCount + 1);
            }

            private async Task<User> CreateNewUser(
                OrderItem order, string cpfCnpj, string role, CancellationToken token
            ) {
                string userName = (
                    order.billing_address.first_name + " " +
                    order.billing_address.last_name
                ).Trim();

                string registrationId = await GetNextRegistrationId(token);

                var user = User.CreateUser(
                    userName,
                    order.billing_address.email,
                    order.billing_address.email,
                    registrationId,
                    cpfCnpj,
                    "", "", "", "", "",
                    role
                ).Data;

                user.EventsInfo = new List<User.UserProgress>();
                user.EventsInfo = new List<User.UserProgress>();
                user.TracksInfo = new List<User.UserProgress>();

                return user;
            }

            private async Task<User> CreateNewUserByManager(int userNumber, User manager, Track track, CancellationToken token)
            {
                string userName = "usuario" + userNumber + "_" + manager.Id.ToString().Substring(1, 8);

                string registrationId = await GetNextRegistrationId(token);

                var user = User.CreateUser(
                    "Novo Usuário", "",
                    userName,
                    registrationId, "",
                    manager.Id.ToString(),
                    manager.Name,
                    "", "", "",
                    "Student"
                ).Data;

                user.EventsInfo = new List<User.UserProgress>();
                user.EventsInfo = new List<User.UserProgress>();
                user.TracksInfo = new List<User.UserProgress>();
                user.EmailVerified = true;

                var userProgress = User.UserProgress.Create(
                    track.Id, 0, 0, track.Title, track.ValidFor
                ).Data;

                user.TracksInfo.Add(userProgress);

                return user;
            }

            private async Task<EcommerceResponse> GetEcommerceOrder(long orderId, CancellationToken token)
            {
                var response = await Woocommerce.GetOrderById(
                    orderId, _configuration
                );

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var parsed = JObject.Parse(content);
                    return JsonConvert.DeserializeObject<EcommerceResponse>(
                        content
                    );
                }
                else
                {
                    string content = await response.Content.ReadAsStringAsync();
                    await CreateErrorLog("order-not-found", content, token);
                }

                return null;
            }

            private async Task<bool> CreateErrorLog(
                string tag, string content, CancellationToken token
            ) {
                var error = ErrorLog.Create(
                    ErrorLogTypeEnum.EcommerceIntegration,
                    tag, content
                ).Data;

                await _db.ErrorLogCollection.InsertOneAsync(
                    error, cancellationToken: token
                );

                return true;
            }

            private async Task<List<Module>> GetModuleByEcommerceId(IEnumerable<long> lineItems, CancellationToken token)
            {

                return await _db.ModuleCollection.AsQueryable()
                    .Where(mod => mod.EcommerceId.HasValue && lineItems.Contains(mod.EcommerceId.Value))
                    .ToListAsync(cancellationToken: token);
            }

            private async Task<List<Event>> GetEventByEcommerceId(IEnumerable<long> lineItems, CancellationToken token)
            {
                return await _db.EventCollection.AsQueryable()
                    .Where(mod => mod.EcommerceId.HasValue && lineItems.Contains(mod.EcommerceId.Value))
                    .ToListAsync(cancellationToken: token);
            }

            private async Task<List<Track>> GetTrackByEcommerceId(IEnumerable<long> lineItems, CancellationToken token)
            {
                return await _db.TrackCollection.AsQueryable()
                    .Where(t =>
                        t.EcommerceProducts != null &&
                        t.EcommerceProducts.Any(e => lineItems.Contains(e.EcommerceId))
                    )
                    .ToListAsync(cancellationToken: token);
            }

            private async Task SavePurchaseHistory(ObjectId userId, List<Module> modules, List<Event> events, List<Track> tracks, CancellationToken token)
            {
                var historyItems = new List<PurchaseHistory.PurchaseHistory>();

                foreach (var mod in modules)
                {
                    historyItems.Add(
                        PurchaseHistory.PurchaseHistory.Create(
                            PurchaseHistory.PurchaseHistory.ProducTypeEnum.Module,
                            mod.Id, userId, DateTimeOffset.Now
                        ).Data
                    );
                }

                foreach (var ev in events)
                {
                    historyItems.Add(
                        PurchaseHistory.PurchaseHistory.Create(
                            PurchaseHistory.PurchaseHistory.ProducTypeEnum.Event,
                            ev.Id, userId, DateTimeOffset.Now
                        ).Data
                    );
                }

                foreach (var track in tracks)
                {
                    historyItems.Add(
                        PurchaseHistory.PurchaseHistory.Create(
                            PurchaseHistory.PurchaseHistory.ProducTypeEnum.Track,
                            track.Id, userId, DateTimeOffset.Now
                        ).Data
                    );
                }

                await _db.PurchaseHistoryCollection.InsertManyAsync(
                    historyItems, cancellationToken: token
                );
            }

            private async Task<bool> SendEmail(User user, string password, CancellationToken token)
            {
                try
                {
                    var emailData = new EmailUserData
                    {
                        Email = user.Email,
                        Name = user.Name,
                        ExtraData = new Dictionary<string, string> {
                            { "name", user.Name },
                            { "token", password },
                            { "username", user.UserName},
                            { "siteurl", _configuration[$"DomainOptions:SiteUrl"] },
                            { "studentmanual", _configuration[$"DomainOptions:StudentManual"] },
                            { "platformtutorial", _configuration[$"DomainOptions:PlatformTutorial"] }
                        }
                    };
                    
                    await _provider.SendEmail(
                        emailData,
                        "Seja bem-vindo à Academia!",
                        "BTG-NewUserMessage"
                    );

                    return true;
                }
                catch (Exception error)
                {
                    await CreateErrorLog(
                        "new-user-mail",
                        JsonConvert.SerializeObject(error),
                        token
                    );

                    return false;
                }
            }

            private async Task<bool> SendManagerEmail(User user, Track track, List<NewUser> newUsers, CancellationToken token)
            {
                try
                {
                    var usersList = "<ul style=\"list-style-type: none; padding: 0;\" >";
                    int userIndex = 1;

                    foreach (var newUser in newUsers)
                    {
                        var userLi = "<li style=\"margin-bottom: 10px; padding-left: 10px\" >";
                        userLi += "<b>Usuário " + userIndex + ":</b> " + newUser.Username + "<br>";
                        userLi += "<b>Senha: </b>" + newUser.Password;
                        userLi += "</li>";

                        usersList += userLi;
                        userIndex++;
                    }

                    usersList += "</ul>";

                    var emailData = new EmailUserData
                    {
                        Email = user.Email,
                        Name = user.Name,
                        ExtraData = new Dictionary<string, string> {
                            { "name", user.Name },
                            { "track", track.Title },
                            { "users", usersList },
                            { "siteurl", _configuration[$"DomainOptions:SiteUrl"] }
                        }
                    };

                    await _provider.SendEmail(
                        emailData,
                        "Usuários Criados" + " - " + track.Title,
                        "BTG-NewUserAndSubordinatesMessage"
                    );

                    return true;
                }
                catch (Exception error)
                {
                    await CreateErrorLog(
                        "new-user-mail",
                        JsonConvert.SerializeObject(error),
                        token
                    );

                    return false;
                }
            }

            private User VerifyUserCollections(User user)
            {
                if (user.ModulesInfo == null)
                    user.ModulesInfo = new List<User.UserProgress>();

                if (user.EventsInfo == null)
                    user.EventsInfo = new List<User.UserProgress>();

                if (user.TracksInfo == null)
                    user.TracksInfo = new List<User.UserProgress>();

                return user;
            }
        }
    }
}
