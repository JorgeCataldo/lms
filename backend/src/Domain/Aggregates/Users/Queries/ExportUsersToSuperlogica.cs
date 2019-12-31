using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Events;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.Users.User;
using Domain.IdentityStores;
using Domain.ECommerceIntegration.PagarMe;
using Microsoft.Extensions.Configuration;
using System.Net;
using Newtonsoft.Json;
using Domain.ECommerceIntegration.Woocommerce;

namespace Domain.Aggregates.Users.Queries
{
    public class ExportUsersToSuperlogica
    {
        public class Contract : CommandContract<Result<List<UserItemExport>>>
        {
            public string id { get; set; }
        }
        
        public class UserItemExport
        {
            //public string aluno_nome { get; set; }
            //public string matricula_data { get; set; }
            //public string turma_data_inicio { get; set; }
            //public string curso_nome { get; set; }
            //public string plapagamento_id { get; set; }

            public string Email { get; set; }
            public string Curso { get; set; }
        }

        public class UserItem
        {
            public string Name { get; set; }
            public DateTimeOffset DateCreated { get; set; }
            public List<UserProgress> ModulesInfo { get; set; }
            public List<UserProgress> TracksInfo { get; set; }
        }

        public class RelationalItem
        {
            public ObjectId Id { get; set; }
            public string Name { get; set; }
        }

        public class EcommerceResponse
        {
            public OrderItem order { get; set; }
        }

        public class OrderItem
        {
            public long id { get; set; }
            public string status { get; set; }
            public List<LineItem> line_items { get; set; }
        }

