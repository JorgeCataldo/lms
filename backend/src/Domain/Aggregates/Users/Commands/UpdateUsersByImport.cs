using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Events;
using Domain.Aggregates.Modules;
using Domain.Aggregates.Tracks;
using Domain.Aggregates.UserProgressHistory;
using Domain.Aggregates.UsersCareer;
using Domain.Data;
using Domain.Base;
using Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.PurchaseHistory.PurchaseHistory;
using static Domain.Aggregates.Users.User;
using UserProgress = Domain.Aggregates.Users.User.UserProgress;

namespace Domain.Aggregates.Users.Commands
{
    public class UpdateUsersByImport
    {
        public class Contract : CommandContract<Result<Contract>>
        {
            public List<UserInfoDB> UsersInfos { get; set; }
        }

        public class RelationalItem
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        public class SeenUser
        {
            public string Email { get; set; }
            public ObjectId UserId { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<Contract>>
        {
            private readonly UserManager<User> _userManager;
            private readonly IDbContext _db;
            private readonly IMediator _mediator;

            public Handler(
                IDbContext db,
                UserManager<User> userManager,
                IMediator mediator
            ) {
                _db = db;
                _userManager = userManager;
                _mediator = mediator;
            }

            public async Task<Result<Contract>> Handle(Contract request, CancellationToken cancellationToken)
            {
                Console.WriteLine("Planilha Lida! " + DateTimeOffset.Now.ToString("hh:mm:ss.fff tt"));
                Console.WriteLine("Salvando Dados... " + DateTimeOffset.Now.ToString("hh:mm:ss.fff tt"));

                var usersEmails = request.UsersInfos.Select(u => u.EmailAluno).Distinct();

                var users = await _db.UserCollection.AsQueryable()
                    .Where(u => usersEmails.Contains(u.Email))
                    .ToListAsync();

                var tracks = await GetTracks(request.UsersInfos);
                var modules = await GetModules(request.UsersInfos);
                var events = await GetEvents(request.UsersInfos);

                var seenUsers = new List<SeenUser>();
                var userTracks = new List<UserProgress>();

                foreach (var userInfo in request.UsersInfos)
                {
                    var currentUser = users.FirstOrDefault(u => u.Email == userInfo.EmailAluno);
                    
                    if (!seenUsers.Any(u => u.Email == userInfo.EmailAluno))
                    {
                        currentUser = await UpdateUserInfo(tracks, currentUser, userInfo, cancellationToken);
                        await SetUserCareerInfo(currentUser, userInfo, cancellationToken);
                        await SetPurchaseHistory(tracks, currentUser, userInfo, cancellationToken);

                        seenUsers.Add(
                            new SeenUser() { Email = userInfo.EmailAluno, UserId = currentUser.Id }
                        );
                        userTracks = currentUser.TracksInfo;
                    }

                    if (CheckProductExists(modules, events, userInfo.IdModuloEvento))
                    {
                        ObjectId userId = currentUser == null ?
                            seenUsers.First(u => u.Email == userInfo.EmailAluno).UserId :
                            currentUser.Id;

                        await SetModuleProgress(modules, tracks, userTracks, userId, userInfo, cancellationToken);
                        await SetEventProgress(events, userId, userInfo, cancellationToken);
                    }
                    else
                        await LogProductError("ProductDoesntExist", userInfo, currentUser.Id, cancellationToken);
                }

                Console.WriteLine("Dados Salvos! " + DateTimeOffset.Now.ToString("hh:mm:ss.fff tt"));
                return Result.Ok(request);
            }

            private async Task<User> UpdateUserInfo(List<Track> tracks, User currentUser, UserInfoDB userInfo, CancellationToken token)
            {
                if (currentUser == null)
                {
                    currentUser = Student.CreateUser(
                        userInfo.NomeCompletoAluno,
                        userInfo.EmailAluno.Trim(),
                        userInfo.EmailAluno.Trim(),
                        userInfo.Matricula,
                        userInfo.CPFAluno,
                        "", "", "",
                        userInfo.Telefone1Aluno,
                        "", "Student"
                    ).Data;

                    currentUser.FirstAccess = false;
                    currentUser = SetUserData(currentUser, userInfo);
                    currentUser = await SetUserTrack(tracks, currentUser, userInfo, token);

                    var result = await _userManager.CreateAsync(currentUser, "Proseek2018!");
                    if (!result.Succeeded)
                    {
                        await _db.ErrorLogCollection.InsertOneAsync(
                            ErrorLog.Create(
                                ErrorLogTypeEnum.DatabaseImport,
                                "CreateNewUser", result.Errors.ToJson()
                            ).Data, cancellationToken: token
                        );
                    }
                }
                else
                {
                    currentUser = SetUserData(currentUser, userInfo);
                    currentUser = await SetUserTrack(tracks, currentUser, userInfo, token);

                    await _db.UserCollection.ReplaceOneAsync(
                        u => u.Id == currentUser.Id, currentUser,
                        cancellationToken: token
                    );
                }

                if (currentUser.TracksInfo != null && currentUser.TracksInfo.Count > 0)
                {
                    var tracksProgress = new List<UserTrackProgress>();
                    foreach (var track in currentUser.TracksInfo)
                    {
                        tracksProgress.Add(
                            new UserTrackProgress(track.Id, currentUser.Id, 0, 0)
                        );
                    }

                    await _db.UserTrackProgressCollection.InsertManyAsync(
                        tracksProgress, cancellationToken: token
                    );
                }

                return currentUser;
            }

            private User SetUserData(User user, UserInfoDB userInfo)
            {
                user.FinancialResponsible = new FinancialResponsibleInfo()
                {
                    Address = new Address(
                        userInfo.Endereco,
                        userInfo.Numero,
                        userInfo.Cidade,
                        userInfo.Estado,
                        userInfo.Pais,
                        userInfo.CEP,
                        userInfo.Bairro
                    ),
                    Email = userInfo.Email,
                    FullName = userInfo.NomeCompleto,
                    MobilePhone = userInfo.Celular,
                    Name = userInfo.Nome,
                    Phone = userInfo.Telefone,
                    Surname = userInfo.Sobrenome
                };

                if (!CellIsEmpty(userInfo.Matricula) && String.IsNullOrEmpty(user.RegistrationId))
                    user.RegistrationId = userInfo.Matricula;

                if (!CellIsEmpty(userInfo.Linkedin) && String.IsNullOrEmpty(user.LinkedIn))
                    user.LinkedIn = userInfo.Linkedin;

                if (!CellIsEmpty(userInfo.NomeCompletoAluno) && String.IsNullOrEmpty(user.Name))
                    user.Name = userInfo.NomeCompletoAluno;

                if (!CellIsEmpty(userInfo.EmailAluno) && String.IsNullOrEmpty(user.Email))
                    user.Email = userInfo.EmailAluno;

                if (user.Address == null || String.IsNullOrEmpty(user.Address.Street))
                {
                    user.Address = new Address(
                        userInfo.EnderecoAluno,
                        userInfo.ComplementoAluno,
                        userInfo.CidadeAluno,
                        userInfo.EstadoAluno,
                        "Brasil", "",
                        userInfo.BairroAluno
                    );
                }

                if (!CellIsEmpty(userInfo.Telefone1Aluno) && String.IsNullOrEmpty(user.Phone))
                    user.Phone = userInfo.Telefone1Aluno;

                if (!CellIsEmpty(userInfo.Telefone2Aluno) && String.IsNullOrEmpty(user.Phone2))
                    user.Phone2 = userInfo.Telefone2Aluno;

                if (!CellIsEmpty(userInfo.DataNascimentoAluno))
                    user.BirthDate = GetParsedDate(userInfo.DataNascimentoAluno);

                if (!CellIsEmpty(userInfo.TipoDocAluno) && user.Document == null)
                    user.Document = (DocumentType)Int32.Parse(userInfo.TipoDocAluno);

                if (!CellIsEmpty(userInfo.NumDocAluno) && String.IsNullOrEmpty(user.DocumentNumber))
                    user.DocumentNumber = userInfo.NumDocAluno;

                if (!CellIsEmpty(userInfo.OrgaoDocAluno) && String.IsNullOrEmpty(user.DocumentEmitter))
                    user.DocumentEmitter = userInfo.OrgaoDocAluno;

                if (!CellIsEmpty(userInfo.DataEmissaoDocAluno))
                    user.EmitDate = GetParsedDate(userInfo.DataEmissaoDocAluno);

                if (!CellIsEmpty(userInfo.ValidadeDocAluno))
                    user.ExpirationDate = GetParsedDate(userInfo.ValidadeDocAluno);

                if (
                    !CellIsEmpty(userInfo.PerfilProseek) ||
                    !CellIsEmpty(userInfo.Vies1) ||
                    !CellIsEmpty(userInfo.Vies2)
                ) {
                    user.Profile = new UserProfile() {
                        Title = userInfo.PerfilProseek,
                        BiasOne = userInfo.Vies1,
                        BiasTwo = userInfo.Vies2
                    };
                }

                return user;
            }

            private async Task SetUserCareerInfo(User user, UserInfoDB userInfo, CancellationToken token)
            {
                bool createNew = false;

                var userCareer = await _db.UserCareerCollection.AsQueryable()
                    .Where(uc => uc.CreatedBy == user.Id)
                    .FirstOrDefaultAsync();

                if (userCareer == null)
                {
                    userCareer = UserCareer.Create(user.Id).Data;
                    createNew = true;
                }

                if (
                    !CellIsEmpty(userInfo.GrauCurso1) ||
                    !CellIsEmpty(userInfo.NomeCurso1) ||
                    !CellIsEmpty(userInfo.Instituicao1) ||
                    !CellIsEmpty(userInfo.AnoInicio1) ||
                    !CellIsEmpty(userInfo.AnoConclusao1) ||
                    !CellIsEmpty(userInfo.PeriodoAnoConclusao1) ||
                    !CellIsEmpty(userInfo.CRAcumulado1) ||
                    !CellIsEmpty(userInfo.SituacaoCurso1) ||
                    !CellIsEmpty(userInfo.Campus1)
                ) {
                    var college = new College()
                    {
                        AcademicDegree = userInfo.GrauCurso1,
                        Name = userInfo.NomeCurso1,
                        Title = userInfo.Instituicao1,
                        Campus = userInfo.Campus1,
                        CR = userInfo.CRAcumulado1,
                        Status = userInfo.SituacaoCurso1
                    };

                    if (!CellIsEmpty(userInfo.PeriodoAnoConclusao1))
                        college.CompletePeriod = Int32.Parse(userInfo.PeriodoAnoConclusao1);

                    if (!CellIsEmpty(userInfo.AnoInicio1))
                    {
                        int year = Int32.Parse(userInfo.AnoInicio1);
                        college.StartDate = new DateTime(year, 1, 1);
                    }

                    if (!CellIsEmpty(userInfo.AnoConclusao1))
                    {
                        int year = Int32.Parse(userInfo.AnoConclusao1);
                        college.EndDate = new DateTime(year, 1, 1);
                    }

                    if (userCareer.Colleges == null)
                        userCareer.Colleges = new List<College>();

                    userCareer.Colleges.Add(college);
                }

                if (!CellIsEmpty(userInfo.Ingles))
                {
                    var language = new PerkLanguage() {
                        Names = "Inglês",
                        Level = userInfo.Ingles
                    };

                    if (userCareer.Languages == null)
                        userCareer.Languages = new List<PerkLanguage>();

                    userCareer.Languages.Add(language);
                }

                if (!CellIsEmpty(userInfo.Excel))
                {
                    var excel = new Perk() {
                        Name = "Excel",
                        Level = userInfo.Excel
                    };

                    if (userCareer.Abilities == null)
                        userCareer.Abilities = new List<Perk>();

                    userCareer.Abilities.Add(excel);
                }

                if (!CellIsEmpty(userInfo.VBA))
                {
                    var vba = new Perk()
                    {
                        Name = "VBA",
                        Level = userInfo.VBA
                    };

                    if (userCareer.Abilities == null)
                        userCareer.Abilities = new List<Perk>();

                    userCareer.Abilities.Add(vba);
                }

                if (
                    !CellIsEmpty(userInfo.Empresa1) ||
                    !CellIsEmpty(userInfo.CargoEmpresa1) ||
                    !CellIsEmpty(userInfo.DescricaoEmpresa1)
                ) {
                    var experience = new ProfessionalExperience() {
                        Title = userInfo.Empresa1,
                        Role = userInfo.CargoEmpresa1,
                        Description = userInfo.DescricaoEmpresa1
                    };
                    
                    if (userCareer.ProfessionalExperiences == null)
                        userCareer.ProfessionalExperiences = new List<ProfessionalExperience>();

                    userCareer.ProfessionalExperiences.Add(experience);
                }

                if (!CellIsEmpty(userInfo.ObjetivoProfissionalCurto) && String.IsNullOrEmpty(userCareer.ShortDateObjectives))
                    userCareer.ShortDateObjectives = userInfo.ObjetivoProfissionalCurto;

                if (!CellIsEmpty(userInfo.ObjetivoProfissionalLongo) && String.IsNullOrEmpty(userCareer.LongDateObjectives))
                    userCareer.LongDateObjectives = userInfo.ObjetivoProfissionalLongo;

                if (createNew)
                {
                    await _db.UserCareerCollection.InsertOneAsync(
                        userCareer, cancellationToken: token
                    );
                }
                else
                {
                    await _db.UserCareerCollection.ReplaceOneAsync(
                        u => u.CreatedBy == user.Id, userCareer,
                        cancellationToken: token
                    );
                }
            }

            private async Task SetPurchaseHistory(List<Track> tracks, User user, UserInfoDB userInfo, CancellationToken token)
            {
                if (!CellIsEmpty(userInfo.CompraIdObj))
                {
                    var trackId = ObjectId.Parse(userInfo.CompraIdObj);                
                    var currentTrack = tracks.FirstOrDefault(t => t.Id == trackId);

                    if (currentTrack != null)
                    {
                        bool parsed = DateTimeOffset.TryParseExact(
                            userInfo.CompraDataPgto,
                            new string[] { "M/d/yyyy", "M/dd/yyyy", "MM/d/yyyy", "MM/dd/yyyy" },
                            CultureInfo.InvariantCulture, DateTimeStyles.None,
                            out DateTimeOffset purchaseDate
                        );

                        await _db.PurchaseHistoryCollection.InsertOneAsync(
                            Create(
                                ProducTypeEnum.Track, trackId, user.Id,
                                parsed ? purchaseDate : DateTimeOffset.MinValue
                            ).Data, cancellationToken: token
                        );
                    }
                }

                if (!CellIsEmpty(userInfo.CompraIdObj2))
                {
                    var trackId = ObjectId.Parse(userInfo.CompraIdObj2);
                    var currentTrack = tracks.FirstOrDefault(t => t.Id == trackId);

                    if (currentTrack != null)
                    {
                        bool parsed = DateTimeOffset.TryParseExact(
                            userInfo.CompraDataPgto2,
                            new string[] { "M/d/yyyy", "M/dd/yyyy", "MM/d/yyyy", "MM/dd/yyyy" },
                            CultureInfo.InvariantCulture, DateTimeStyles.None,
                            out DateTimeOffset purchaseDate
                        );
                        
                        await _db.PurchaseHistoryCollection.InsertOneAsync(
                            Create(
                                ProducTypeEnum.Track, trackId, user.Id,
                                parsed ? purchaseDate : DateTimeOffset.MinValue
                            ).Data, cancellationToken: token
                        );
                    }
                }
            }

            private async Task LogPurchaseHistoryError(UserInfoDB userInfo, ObjectId userId, CancellationToken token)
            {
                await _db.ErrorLogCollection.InsertOneAsync(
                    ErrorLog.Create(
                        ErrorLogTypeEnum.DatabaseImport,
                        "TrackDoesntExist", new {
                            DataCompra = userInfo.CompraDataPgto,
                            IdProduto = userInfo.CompraIdObj,
                            UserId = userId
                        }.ToJson()
                    ).Data, cancellationToken: token
                );
            }

            private async Task<User> SetUserTrack(List<Track> tracks, User user, UserInfoDB userInfo, CancellationToken token)
            {
                if (!CellIsEmpty(userInfo.CompraIdObj))
                {
                    var trackId = ObjectId.Parse(userInfo.CompraIdObj);
                    var currentTrack = tracks.FirstOrDefault(t => t.Id == trackId);

                    if (currentTrack == null)
                        await LogPurchaseHistoryError(userInfo, user.Id, token);
                    else
                    {
                        if (user.TracksInfo == null)
                            user.TracksInfo = new List<UserProgress>();

                        var find = user.TracksInfo.FirstOrDefault(x => x.Id == currentTrack.Id);
                        if (find == null)
                        {
                            user.TracksInfo.Add(
                                UserProgress.Create(
                                    currentTrack.Id, 0, 0, currentTrack.Title, currentTrack.ValidFor
                                ).Data
                            );

                            currentTrack.StudentsCount++;
                            await _db.TrackCollection.ReplaceOneAsync(
                                t => t.Id == currentTrack.Id, currentTrack,
                                cancellationToken: token
                            );
                        }
                    }
                }

                if (!CellIsEmpty(userInfo.CompraIdObj2))
                {
                    var trackId = ObjectId.Parse(userInfo.CompraIdObj2);
                    var currentTrack = tracks.FirstOrDefault(t => t.Id == trackId);

                    if (currentTrack == null)
                        await LogPurchaseHistoryError(userInfo, user.Id, token);
                    else
                    {
                        if (user.TracksInfo == null)
                            user.TracksInfo = new List<UserProgress>();

                        var find = user.TracksInfo.FirstOrDefault(x => x.Id == currentTrack.Id);
                        if (find == null)
                        {
                            user.TracksInfo.Add(
                                UserProgress.Create(
                                    currentTrack.Id, 0, 0, currentTrack.Title, currentTrack.ValidFor
                                ).Data
                            );

                            currentTrack.StudentsCount++;
                            await _db.TrackCollection.ReplaceOneAsync(
                                t => t.Id == currentTrack.Id, currentTrack,
                                cancellationToken: token
                            );
                        }
                    }
                }

                return user;
            }

            private async Task SetModuleProgress(List<Module> modules, List<Track> tracks, List<UserProgress> userTracks, ObjectId userId, UserInfoDB userInfo, CancellationToken token)
            {
                var moduleId = ObjectId.Parse(userInfo.IdModuloEvento);
                var currentModule = modules.FirstOrDefault(t => t.Id == moduleId);

                if (currentModule != null)
                {
                    await _db.ModuleGradeCollection.InsertOneAsync(
                        ModuleGrade.Create(
                            ObjectId.Parse(userInfo.IdModuloEvento), userId,
                            CellIsEmpty(userInfo.Pontuacao) ? 0 : decimal.Parse(userInfo.Pontuacao.Replace(".", ",")),
                            CellIsEmpty(userInfo.Presenca) ? 0 : decimal.Parse(userInfo.Presenca.Replace(".", ","))
                        ).Data, cancellationToken: token
                    );

                    int currentLevel = GetLevelByGrade(userInfo.Pontuacao);
                    var progressesToAdd = new List<UserSubjectProgress>();

                    if (currentModule.Subjects != null && currentModule.Subjects.Count > 0)
                    {
                        foreach (var subject  in currentModule.Subjects)
                        {
                            var progress = UserSubjectProgress.Create(moduleId, subject.Id, userId);
                            progress.Level = currentLevel;
                            progress.Points = 100 * currentLevel;
                            progressesToAdd.Add(progress);
                        }

                        await _db.UserSubjectProgressCollection.InsertManyAsync(
                            progressesToAdd, cancellationToken: token
                        );
                    }

                    var moduleProgress = UserModuleProgress.Create(moduleId, userId);
                    moduleProgress.Level = currentLevel;
                    moduleProgress.Points = progressesToAdd.Sum(p => p.Points);

                    await _db.UserModuleProgressCollection.InsertOneAsync(
                        moduleProgress, cancellationToken: token
                    );
                    
                    var track = tracks.FirstOrDefault(t =>
                        userTracks.Any(ut => ut.Id == t.Id) &&
                        t.ModulesConfiguration.Any(m => m.ModuleId == moduleId)
                    );

                    if (track != null)
                    {
                        var trackProgress = await _db.UserTrackProgressCollection
                            .AsQueryable()
                            .Where(t => t.TrackId == track.Id && t.UserId == userId)
                            .FirstOrDefaultAsync();

                        if (trackProgress != null)
                        {
                            int modulesCompleted = 0;

                            var trackConfig = track.ModulesConfiguration.FirstOrDefault(m => m.ModuleId == moduleId);

                            if (trackConfig != null && currentLevel > trackConfig.Level)
                            {
                                modulesCompleted++;
                                trackProgress.ModulesCompleted.Add(moduleId);
                                trackProgress.Progress = (decimal)modulesCompleted / (decimal)track.ModulesConfiguration.Count;

                                await _db.UserTrackProgressCollection.ReplaceOneAsync(
                                    t => t.Id == trackProgress.Id, trackProgress,
                                    cancellationToken: token
                                );
                            }
                        }
                    }
                }
            }

            private async Task SetEventProgress(List<Event> events, ObjectId userId, UserInfoDB userInfo, CancellationToken token)
            {
                var eventId = ObjectId.Parse(userInfo.IdModuloEvento);
                var currentEvent = events.FirstOrDefault(t => t.Id == eventId);

                if (currentEvent != null)
                {
                    if (currentEvent.Schedules == null)
                    {
                        await LogProductError("ScheduleDoesntExist", userInfo, userId, token);
                        return;
                    }

                    decimal grade = CellIsEmpty(userInfo.Pontuacao) ? 0 : decimal.Parse(userInfo.Pontuacao.Replace(".", ","));

                    var schedule = currentEvent.Schedules.FirstOrDefault();

                    if (schedule == null)
                        await LogProductError("ScheduleDoesntExist", userInfo, userId, token);
                    else
                    {
                        var application = new EventApplication()
                        {
                            UserId = userId,
                            EventId = currentEvent.Id,
                            ScheduleId = schedule.Id,
                            EventDate = schedule.EventDate,
                            ApplicationStatus = ApplicationStatus.Approved,
                            RequestedDate = DateTimeOffset.Now,
                            ResolutionDate = DateTimeOffset.Now,
                            OrganicGrade = grade,
                            InorganicGrade = grade,
                            UserPresence = true
                        };

                        application.GradeBaseValues = new List<BaseValue>();

                        if (!CellIsEmpty(userInfo.CC))
                            application.GradeBaseValues.Add(new BaseValue() { Key = "CC", Value = userInfo.CC });

                        if (!CellIsEmpty(userInfo.CS))
                            application.GradeBaseValues.Add(new BaseValue() { Key = "CS", Value = userInfo.CS });

                        if (!CellIsEmpty(userInfo.CCS))
                            application.GradeBaseValues.Add(new BaseValue() { Key = "CCS", Value = userInfo.CCS });

                        if (!CellIsEmpty(userInfo.REIPODO))
                            application.GradeBaseValues.Add(new BaseValue() { Key = "REIPODO", Value = userInfo.REIPODO });

                        if (!CellIsEmpty(userInfo.ADODO))
                            application.GradeBaseValues.Add(new BaseValue() { Key = "ADODO", Value = userInfo.ADODO });

                        if (!CellIsEmpty(userInfo.EAEDO))
                            application.GradeBaseValues.Add(new BaseValue() { Key = "EAEDO", Value = userInfo.EAEDO });

                        if (!CellIsEmpty(userInfo.PA))
                            application.GradeBaseValues.Add(new BaseValue() { Key = "PA", Value = userInfo.PA });

                        if (!CellIsEmpty(userInfo.FA))
                            application.GradeBaseValues.Add(new BaseValue() { Key = "FA", Value = userInfo.FA });

                        if (!CellIsEmpty(userInfo.EA))
                            application.GradeBaseValues.Add(new BaseValue() { Key = "EA", Value = userInfo.EA });

                        if (!CellIsEmpty(userInfo.CapacidadeTecnica))
                            application.GradeBaseValues.Add(new BaseValue() { Key = "CapacidadeTecnica", Value = userInfo.CapacidadeTecnica });

                        if (!CellIsEmpty(userInfo.Postura))
                            application.GradeBaseValues.Add(new BaseValue() { Key = "Postura", Value = userInfo.Postura });

                        if (!CellIsEmpty(userInfo.Argumentacao))
                            application.GradeBaseValues.Add(new BaseValue() { Key = "Argumentacao", Value = userInfo.Argumentacao });
                        
                        if (!CellIsEmpty(userInfo.Articulacao))
                            application.GradeBaseValues.Add(new BaseValue() { Key = "Articulacao", Value = userInfo.Articulacao });
                        
                        if (!CellIsEmpty(userInfo.Negociacao))
                            application.GradeBaseValues.Add(new BaseValue() { Key = "Negociacao", Value = userInfo.Negociacao });

                        await _db.EventApplicationCollection.InsertOneAsync(
                            application, cancellationToken: token
                        );
                    }
                }
            }

            private async Task LogProductError(string TagLog, UserInfoDB userInfo, ObjectId userId, CancellationToken token)
            {
                await _db.ErrorLogCollection.InsertOneAsync(
                    ErrorLog.Create(
                        ErrorLogTypeEnum.DatabaseImport,
                        TagLog, new {
                            NomeProduto = userInfo.NomeModulo,
                            IdProduto = userInfo.IdModuloEvento,
                            UserId = userId
                        }.ToJson()
                    ).Data, cancellationToken: token
                );
            }

            private async Task<List<Track>> GetTracks(List<UserInfoDB> userInfos)
            {
                var tracksIds1 = userInfos
                    .Where(u => !CellIsEmpty(u.CompraIdObj))
                    .Select(u => ObjectId.Parse(u.CompraIdObj))
                    .Distinct();

                var tracksIds2 = userInfos
                    .Where(u => !CellIsEmpty(u.CompraIdObj2))
                    .Select(u => ObjectId.Parse(u.CompraIdObj2))
                    .Distinct();

                return await _db.TrackCollection.AsQueryable()
                    .Where(t => tracksIds1.Contains(t.Id) || tracksIds2.Contains(t.Id))
                    .ToListAsync();
            }

            private async Task<List<Module>> GetModules(List<UserInfoDB> userInfos)
            {
                var modulesIds = userInfos
                    .Where(u => !CellIsEmpty(u.IdModuloEvento))
                    .Select(u => ObjectId.Parse(u.IdModuloEvento))
                    .Distinct();

                return await _db.ModuleCollection.AsQueryable()
                    .Where(t => modulesIds.Contains(t.Id))
                    .ToListAsync();
            }

            private async Task<List<Event>> GetEvents(List<UserInfoDB> userInfos)
            {
                var eventsIds = userInfos
                    .Where(u => !CellIsEmpty(u.IdModuloEvento))
                    .Select(u => ObjectId.Parse(u.IdModuloEvento))
                    .Distinct();

                return await _db.EventCollection.AsQueryable()
                    .Where(t => eventsIds.Contains(t.Id))
                    .ToListAsync();
            }

            private bool CheckProductExists(List<Module> modules, List<Event> events, string productIdStr)
            {
                if (CellIsEmpty(productIdStr))
                    return false;

                var productId = ObjectId.Parse(productIdStr);

                return modules.Any(t => t.Id == productId) || events.Any(t => t.Id == productId);
            }

            private DateTimeOffset? GetParsedDate(string dateStr)
            {
                bool parsed = DateTimeOffset.TryParseExact(
                    dateStr,
                    new string[] { "M/d/yyyy", "M/dd/yyyy", "MM/d/yyyy", "MM/dd/yyyy" },
                    CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out DateTimeOffset parsedDate
                );

                return parsed ? parsedDate : DateTimeOffset.MinValue;
            }

            private int GetLevelByGrade(string grade)
            {
                decimal parsedGrade = CellIsEmpty(grade) ? 0 : decimal.Parse(grade.Replace(".", ","));
                if (parsedGrade < 4)
                    return 0;
                else if (parsedGrade < 6)
                    return 1;
                else if (parsedGrade < 7)
                    return 2;
                else if (parsedGrade < 10)
                    return 3;
                return 4;
            }

            private bool CellIsEmpty(string cellValue)
            {
                return String.IsNullOrEmpty(cellValue) ||
                    cellValue == "-" ||
                    cellValue == "\"\"" ||
                    cellValue == "Não sei";
            }
        }
    }
}
