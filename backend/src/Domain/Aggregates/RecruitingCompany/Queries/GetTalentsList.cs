using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Events;
using Domain.Aggregates.UserProgressHistory;
using Domain.Aggregates.Users;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
using Domain.Aggregates.UsersCareer;
using Domain.ValueObjects;
using System.Globalization;

namespace Domain.Aggregates.RecruitingCompany.Queries
{
    public class GetTalentsList
    {
        public class Contract : CommandContract<Result<PagedUserItems>>
        {
            public int Page { get; set; }
            public int PageSize { get; set; }
            public RequestFilters Filters { get; set; }
            public string UserRole { get; set; }
            public ObjectId UserId { get; set; }
            public bool MandatoryFields { get; set; }
            public bool SelectAllUsers { get; set; }
        }

        public class RequestFilters
        {
            public string Term { get; set; }
            public List<UserCategoryFilter> CategoryFilter { get; set; }
            public string SortBy { get; set; }
            public bool? IsSortAscending { get; set; }
        }

        public class UserCategoryFilter
        {
            public string ColumnName { get; set; }
            public List<string> ContentNames { get; set; }
        }

        public class PagedUserItems
        {
            public int Page { get; set; }
            public long ItemsCount { get; set; }
            public List<UserItem> UserItems { get; set; }
        }

        public class ProseekPerfilItem
        {
            public ObjectId UserId { get; set; }
            public List<BaseValue> UserGradeBaseValues { get; set; }
        }

        public class UserItem
        {
            public ObjectId Id { get; set; }
            public ObjectId ResponsibleId { get; set; }
            public string ImageUrl { get; set; }
            public string Name { get; set; }
            public string LineManager { get; set; }
            public string UserName { get; set; }
            public string RegistrationId { get; set; }
            public User.RelationalItem Rank { get; set; }
            public List<User.UserProgress> TracksInfo { get; set; }
            public List<User.UserProgress> ModulesInfo { get; set; }
            public DateTimeOffset? DateBorn { get; set; }
            public Address Address { get; set; }
            public bool IsFavorite { get; set; } = false;
        }

        public class Handler : IRequestHandler<Contract, Result<PagedUserItems>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<PagedUserItems>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole != "Recruiter" && request.UserRole != "Admin" && request.UserRole != "HumanResources")
                    return Result.Fail<PagedUserItems>("Acesso Negado");

                var queryable = _db.UserCollection.AsQueryable();

                queryable = SetFilters(request, queryable);

                var favoritesIds = await GetFavorites(request.UserId);

                var users = await queryable
                    .Select(u => new UserItem {
                        Id = u.Id,
                        ImageUrl = u.ImageUrl,
                        LineManager = u.LineManager,
                        Name = u.Name,
                        Rank = u.Rank,
                        ResponsibleId = u.ResponsibleId,
                        UserName = u.UserName,
                        RegistrationId = u.RegistrationId,
                        TracksInfo = u.TracksInfo,
                        DateBorn = u.DateBorn,
                        Address = u.Address,
                        IsFavorite = favoritesIds.Contains(u.Id)
                    }).ToListAsync();

                var usersIds = users.Select(u => u.Id).ToList();

                var careers = new List<UserCareer>();

                if (request.MandatoryFields)
                {
                    careers = await GetCareers(request, usersIds);
                }