        public class LineItem
        {
            public string name { get; set; }
            public long product_id { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<List<UserItemExport>>>
        {
            private readonly IDbContext _db;
            private readonly IConfiguration _configuration;

            public Handler(IDbContext db, IMediator mediator, IConfiguration configuration)
            {
                _db = db;
                _configuration = configuration;
            }

            public async Task<Result<List<UserItemExport>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                //if (request.UserRole == "Student")
                //    return Result.Fail<List<UserItem>>("Acesso Negado");                               

                var userItems = new List<UserItemExport>();
                var currentTransaction = new TransactionItem();
                var currentProduct = new EcommerceResponse();

                //int i = 0;
                //int k = 0;

                var emailsNaoEncontrados = new List<string>();

                string[] emails = new string[] {"felipeestrella@terra.com.br"
                                ,"gabrielnevesfm@gmail.com"
                                ,"lucasrj_Israel@Hotmail.com"
                                ,"hebertguerra123@gmail.com"
                                ,"mr.vinicius224@gmail.com"
                                ,"mschutzgarcia@gmail.com"
                                ,"pedrowerneck2012@gmail.com"
                                ,"bernardo.magalhaes@btgpactual.com"
                                ,"falcaojoaoguilherme@gmail.com"
                                ,"joaogdomiciano@gmail.com"
                                ,"gabrielgruberbernstein@gmail.com"
                                ,"Viniciusvales@hotmail.com"
                                ,"gusnascimento@gmail.com"
                                ,"pedro.desa@live.com"
                                ,"johnson_hsu-97@hotmail.com"
                                ,"henrique_audrey@hotmail.com"
                                ,"tales.alves@icloud.com"
                                ,"tales_alves@icloud.com"
                                ,"paulobaffi@gmail.com"
                                ,"luzbrafael@gmail.com"
                                ,"jvmorcerf@hotmail.com"
                                ,"sabbaalfredo@gmail.com"
                                ,"jpbelmonte1992@gmail.com"
                                ,"eduardacaldas@hotmail.com"
                                ,"tilacorte@gmail.com"
                                ,"gabrielmaga@gmail.com"
                                ,"pedronudelman@gmail.com"
                                ,"fellipe0205@gmail.com"
                                ,"micaelsantos182@gmail.com"
                                ,"lizarralderdaniel@gmail.com"
                                ,"CARRILHO.MARIANA@YAHOO.COM"
                                ,"thompsonvaz@icloud.com"
                                ,"rodrigo.gomes36@yahoo.com.br"
                                ,"lopezramos@uol.com.br"
                                ,"victorvidal012@gmail.com"
                                ,"alinemelorj@live.com"
                                ,"pereira.alvaro12@gmail.com"
                                ,"evandromanso.jf@gmail.com"
                                ,"santos_nathalia@hotmail.com"
                                ,"thomasaltit@hotmail.com"
                                ,"cazonirj@yahoo.com.br"
                                ,"ricardolang2012@gmail.com"
                                ,"azulayleo@gmail.com"
                                ,"marcoa.carvalho@outlook.com"
                                ,"altamiromotta@petrobras.com.br"
                                ,"matheusaflorim@gmail.com"
                                ,"erickrodrigues.contato@gmail.com"
                                ,"ajoaovtbs@gmail.com"
                                ,"juliafbsmith@gmail.com"
                                ,"jcrochadacosta@gmail.com"
                                ,"jefferson-flu@hotmail.com"
                                ,"joaopauloleitao1@gmail.com"
                                ,"feliperangel@hotmail.com"
                                ,"danielapl.cavalheio@gmail.com"
                                ,"guicollier@gmail.com"
                                ,"lucascarelli@hotmail.com"
                                ,"andre.oliveira.rj@gmail.com"
                                ,"renato.higa@me.com"
                                ,"mateus.damata98@gmail.com"
                                ,"gustavogarcez@live.com"
                                ,"amanda1311@gmail.com"
                                ,"joaovitor_0908@hotmail.com"
                                ,"mscamposinvest@hotmail.com"
                                ,"ziraldonobrega_flh@hotmail.com"
                                ,"rilb@uol.com.br"
                                ,"carolinnemaroni@gmail.com"
                                ,"webmaster@proseek.com"
                                ,"rciarlini@globo.com"
                                ,"rodrigohamacher@gmail.com"
                                ,"josevictor03@gmail.com"
                                ,"dani.strude@gmail.com"
                                ,"frederico@grupotp.com.br"
                                ,"dfdb0405@gmail.com"
                                ,"lucasmoura@gmail.com"
                                ,"hellen_hellen2008@hotmail.com"
                                ,"edurego99@hotmail.com"
                                ,"caio.fguerra@hotmail.com"
                                ,"igorchaffin@hotmail.con"
                                ,"paulo-investments@outlook.com"
                                ,"felipe.spolaor@hotmail.com"
                                ,"guilherme.placidino@live.com"
                                ,"lucascostacfp@gmail.com"
                                ,"chrismycruz@gmail.com"
                                ,"f.cassab@uol.com.br"
                                ,"portes_leo@hotmail.com"
                                ,"josevictorgaspari@gmail.com"
                                ,"lhteixeirapinto@outlook.com"
                                ,"daniel.cecchetti@outlook.com"
                                ,"andressaanr@gmail.com"
                                ,"franceschiniju@gmail.com"
                                ,"gabriel.fama@hotmail.com"
                                ,"matheusvmartins06@gmail.com"
                                ,"ps501676@gmail.com"
                                ,"caiosantosnobre@gmail.com"
                                ,"lucas.silva.neris@gmail.com"
                                ,"leticianacifgomes@hotmail.com"
                                ,"paulabazzo@yahoo.com.br"
                                ,"pedro.marcio.ctc@gmail.com"
                                ,"sidsamer@hotmail.com"
                                ,"pwnagegameplay@gmail.com"
                                ,"lucas.victore@gmail.com"
                                ,"allysoncabral10@icloud.com"
                                ,"farialucas@gmail.com"
                                ,"gabriel.almeidagass@gmail.com"
                                ,"contato@allevi.com.br"
                                ,"douglas.o.decastro@gmail.com"
                                ,"alexandre@tourgourmet.com.br"
                                ,"mlcmb73@gmail.com"
                                ,"caiodimas@hotmail.com"
                                ,"gabriel.rdk@outlook.com"
                                ,"luigibrcl@yahoo.com"
                                ,"junior.war1000@gmail.com"
                                ,"alangadelha17@gmail.com"
                                ,"fbleinat@gmail.com"
                                ,"allanbandrade@gmail.com"
                                ,"alexanderjonathan97@gmail.com"
                                ,"michelebruno@uol.com.br"
                                ,"joao.gottgtroy@gmail.com"
                                ,"pedro.areli@hotmail.com"
                                ,"ennesreis86@gmail.com"
                                ,"herbertyukihiro@gmail.com"
                                ,"gilpamplona@id.uff.br"
                                ,"renaralia@hotmail.com"
                                ,"pessoar25@gmail.com"
                                ,"ffazio@uol.com.br"
                                ,"isabela.araujo@bancointer.com.br"
                                ,"osieldamatta@oi.com.br"
                                ,"sulamy1955@gmail.com"
                                ,"LUIZ.GOMES@LIVE.COM"
                                ,"ricardonucara.uffinance@gmail.com"
                                ,"fernanda.luz@safra.com.br"
                                ,"lucasfcopereira@gmail.com"
                                ,"rsilva.investimentos@gmail.com" };

                try
                {

                    for (int i = 0; i < emails.Length; i++)
                    {
                        var dbStudents = await _db.UserCollection
                            .AsQueryable()
                            .Where(x => x.Email.ToLower() == emails[i].ToLower())
                            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                        if(dbStudents == null)
                        {
                            emailsNaoEncontrados.Add(emails[i]);
                            continue;
                        }

                        if (dbStudents.ModulesInfo != null)
                        {
                            for (var k = 0; k < dbStudents.ModulesInfo.Count; k++)
                            {
                                userItems.Add(new UserItemExport
                                {
                                    Email = dbStudents.Email,
                                    Curso = dbStudents.ModulesInfo[k].Name
                                });
                            }
                        }
                        if (dbStudents.TracksInfo != null)
                        {
                            for (var k = 0; k < dbStudents.TracksInfo.Count; k++)
                            {
                                userItems.Add(new UserItemExport
                                {
                                    Email = dbStudents.Email,
                                    Curso = dbStudents.TracksInfo[k].Name
                                });
                            }
                        }
                    }

                    //var transactions = new List<TransactionItem>();
                    //var response = await PagarMe.GetTransactions(_configuration);

                    //if (response.StatusCode == HttpStatusCode.OK)
                    //{
                    //    var content = await response.Content.ReadAsStringAsync();
                    //    //var parsed = JObject.Parse(content);
                    //    transactions = JsonConvert.DeserializeObject<List<TransactionItem>>(
                    //        content
                    //    );
                    //}
                    //else
                    //{
                    //    string content = await response.Content.ReadAsStringAsync();
                    //    //await CreateErrorLog("transaction-not-found", content, cancellationToken);
                    //}

                    //for (i = 0; i < transactions.Count; i++)
                    //{
                    //    currentTransaction = transactions[i];
                    //    var orderResp = await Woocommerce.GetOrderById(transactions[i].metadata.order_number, _configuration);
                    //    var product = new EcommerceResponse();

                    //    if (orderResp.StatusCode == HttpStatusCode.OK)
                    //    {
                    //        var content = await orderResp.Content.ReadAsStringAsync();
                    //        //var parsed = JObject.Parse(content);
                    //        product = JsonConvert.DeserializeObject<EcommerceResponse>(
                    //            content
                    //        );

                    //        currentProduct = product;
                    //    }
                    //    //else
                    //    //{
                    //    //    continue;
                    //    //    //string content = await response.Content.ReadAsStringAsync();
                    //    //    //await CreateErrorLog("transaction-not-found", content, cancellationToken);
                    //    //}

                    //    userItems.Add(new UserItemExport
                    //    {
                    //        aluno_nome = transactions[i].customer != null ? transactions[i].customer.name : "",
                    //        matricula_data = transactions[i].formated_date_created,
                    //        plapagamento_id = transactions[i].payment_method,
                    //        turma_data_inicio = transactions[i].formated_date_created,
                    //        curso_nome = product.order != null ? product.order.line_items[0].name : ""
                    //    });





                    //var dbStudents = await _db.UserCollection
                    //    .AsQueryable()
                    //    .Where(x => (x.ModulesInfo != null && x.ModulesInfo.Count > 0)
                    //        || (x.TracksInfo != null && x.TracksInfo.Count > 0))
                    //    .ToListAsync(cancellationToken: cancellationToken);

                    //for (i = 0; i < dbStudents.Count; i++)
                    //{
                    //    if (dbStudents[i].ModulesInfo != null)
                    //    {
                    //        for (k = 0; k < dbStudents[i].ModulesInfo.Count; k++)
                    //        {
                    //            userItems.Add(new UserItemExport
                    //            {
                    //                aluno_nome = dbStudents[i].Name,
                    //                matricula_data = dbStudents[i].ModulesInfo[k].CreatedAt != null ?
                    //                                    dbStudents[i].ModulesInfo[k].CreatedAt.Value.ToString("dd/MM/yyyy") :
                    //                                    dbStudents[i].CreatedAt.ToString("dd/MM/yyyy"),
                    //                turma_data_inicio = dbStudents[i].ModulesInfo[k].CreatedAt != null ?
                    //                                    dbStudents[i].ModulesInfo[k].CreatedAt.Value.ToString("dd/MM/yyyy") :
                    //                                    dbStudents[i].CreatedAt.ToString("dd/MM/yyyy"),
                    //                plapagamento_id = "X",
                    //                curso_nome = dbStudents[i].ModulesInfo[k].Name
                    //            });
                    //        }
                    //    }
                    //    if (dbStudents[i].TracksInfo != null)
                    //    {
                    //        for (k = 0; k < dbStudents[i].TracksInfo.Count; k++)
                    //        {
                    //            userItems.Add(new UserItemExport
                    //            {
                    //                aluno_nome = dbStudents[i].Name,
                    //                matricula_data = dbStudents[i].TracksInfo[k].CreatedAt != null ?
                    //                                    dbStudents[i].TracksInfo[k].CreatedAt.Value.ToString("dd/MM/yyyy") :
                    //                                    dbStudents[i].CreatedAt.ToString("dd/MM/yyyy"),
                    //                turma_data_inicio = dbStudents[i].TracksInfo[k].CreatedAt != null ?
                    //                                    dbStudents[i].TracksInfo[k].CreatedAt.Value.ToString("dd/MM/yyyy") :
                    //                                    dbStudents[i].CreatedAt.ToString("dd/MM/yyyy"),
                    //                plapagamento_id = "X",
                    //                curso_nome = dbStudents[i].TracksInfo[k].Name
                    //            });
                    //        }
                    //    }
                    //}

                    //var dbEvents = await _db.EventCollection
                    //    .AsQueryable()
                    //    .Where(x => eventsIds.Contains(x.Id))
                    //    .Select(x => new RelationalItem
                    //    {
                    //        Id = x.Id,
                    //        Name = x.Title
                    //    })
                    //    .ToListAsync(cancellationToken: cancellationToken);
                    //}
                    var emailsString = string.Join(";", emailsNaoEncontrados);
                    return Result.Ok(userItems.ToList());
                }
                catch (Exception ex)
                {
                    //var iError = i;
                    //var kError = k;
                    //var errorTransaction = currentTransaction;
                    //var errorProduct = currentProduct;
                    return Result.Fail<List<UserItemExport>>("Acesso Negado");
                }
            }
        }
    }
}
