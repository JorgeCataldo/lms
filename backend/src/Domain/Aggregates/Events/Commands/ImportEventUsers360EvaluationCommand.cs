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

namespace Domain.Aggregates.Events.Commands
{
    public class ImportEventUsers360EvaluationCommand
    {
        public class Contract : CommandContract<Result<Contract>>
        {
            public string EventId { get; set; }
            public string EventScheduleId { get; set; }
            public string FileContent { get; set; }
        }

        public class UserEventApplicationItem
        {
            public ObjectId UserId { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string TranscribedParticipation { get; set; }
            public List<BaseValue> GradeBaseValues { get; set; }
        }

        public class UserBase
        {
            public ObjectId UserId { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<Contract>>
        {
            private readonly IDbContext _db;
            private readonly int keysStartColumn = 3;

            public Handler(IDbContext db)
            {
                _db = db;
            }

            public async Task<Result<Contract>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (string.IsNullOrEmpty(request.EventId))
                    return Result.Fail<Contract>("Id do evento não informado");
                if (string.IsNullOrEmpty(request.EventScheduleId))
                    return Result.Fail<Contract>("Id do agendamento do evento não informado");

                var list = ReadAsList(request.FileContent);
                if (list == null)
                    return Result.Fail<Contract>("Lista nula");

                var eventId = ObjectId.Parse(request.EventId);
                var eventScheduleId = ObjectId.Parse(request.EventScheduleId);

                var buildedFile = BuildFile(list);

                buildedFile = await FindUsers(buildedFile, cancellationToken);
                 if(buildedFile.Count == 0)
                    return Result.Fail<Contract>("Não foram encontrados usuários com esses nomes/emails");

                var updated = await UpdateUserEventApplicationGrades(buildedFile, eventId, eventScheduleId, cancellationToken);
                if (!updated)
                    return Result.Fail<Contract>("Não foram encontradas aplicações para estes usários neste evento");

                return Result.Ok(request);
            }

            private List<string> ReadAsList(string content)
            {
                return string.IsNullOrEmpty(content)
                    ? null
                    : content.Split('\n').ToList();
            }

            private List<UserEventApplicationItem> BuildFile(List<string> fileList)
            {
                List<UserEventApplicationItem> usersFile = new List<UserEventApplicationItem>();
                List<string> keys = new List<string>();

                for (int i = 0; i < fileList.Count; i++)
                {
                    var splittedRow = fileList[i].Split(';');
                    if (i == 0)
                    {
                        keys = SetKeys(splittedRow);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(splittedRow[0]) && splittedRow[0].Length > 3)
                        {
                            var parsedUser = ParseToUserEventApplication(splittedRow, keys);
                            var listUser = usersFile.FirstOrDefault(x => x.Name == splittedRow[0]);
                            if (listUser == null)
                            {
                                usersFile.Add(parsedUser);
                            }
                            else
                            {
                                UpdateKeyValues(listUser.GradeBaseValues, parsedUser.GradeBaseValues);
                                if (!string.IsNullOrEmpty(parsedUser.TranscribedParticipation))
                                {
                                    listUser.TranscribedParticipation = listUser.TranscribedParticipation + ";" +
                                        parsedUser.TranscribedParticipation;
                                }
                            }
                        }
                    }
                }
                return usersFile;
            }

            private List<string> SetKeys(string[] content)
            {
                List<string> keys = new List<string>();
                for (int i = keysStartColumn; i < content.Length; i++)
                {
                    keys.Add(content[i].Replace("\r", ""));
                }
                return keys;
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
                            var itemValue = string.IsNullOrEmpty(currentKey.Value) ? 0 : int.Parse(currentKey.Value);
                            var addItemValue = string.IsNullOrEmpty(baseValue.Value) ? 0 : int.Parse(baseValue.Value);
                            var valuesSum = itemValue + addItemValue;
                            currentKey.Value = valuesSum == 0 ? "" : valuesSum.ToString();
                        }
                    }
                }
            }

