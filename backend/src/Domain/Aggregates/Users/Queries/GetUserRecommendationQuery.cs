using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;
using Domain.Aggregates.UsersCareer;
using Domain.Aggregates.Events;
using System;
using Domain.ValueObjects;
using System.Globalization;

namespace Domain.Aggregates.Users.Queries
{
    public class GetUserRecommendationQuery
    {
        public class Contract : CommandContract<Result<RecommendationItem>>
        {
            public string UserId { get; set; }
            public string CurrentUserRole { get; set; }
            public string CurrentUserId { get; set; }
        }

        public class RecommendationItem
        {
            public UserInfoItem UserInfo { get; set; }
            public UserCareer UserCareer { get; set; }
            public List<UserEventApplicationItem> UserEventApplications { get; set; }
            public bool CurrentUser { get; set; }
            public bool CanFavorite { get; set; } = false;
            public bool IsFavorite { get; set; } = false;
        }

        public class UserInfoItem
        {
            public string ImageUrl { get; set; }
            public string Name { get; set; }
            public string Info { get; set; }
            public DateTimeOffset? DateBorn { get; set; }
            public Address Address { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public string Phone2 { get; set; }
            public string LinkedIn { get; set; }
            public User.UserProfile Profile { get; set; }
            public bool? AllowRecommendation { get; set; }
            public bool? SecretaryAllowRecommendation { get; set; }
        }

        public class UserEventApplicationItem
        {
            public ObjectId EventId { get; set; }
            public string EventName { get; set; }
            public List<BaseValue> UserGradeBaseValues { get; set; }
            public decimal QCGrade { get; set; }
            public decimal TGGrade { get; set; }
            public decimal FAGrade { get; set; }
            public string FeaturedStudent { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<RecommendationItem>>
        {
            private readonly IDbContext _db;
            private readonly List<string> _userGrades = new List<string> { "Auto_QC", "Auto_TG", "Auto_FA", "Intragrupo_QC",
                "Intragrupo_TG", "Intragrupo_FA", "Nota_Professor_Grupo", "Nota_Final", "QC", "TG", "FA", "featured_student" };

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<RecommendationItem>> Handle(Contract request, CancellationToken cancellationToken)
            {
                var recomendation = new RecommendationItem
                {
                    CurrentUser = request.CurrentUserId == request.UserId
                };
                recomendation.UserInfo = await GetUserInfo(request, cancellationToken);
                recomendation.UserCareer = await GetUserCareer(request, cancellationToken);
                recomendation.UserEventApplications = await GetUserGradeBaseValues(request, cancellationToken);

                if (request.CurrentUserRole == "Recruiter" || request.CurrentUserRole == "HumanResources")
                {
                    recomendation.CanFavorite = true;
                    recomendation.IsFavorite = await CheckIsFavorite(request.UserId, request.CurrentUserId);
                }

                return Result.Ok(recomendation);
            }

            private async Task<UserCareer> GetUserCareer(Contract request, CancellationToken token)
            {
                var userId = ObjectId.Parse(request.UserId);

                return await _db.UserCareerCollection
                    .AsQueryable()
                    .Where(x => x.CreatedBy == userId)
                    .FirstOrDefaultAsync(cancellationToken: token);
            }

            private async Task<UserInfoItem> GetUserInfo(Contract request, CancellationToken token)
            {
                var userId = ObjectId.Parse(request.UserId);

                return await _db.UserCollection
                    .AsQueryable()
                    .Where(x => x.Id == userId)
                    .Select(x => new UserInfoItem
                    {
                        ImageUrl = x.ImageUrl,
                        Name = x.Name,
                        Info = x.Info,
                        DateBorn = x.DateBorn,
                        Address = x.Address,
                        Email = x.Email,
                        Phone = x.Phone,
                        Phone2 = x.Phone2,
                        LinkedIn = x.LinkedIn,
                        Profile = x.Profile,
                        AllowRecommendation = x.AllowRecommendation,
                        SecretaryAllowRecommendation = x.SecretaryAllowRecommendation
                    })
                    .FirstOrDefaultAsync();
            }

            private async Task<List<UserEventApplicationItem>> GetUserGradeBaseValues(Contract request, CancellationToken token)
            {
                var userId = ObjectId.Parse(request.UserId);
                var filteredGradesList = new List<UserEventApplicationItem>();

                var eventApllicationsGrades =  await _db.EventApplicationCollection
                    .AsQueryable()
                    .Where(x => x.UserId == userId)
                    .Select(x => new UserEventApplicationItem
                    {
                        EventId =  x.EventId,
                        UserGradeBaseValues = x.GradeBaseValues
                    })
                    .ToListAsync(token);

                if (eventApllicationsGrades == null || eventApllicationsGrades.Count == 0)
                    return filteredGradesList;

                var eventIds = eventApllicationsGrades.Select(x => x.EventId).ToList();

                var events = await _db.EventCollection
                    .AsQueryable()
                    .Where(x => eventIds.Contains(x.Id))
                    .ToListAsync(token);

                foreach (UserEventApplicationItem listGrade in eventApllicationsGrades)
                {
                    if (listGrade.UserGradeBaseValues != null && listGrade.UserGradeBaseValues.Count > 0) //&& CheckEventGradeValid(listGrade.UserGradeBaseValues)
                    {
                        var currentEvent = events.FirstOrDefault(x => x.Id == listGrade.EventId);
                        if (currentEvent != null)
                        {
                            listGrade.EventName = currentEvent.Title;
                            filteredGradesList.Add(FillGradeFields(listGrade));
                        }
                    }
                }

                return filteredGradesList;
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
                            var itemValue = string.IsNullOrEmpty(currentKey.Value) ? 0 : decimal.Parse(currentKey.Value.Replace(',','.'));
                            var addItemValue = string.IsNullOrEmpty(baseValue.Value) ? 0 : decimal.Parse(baseValue.Value.Replace(',', '.'));
                            var valuesSum = itemValue + addItemValue;
                            currentKey.Value = valuesSum == 0 ? "" : valuesSum.ToString();
                        }
                    }
                }
            }