                if (request.Filters.CategoryFilter != null && request.Filters.CategoryFilter.Count > 0)
                {
                    
                    var progressList = new List<UserModuleProgress>();
                    var applications = new List<EventApplication>();
                    
                    var proseekPerfils = new List<ProseekPerfilItem>();

                    var careerKeys = new List<string>()
                    {
                        "hist.degree", "hist.collegeName", "hist.year", "hist.cr", "compForm", "compForm.name",
                        "compForm.level", "compInfo.travel", "compInfo.certificates", "compInfo.languages",
                        "compInfo.languages.name", "compInfo.languages.level", "compInfo.skills"
                    };
                    var perfilKeys = new List<string>()
                    {
                        "proseek.perfil", "proseek.vies"
                    };

                    request.Filters.CategoryFilter = request.Filters.CategoryFilter.Where(x => x.ContentNames.Count > 0).ToList();

                    var checkProgress = request.Filters.CategoryFilter.Any(f =>
                        (f.ColumnName == "module.id" || f.ColumnName == "level.id") &&
                        f.ContentNames.Count > 0
                    );

                    var checkApplications = request.Filters.CategoryFilter.Any(f =>
                        f.ColumnName == "event.id" &&
                        f.ContentNames.Count > 0
                    );

                    var checkCareer = request.Filters.CategoryFilter.Any(f =>
                        careerKeys.Contains(f.ColumnName) &&
                        f.ContentNames.Count > 0
                    );

                    var checkPerfil = request.Filters.CategoryFilter.Any(f =>
                        perfilKeys.Contains(f.ColumnName) &&
                        f.ContentNames.Count > 0
                    );

                    if (checkProgress)
                        progressList = await GetModuleProgress(usersIds);

                    if (checkApplications)
                        applications = await GetApplications(usersIds);

                    if (checkCareer)
                    {
                        if (!request.MandatoryFields)
                        {
                            careers = await GetCareers(request, usersIds);
                        }
                        MixCompFormFilter(request.Filters.CategoryFilter);
                        MixCompLanguageFilter(request.Filters.CategoryFilter);
                    }

                    if (checkPerfil)
                        proseekPerfils = await GetPerfils(usersIds);

                    foreach (UserCategoryFilter userCategoryFilter in request.Filters.CategoryFilter)
                    {
                        var isCareer = careerKeys.Contains(userCategoryFilter.ColumnName);
                        var isPerfil = perfilKeys.Contains(userCategoryFilter.ColumnName);
                        if (isCareer)
                        {
                            foreach (string contentName in userCategoryFilter.ContentNames)
                            {
                                careers = FilterByCareer(
                                    careers, userCategoryFilter.ColumnName, contentName
                                );
                            }
                        }
                        else if (isPerfil)
                        {
                            foreach (string contentName in userCategoryFilter.ContentNames)
                            {
                                proseekPerfils = FilterByPerfil(
                                    proseekPerfils, userCategoryFilter.ColumnName, contentName
                                );
                            }
                        }
                        else
                        {
                            foreach (string contentName in userCategoryFilter.ContentNames)
                            {
                                users = FilterByProgress(
                                    users, userCategoryFilter.ColumnName, contentName,
                                    progressList, applications
                                );
                            }
                        }      
                    }
                    
                    if (checkCareer && !request.MandatoryFields)
                    {
                        var careerUserIds = careers.Select(x => x.CreatedBy).ToList();
                        users = users.Where(x => careerUserIds.Contains(x.Id)).ToList();
                    }

                    if (checkPerfil)
                    {
                        var proseekPerfilsIds = proseekPerfils.Select(x => x.UserId).ToList();
                        users = users.Where(x => proseekPerfilsIds.Contains(x.Id)).ToList();
                    }
                }

                if (request.MandatoryFields)
                {
                    var careerUserIds = careers.Select(x => x.CreatedBy).ToList();
                    users = users.Where(x => careerUserIds.Contains(x.Id)).ToList();
                }

                var result = new PagedUserItems();

                if (!request.SelectAllUsers)
                {
                    result = new PagedUserItems()
                    {
                        Page = request.Page,
                        ItemsCount = users.Count,
                        UserItems = users
                        .OrderByDescending(u => u.IsFavorite)
                        .Skip((request.Page - 1) * request.PageSize)
                        .Take(request.PageSize)
                        .ToList()
                    };
                }
                else
                {
                    result = new PagedUserItems()
                    {
                        Page = request.Page,
                        ItemsCount = users.Count,
                        UserItems = users
                        .OrderByDescending(u => u.IsFavorite)
                        .ToList()
                    };
                }