            private UserEventApplicationItem ParseToUserEventApplication(string[] userFile, List<string> keys)
            {
                var userItem = new UserEventApplicationItem
                {
                    Name = userFile[0],
                    Email = userFile[1],
                    TranscribedParticipation = userFile[2],
                    GradeBaseValues = new List<BaseValue>()
                };

                for (int i = 0; i < keys.Count; i++)
                {
                    var value = userFile[i + keysStartColumn].Replace("\r", "");
                    value = string.IsNullOrEmpty(value) ? "0" : value;
                    value = value.All(c => char.IsDigit(c) || c == '-' || c == ',' || c == '.') ? value : "0";
                    value = value.Any(c => char.IsDigit(c)) ? value : "0";
                    userItem.GradeBaseValues.Add(new BaseValue
                    {
                        Key = keys[i],
                        Value = value
                    });
                }

                return userItem;
            }

            private async Task<List<UserEventApplicationItem>> FindUsers(List<UserEventApplicationItem> userItems, CancellationToken cancellationToken)
            {
                var names = userItems.Where(x => !string.IsNullOrEmpty(x.Name)).Select(x => x.Name);
                var emails = userItems.Where(x => !string.IsNullOrEmpty(x.Email)).Select(x => x.Email);
                var users = await _db.UserCollection
                    .AsQueryable()
                    .Where(x => names.Contains(x.Name) || emails.Contains(x.Email))
                    .Select(x =>  new UserBase { UserId = x.Id, Email = x.Email, Name = x.Name})
                    .ToListAsync(cancellationToken);

                for (int i = 0; i < userItems.Count; i++)
                {
                    var user = users.FirstOrDefault(x => x.Name == userItems[i].Name || x.Email == userItems[i].Email);
                    if (user != null)
                        userItems[i].UserId = user.UserId;
                }

                return userItems.Where(x => 
                    x.UserId != null && 
                    x.UserId != ObjectId.Empty
                ).ToList();
            }

            private async Task<bool> UpdateUserEventApplicationGrades(List<UserEventApplicationItem> userItems, 
                ObjectId eventId, ObjectId eventScheduleId, CancellationToken cancellationToken)
            {
                var userItemsIds = userItems.Select(x => x.UserId);
                var eventApplications = await _db.EventApplicationCollection
                    .AsQueryable()
                    .Where(x => 
                        x.EventId == eventId &&
                        x.ScheduleId == eventScheduleId &&
                        userItemsIds.Contains(x.UserId)
                    )
                    .ToListAsync(cancellationToken);

                if (eventApplications == null || eventApplications.Count == 0)
                    return false;

                var models = new List<WriteModel<EventApplication>>();
                foreach (EventApplication eventApplication in eventApplications)
                {
                    var user = userItems.FirstOrDefault(x => x.UserId == eventApplication.UserId);
                    if (user != null)
                    {
                        List<UpdateDefinition<EventApplication>> updates = new List<UpdateDefinition<EventApplication>>();

                        if (user.GradeBaseValues != null && user.GradeBaseValues.Count > 0)
                        {
                            eventApplication.GradeBaseValues = new List<BaseValue>();
                            UpdateKeyValues(eventApplication.GradeBaseValues, user.GradeBaseValues);
                            updates.Add(Builders<EventApplication>.Update.Set(x => x.GradeBaseValues, eventApplication.GradeBaseValues));
                        }
                        
                        eventApplication.TranscribedParticipation = user.TranscribedParticipation;
                        updates.Add(Builders<EventApplication>.Update.Set(x => x.TranscribedParticipation, eventApplication.TranscribedParticipation));

                        var combined = Builders<EventApplication>.Update.Combine(updates);

                        models.Add(new UpdateOneModel<EventApplication>(new BsonDocument("_id", eventApplication.Id), combined));
                    }
                }
                await _db.EventApplicationCollection.BulkWriteAsync(models);
                return true;
            }
        }
    }
}