            private async Task<bool> CheckIsFavorite(string userIdStr, string recruiterIdStr)
            {
                var userId = ObjectId.Parse(userIdStr);
                var recruiterId = ObjectId.Parse(recruiterIdStr);

                return await _db.RecruitmentFavoriteCollection.AsQueryable()
                    .AnyAsync(r => r.UserId == userId && r.RecruiterId == recruiterId);

            }

            private bool CheckEventGradeValid(List<BaseValue> baseValues)
            {
                foreach (string userGrade in _userGrades)
                {
                    var foundGrade = baseValues.FirstOrDefault(x => x.Key == userGrade);

                    if (foundGrade == null)
                        return false;
                }
                return true;
            }

            private UserEventApplicationItem FillGradeFields(UserEventApplicationItem userEventApplicationItem)
            {
                //var autoQC = decimal.Parse(userEventApplicationItem.UserGradeBaseValues.First(x => x.Key == "Auto_QC").Value, CultureInfo.InvariantCulture);
                //var autoTG = decimal.Parse(userEventApplicationItem.UserGradeBaseValues.First(x => x.Key == "Auto_TG").Value, CultureInfo.InvariantCulture);
                //var autoFA = decimal.Parse(userEventApplicationItem.UserGradeBaseValues.First(x => x.Key == "Auto_FA").Value, CultureInfo.InvariantCulture);
                //var intragrupoQC = decimal.Parse(userEventApplicationItem.UserGradeBaseValues.First(x => x.Key == "Intragrupo_QC").Value, CultureInfo.InvariantCulture);
                //var intragrupoTG = decimal.Parse(userEventApplicationItem.UserGradeBaseValues.First(x => x.Key == "Intragrupo_TG").Value, CultureInfo.InvariantCulture);
                //var intragrupoFA = decimal.Parse(userEventApplicationItem.UserGradeBaseValues.First(x => x.Key == "Intragrupo_FA").Value, CultureInfo.InvariantCulture);
                //var notaProfessorGrupo = decimal.Parse(userEventApplicationItem.UserGradeBaseValues.First(x => x.Key == "Nota_Professor_Grupo").Value, CultureInfo.InvariantCulture);
                var QC = decimal.Parse(userEventApplicationItem.UserGradeBaseValues.First(x => x.Key == "QC").Value, CultureInfo.InvariantCulture);
                var TG = decimal.Parse(userEventApplicationItem.UserGradeBaseValues.First(x => x.Key == "TG").Value, CultureInfo.InvariantCulture);
                var FA = decimal.Parse(userEventApplicationItem.UserGradeBaseValues.First(x => x.Key == "FA").Value, CultureInfo.InvariantCulture);
                var featuredStudent = userEventApplicationItem.UserGradeBaseValues.First(x => x.Key == "featured_student").Value;

                //userEventApplicationItem.QCGrade = (autoQC * 1 / 10) + (intragrupoQC * 6 / 10) + (notaProfessorGrupo * 3 / 10);
                //userEventApplicationItem.TGGrade = (autoTG * 1 / 10) + (intragrupoTG * 6 / 10) + (notaProfessorGrupo * 3 / 10);
                //userEventApplicationItem.FAGrade = (autoFA * 1 / 10) + (intragrupoFA * 6 / 10) + (notaProfessorGrupo * 3 / 10);

                userEventApplicationItem.QCGrade = QC;
                userEventApplicationItem.TGGrade = TG;
                userEventApplicationItem.FAGrade = FA;

                return userEventApplicationItem;
            }
        }
    }
}