                return Result.Ok(result);
            }

            private IMongoQueryable<User> SetFilters(Contract request, IMongoQueryable<User> queryable)
            {
                queryable = queryable.Where(u =>
                    u.Id != request.UserId && !u.IsBlocked
                );

                if (request.UserRole != "Admin")
                {
                    queryable = queryable.Where(u =>
                        u.AllowRecommendation.HasValue &&
                        u.AllowRecommendation.Value &&
                        u.SecretaryAllowRecommendation.HasValue &&
                        u.SecretaryAllowRecommendation.Value
                    );
                }

                if (request.MandatoryFields)
                {
                    queryable = queryable.Where(u =>
                        !string.IsNullOrEmpty(u.ImageUrl) &&
                        !string.IsNullOrEmpty(u.Name) &&
                        u.DateBorn != null &&
                        u.Address != null &&
                        !string.IsNullOrEmpty(u.Address.Street) &&
                        !string.IsNullOrEmpty(u.Address.District) &&
                        !string.IsNullOrEmpty(u.Address.City) &&
                        !string.IsNullOrEmpty(u.Address.State) &&
                        !string.IsNullOrEmpty(u.Phone) &&
                        !string.IsNullOrEmpty(u.Email)
                    );
                }

                if (request.Filters == null) return queryable;

                if (!String.IsNullOrEmpty(request.Filters.Term))
                {
                    queryable = queryable.Where(user =>
                        user.Name.ToLower()
                            .Contains(request.Filters.Term.ToLower()) ||
                        user.Email.ToLower()
                            .Contains(request.Filters.Term.ToLower())
                        );
                }

                if (!string.IsNullOrEmpty(request.Filters.SortBy))
                {
                    if (request.Filters.SortBy == "name")
                    {
                        if (request.Filters.IsSortAscending.Value)
                            queryable = queryable.OrderBy(user => user.Name);
                        else
                            queryable = queryable.OrderByDescending(user => user.Name);
                    }
                }

                return queryable;
            }

            private List<UserItem> FilterByProgress(
                List<UserItem> users, string columnName, string value,
                List<UserModuleProgress> progressList, List<EventApplication> applications
            ) {
                switch (columnName)
                {
                    case "track.id":
                        return users.Where(u =>
                            u.TracksInfo != null &&
                            u.TracksInfo.Any(t => t.Id == ObjectId.Parse(value))
                        ).ToList();
                    case "module.id":
                        return users.Where(u =>
                            progressList.Find(p =>
                                p.UserId == u.Id &&
                                p.ModuleId == ObjectId.Parse(value)
                            ) != null
                        ).ToList();
                    case "level.id":
                        return users.Where(u =>
                            progressList.Any(p =>
                                p.Level > Int32.Parse(value)
                            )
                        ).ToList();
                    case "event.id":
                        return users.Where(u =>
                            applications.Any(app =>
                                app.UserId == u.Id &&
                                app.EventId == ObjectId.Parse(value)
                            )
                        ).ToList();
                    case "isFavorite":
                        return users.Where(u =>
                             value == "true" ? u.IsFavorite : !u.IsFavorite
                         ).ToList();
                    default:
                        return users;
                }
            }

            private List<UserCareer> FilterByCareer(
                List<UserCareer> users, string columnName, string value
            )
            {
                switch (columnName)
                {
                    case "compInfo.travel":
                        return users.Where(u =>
                            value == "true" ? u.TravelAvailability : !u.TravelAvailability
                        ).ToList();
                    case "hist.degree":
                        return users.Where(u =>
                            u.Colleges != null &&
                            u.Colleges.Any(t => t.AcademicDegree == value)
                        ).ToList();
                    case "hist.collegeName":
                        return users.Where(u =>
                            u.Colleges != null &&
                            u.Colleges.Any(t => t.Name.ToLower().Contains(value.ToLower()))
                        ).ToList();
                    case "hist.year":
                        var yearSemester = value.Split('.');
                        var year = int.Parse(yearSemester[0]);
                        var semester = int.Parse(yearSemester[1]);
                        return users.Where(u =>
                            u.Colleges != null &&
                            u.Colleges.Any(t => 
                                t.EndDate.HasValue && 
                                t.EndDate.Value.Year == year &&
                                t.CompletePeriod == semester
                            )
                        ).ToList();
                    case "hist.cr":
                        return users.Where(u =>
                            u.Colleges != null &&
                            u.Colleges.Any(t => 
                                !string.IsNullOrEmpty(t.CR) &&
                                t.CR.All(x => char.IsDigit(x) || x == '.' || x == ',') &&
                                decimal.Parse(t.CR.Replace(',', '.'), CultureInfo.InvariantCulture) >= decimal.Parse(value.Replace(',', '.'), CultureInfo.InvariantCulture)
                            )
                        ).ToList();
                    case "compForm":
                        var nameValue = value.Split('-');
                        return users.Where(u =>
                            u.Abilities != null &&
                            u.Abilities.Any(t => 
                                t.Name.ToLower().Contains(nameValue[0].ToLower()) &&
                                t.Level.ToLower().Contains(nameValue[1].ToLower())
                            )
                        ).ToList();
                    case "compForm.name":
                        return users.Where(u =>
                            u.Abilities != null &&
                            u.Abilities.Any(t => t.Name.ToLower().Contains(value.ToLower()))
                        ).ToList();
                    case "compForm.level":
                        return users.Where(u =>
                            u.Abilities != null &&
                            u.Abilities.Any(t => t.Level.ToLower().Contains(value.ToLower()))
                        ).ToList();
                    case "compInfo.certificates":
                        return users.Where(u =>
                            u.Certificates != null &&
                            u.Certificates.Any(t => t.Title.ToLower().Contains(value.ToLower()))
                        ).ToList();
                    case "compInfo.skills":
                        return users.Where(u =>
                            u.Skills != null &&
                            u.Skills.Any(t => t.ToLower().Contains(value.ToLower()))
                        ).ToList();
                    case "compInfo.languages":
                        var languageValue = value.Split('-');
                        return users.Where(u =>
                            u.Languages != null &&
                            u.Languages.Any(t =>
                                t.Names.ToLower().Contains(languageValue[0].ToLower()) &&
                                t.Level.ToLower().Contains(languageValue[1].ToLower())
                            )
                        ).ToList();
                    case "compInfo.languages.name":
                        return users.Where(u =>
                            u.Languages != null &&
                            u.Languages.Any(t => t.Names.ToLower().Contains(value.ToLower()))
                        ).ToList();
                    case "compInfo.languages.level":
                        return users.Where(u =>
                            u.Languages != null &&
                            u.Languages.Any(t => t.Level.ToLower().Contains(value.ToLower()))
                        ).ToList();
                    default:
                        return users;
                }
            }

            private List<ProseekPerfilItem> FilterByPerfil(
                List<ProseekPerfilItem> users, string columnName, string value
            )
            {
                switch (columnName)
                {
                    case "proseek.perfil":
                        return FilterPrefil(users, value);
                    case "proseek.vies":
                        return FilterVies(users, value);
                    default:
                        return users;
                }
            }
            private async Task<List<UserModuleProgress>> GetModuleProgress(List<ObjectId> usersIds)
            {
                var collection = _db.Database.GetCollection<UserModuleProgress>("UserModuleProgress");
                var queryable = collection.AsQueryable();

                return await queryable.Where(modProgress =>
                    usersIds.Contains(modProgress.UserId)
                ).ToListAsync();
            }

            private async Task<List<EventApplication>> GetApplications(List<ObjectId> usersIds)
            {
                var collection = _db.Database.GetCollection<EventApplication>("EventApplications");
                var queryable = collection.AsQueryable();

                return await queryable.Where(app =>
                    usersIds.Contains(app.UserId) &&
                    app.ApplicationStatus == ApplicationStatus.Approved &&
                    app.UserPresence.HasValue &&
                    app.UserPresence.Value
                ).ToListAsync();
            }

            private async Task<List<ObjectId>> GetFavorites(ObjectId recruiterId)
            {
                var queryable = _db.RecruitmentFavoriteCollection.AsQueryable();

                return await queryable
                    .Where(f => f.RecruiterId == recruiterId)
                    .Select(f => f.UserId)
                    .ToListAsync();
            }


            private async Task<List<UserCareer>> GetCareers(Contract request, List<ObjectId> usersIds)
            {
                var queryable = _db.UserCareerCollection.AsQueryable();

                if (request.MandatoryFields)
                {
                    return await queryable
                        .Where(c => 
                            usersIds.Contains(c.CreatedBy) &&
                            c.Colleges != null &&
                            c.Colleges.Any(t =>
                                !string.IsNullOrEmpty(t.Title) &&
                                !string.IsNullOrEmpty(t.Campus) &&
                                !string.IsNullOrEmpty(t.Name) &&
                                !string.IsNullOrEmpty(t.AcademicDegree) &&
                                t.EndDate != null
                            ) &&
                            c.ProfessionalExperience == true &&
                            c.ProfessionalExperiences.Any(t =>
                                !string.IsNullOrEmpty(t.Title) &&
                                !string.IsNullOrEmpty(t.Role) &&
                                !string.IsNullOrEmpty(t.Description) &&
                                t.EndDate != null
                            ) &&
                            c.Abilities != null &&
                            c.Abilities.Any(t => t.Name.ToLower().Contains("excel")) &&
                            c.Abilities.Any(t => t.Name.ToLower().Contains("vba"))
                        )
                    .ToListAsync();
                }
                else
                {
                    return await queryable
                        .Where(c => usersIds.Contains(c.CreatedBy))
                    .ToListAsync();
                }
            }

            private async Task<List<ProseekPerfilItem>> GetPerfils(List<ObjectId> usersIds)
            {
                var listproseekPerfilItem = new List<ProseekPerfilItem>();
                var eventApplications = await _db.EventApplicationCollection
                    .AsQueryable()
                    .Where(x => usersIds.Contains(x.UserId))
                    .Select(x => new ProseekPerfilItem()
                    {
                        UserId = x.UserId,
                        UserGradeBaseValues = x.GradeBaseValues
                    })
                    .ToListAsync();

                if (eventApplications == null || eventApplications.Count == 0)
                    return listproseekPerfilItem;

                foreach(ProseekPerfilItem eventApp in eventApplications)
                {
                    var listUser = listproseekPerfilItem.FirstOrDefault(x => x.UserId == eventApp.UserId);
                    if (listUser == null)
                    {
                        if(eventApp.UserGradeBaseValues == null)
                        {
                            eventApp.UserGradeBaseValues = new List<BaseValue>();
                        }
                        listproseekPerfilItem.Add(eventApp);
                    }
                    else
                    {
                        UpdateKeyValues(listUser.UserGradeBaseValues, eventApp.UserGradeBaseValues);
                    }
                }

                return listproseekPerfilItem;
            }

            private void UpdateKeyValues(List<BaseValue> currentValues, List<BaseValue> newValues)
            {
                if (newValues != null && newValues.Count > 0)
                {
                    foreach (BaseValue baseValue in newValues)
                    {
                        var currentKey = currentValues.FirstOrDefault(x => x.Key == baseValue.Key);
                        if (currentKey == null)
                        {
                            currentValues.Add(baseValue);
                        }
                        else
                        {
                            var itemValue = string.IsNullOrEmpty(currentKey.Value) ? 0 : decimal.Parse(currentKey.Value.Replace(',', '.'));
                            var addItemValue = string.IsNullOrEmpty(baseValue.Value) ? 0 : decimal.Parse(baseValue.Value.Replace(',', '.'));
                            var valuesSum = itemValue + addItemValue;
                            currentKey.Value = valuesSum == 0 ? "" : valuesSum.ToString();
                        }
                    }
                }
            }

            private List<ProseekPerfilItem> FilterPrefil(List<ProseekPerfilItem> users, string type)
            {
                foreach (ProseekPerfilItem user in users.Reverse<ProseekPerfilItem>())
                {
                    if (user.UserGradeBaseValues == null || user.UserGradeBaseValues.Count == 0)
                    {
                        users.Remove(user);
                    }
                    else
                    {
                        var userPerfilType = "";
                        var CCValue = GetKeyValue(user.UserGradeBaseValues, "CC");
                        var CSValue = GetKeyValue(user.UserGradeBaseValues, "CS");
                        var CCSValue = GetKeyValue(user.UserGradeBaseValues, "CCS");
                        var analyticValue = (CCValue ?? 0) + (CSValue ?? 0) + (CCSValue ?? 0);

                        var PAValue = GetKeyValue(user.UserGradeBaseValues, "PA");
                        var FAValue = GetKeyValue(user.UserGradeBaseValues, "FA");
                        var EAValue = GetKeyValue(user.UserGradeBaseValues, "EA");
                        var teamPlayerValue = (PAValue ?? 0) + (FAValue ?? 0) + (EAValue ?? 0);

                        var RIPODOValue = GetKeyValue(user.UserGradeBaseValues, "RIPODO");
                        var ADODOValue = GetKeyValue(user.UserGradeBaseValues, "ADODO");
                        var EAEDOValue = GetKeyValue(user.UserGradeBaseValues, "EAEDO");
                        var assertiveValue = (RIPODOValue ?? 0) + (ADODOValue ?? 0) + (EAEDOValue ?? 0);

                        if (analyticValue > teamPlayerValue && analyticValue > assertiveValue)
                        {
                            userPerfilType = "ANALÍTICO";
                        }
                        else if (teamPlayerValue > analyticValue && teamPlayerValue > assertiveValue)
                        {
                            userPerfilType = "TEAM-PLAYER";
                        }
                        else if (assertiveValue > analyticValue && assertiveValue > teamPlayerValue)
                        {
                            userPerfilType = "ASSERTIVO";
                        }

                        if (userPerfilType != type)
                            users.Remove(user);
                    }
                }
                return users;
            }

            private decimal? GetKeyValue(List<BaseValue> values, string key)
            {
                var baseValue = values.FirstOrDefault(x => x.Key == key);
                if (baseValue == null || string.IsNullOrEmpty(baseValue.Value))
                    return null;
                return decimal.Parse(baseValue.Value);
            }

            private List<ProseekPerfilItem> FilterVies(List<ProseekPerfilItem> users, string type)
            {
                foreach (ProseekPerfilItem user in users.Reverse<ProseekPerfilItem>())
                {
                    if (user.UserGradeBaseValues == null || user.UserGradeBaseValues.Count == 0)
                    {
                        users.Remove(user);
                    }
                    else
                    {
                        var userPerfilType = "";

                        if (type == "PROPOSITIVO" || type == "QUESTIONADOR")
                        {
                            var QCValue = GetKeyValue(user.UserGradeBaseValues, "QC");
                            if (QCValue != null)
                            {
                                if (QCValue > 1)
                                {
                                    userPerfilType = "PROPOSITIVO";
                                }
                                else
                                {
                                    userPerfilType = "QUESTIONADOR";
                                }
                            }
                        }
                        else if (type == "REATIVO" || type == "PROATIVO")
                        {
                            var IOValue = GetKeyValue(user.UserGradeBaseValues, "I/O");
                            if (IOValue != null && IOValue != 0)
                            {
                                if (IOValue > 0)
                                {
                                    userPerfilType = "REATIVO";
                                }
                                else
                                {
                                    userPerfilType = "PROATIVO";
                                }
                            }
                        }

                        if (userPerfilType != type)
                            users.Remove(user);
                    }
                }
                return users;
            }

            private void MixCompFormFilter(List<UserCategoryFilter> CategoryFilters)
            {
                var compFormName = CategoryFilters.FirstOrDefault(x => x.ColumnName == "compForm.name");
                var compFormLevel = CategoryFilters.FirstOrDefault(x => x.ColumnName == "compForm.level");
                if (compFormName != null && compFormLevel != null)
                {
                    var mixedUserCategoryFilter = new UserCategoryFilter
                    {
                        ColumnName  = "compForm",
                        ContentNames = new List<string>()
                    };

                    for(int i = 0; i < compFormName.ContentNames.Count; i++)
                    {
                        for (int j = 0; j < compFormLevel.ContentNames.Count; j++)
                        {
                            mixedUserCategoryFilter.ContentNames.Add(compFormName.ContentNames[i] + "-" + compFormLevel.ContentNames[j]);
                        }
                    };

                    CategoryFilters.Remove(compFormName);
                    CategoryFilters.Remove(compFormLevel);
                    CategoryFilters.Add(mixedUserCategoryFilter);
                }
            }

            private void MixCompLanguageFilter(List<UserCategoryFilter> CategoryFilters)
            {
                var compInfoName = CategoryFilters.FirstOrDefault(x => x.ColumnName == "compInfo.languages.name");
                var compInfoLevel = CategoryFilters.FirstOrDefault(x => x.ColumnName == "compInfo.languages.level");
                if (compInfoName != null && compInfoLevel != null)
                {
                    var mixedUserLaguageCategoryFilter = new UserCategoryFilter
                    {
                        ColumnName = "compInfo.languages",
                        ContentNames = new List<string>()
                    };

                    for (int i = 0; i < compInfoName.ContentNames.Count; i++)
                    {
                        for (int j = 0; j < compInfoLevel.ContentNames.Count; j++)
                        {
                            mixedUserLaguageCategoryFilter.ContentNames.Add(compInfoName.ContentNames[i] + "-" + compInfoLevel.ContentNames[j]);
                        }
                    };

                    CategoryFilters.Remove(compInfoName);
                    CategoryFilters.Remove(compInfoLevel);
                    CategoryFilters.Add(mixedUserLaguageCategoryFilter);
                }
            }
        }
    }
}
