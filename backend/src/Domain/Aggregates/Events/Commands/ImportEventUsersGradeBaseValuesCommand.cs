using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;
using Domain.Base;
using System;
using System.IO;
using OfficeOpenXml;

namespace Domain.Aggregates.Events.Commands
{
    public class ImportEventUsersGradeBaseValuesCommand
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
            private readonly int keysStartColumn = 4;

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

                //var list = ReadAsList(request.FileContent);
                //if (list == null)
                //    return Result.Fail<Contract>("Lista nula");

                var eventId = ObjectId.Parse(request.EventId);
                var eventScheduleId = ObjectId.Parse(request.EventScheduleId);

                var buildedFile = BuildFile(request.FileContent);

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

            private List<UserEventApplicationItem> BuildFile(string file)
            {
                List<UserEventApplicationItem> usersFile = new List<UserEventApplicationItem>();
                List<string> keys = new List<string>();

                file = file.Substring(file.IndexOf(",", StringComparison.Ordinal) + 1);
                byte[] bytes = Convert.FromBase64String(file);
                using (MemoryStream stream = new MemoryStream(bytes))
                {
                    using (var xlPackage = new ExcelPackage(stream))
                    {
                        var baseWorksheet = xlPackage.Workbook.Worksheets[0];
                        var currentLine = 2;


                        while (baseWorksheet.Cells[currentLine, 3].Value != null && baseWorksheet.Cells[currentLine, 2].Value != null)
                        {
                            /*
                               Id	
                               Nome	
                               Email
                               Data_Realização	
                               Case	Grupo	
                               Nota_QC	
                               Nota_TG	
                               Nota_FA	
                               Média_Autoavaliação	
                               Média_Nota_QC	
                               Média_Nota_TG	
                               Média_Nota_FA	
                               Média_Grupo	
                               Nota_Professor_Grupo	
                               Média_Final_Case	
                               Destaque_Justificativa                      
                             */

                            var user = new UserEventApplicationItem();
                            user.UserId = baseWorksheet.Cells[currentLine, 1].Value != null 
                                ? ObjectId.Parse(baseWorksheet.Cells[currentLine, 1].Value?.ToString())
                                : ObjectId.Empty;
                            user.Name = baseWorksheet.Cells[currentLine, 2].Value?.ToString();
                            user.Email = baseWorksheet.Cells[currentLine, 3].Value?.ToString();

                            var baseGrades = new List<BaseValue>();
                                                       
                            var QcGrade = baseWorksheet.Cells[currentLine, 11].Value?.ToString();
                            var TgGrade = baseWorksheet.Cells[currentLine, 12].Value?.ToString();
                            var FaGrade = baseWorksheet.Cells[currentLine, 13].Value?.ToString();
                            var finalGrade = baseWorksheet.Cells[currentLine, 16].Value?.ToString();
                            var featuredStudent = baseWorksheet.Cells[currentLine, 17].Value?.ToString();

                            baseGrades.Add(new BaseValue()
                            {
                                Key = "QC",
                                Value = QcGrade != null && decimal.TryParse(QcGrade, out decimal QC) ? QC.ToString() : "0"
                            });

                            baseGrades.Add(new BaseValue()
                            {
                                Key = "TG",
                                Value = TgGrade != null && decimal.TryParse(TgGrade, out decimal TG) ? TG.ToString() : "0"
                            });

                            baseGrades.Add(new BaseValue()
                            {
                                Key = "FA",
                                Value = FaGrade != null && decimal.TryParse(FaGrade, out decimal FA) ? FA.ToString() : "0"
                            });

                            baseGrades.Add(new BaseValue()
                            {
                                Key = "final_grade",
                                Value = finalGrade != null && decimal.TryParse(finalGrade, out decimal final) ? final.ToString() : "0"
                            });

                            baseGrades.Add(new BaseValue()
                            {
                                Key = "featured_student",
                                Value = featuredStudent
                            });

                            user.GradeBaseValues = baseGrades;
                            usersFile.Add(user);

                            currentLine++;
                        }

                    }
                }

                //for (int i = 0; i < fileList.Count; i++)
                //{
                //    var splittedRow = fileList[i].Split(';');
                //    if (i == 0)
                //    {
                //        keys = SetKeys(splittedRow);
                //    }
                //    else
                //    {
                //        if (!string.IsNullOrEmpty(splittedRow[0]) && splittedRow[0].Length > 3)
                //        {
                //            var parsedUser = ParseToUserEventApplication(splittedRow, keys);
                //            var listUser = usersFile.FirstOrDefault(x => x.Name == splittedRow[0]);
                //            if (listUser == null)
                //            {
                //                usersFile.Add(parsedUser);
                //            }
                //            else
                //            {
                //                UpdateKeyValues(listUser.GradeBaseValues, parsedUser.GradeBaseValues);
                //                if (!string.IsNullOrEmpty(parsedUser.TranscribedParticipation))
                //                {
                //                    listUser.TranscribedParticipation = listUser.TranscribedParticipation + ";" +
                //                        parsedUser.TranscribedParticipation;
                //                }
                //            }
                //        }
                //    }
                //}
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
                var untrackedUsersEmails = userItems.Where(x => x.UserId == ObjectId.Empty).Select(x => x.Email).ToList();
                var users = await _db.UserCollection
                    .AsQueryable()
                    .Where(x => untrackedUsersEmails.Contains(x.Email))
                    .Select(x =>  new UserBase { UserId = x.Id, Email = x.Email, Name = x.Name})
                    .ToListAsync(cancellationToken);


                for (int i = 0; i < userItems.Count; i++)
                {
                    var user = users.FirstOrDefault(x => x.Email == userItems[i].Email);
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

                var eventApplicationsUserIds = eventApplications.Select(x => x.UserId).ToList();
                var usersNotApplyed = userItemsIds.Where(x => !eventApplicationsUserIds.Contains(x)).ToList();


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

                //var newEventApplications = new List<EventApplication>();
                //foreach (ObjectId userNotApplyed in usersNotApplyed)
                //{
                //    var user = await _db.UserCollection
                //        .AsQueryable()
                //        .Where(x =>
                //            x.Id == userNotApplyed
                //        )
                //        .ToListAsync(cancellationToken);

                //    if (user != null)
                //    {
                //        newEventApplications.Add(new EventApplication {
                //            UserId = userNotApplyed,
                //            ApplicationStatus = ApplicationStatus.Approved,
                //            EventId = eventId,
                //            ScheduleId = eventScheduleId
                //        });
                //    }
                //}

                await _db.EventApplicationCollection.BulkWriteAsync(models);
                //await _db.EventApplicationCollection.BulkWriteAsync(newEventApplications.Select(d => new InsertOneModel<EventApplication>(d)));

                var evt = await (await _db
                    .Database
                    .GetCollection<Event>("Events")
                    .FindAsync(x => x.Id == eventId, cancellationToken: cancellationToken))
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                if (evt == null)
                    return false;

                var schedule = evt.Schedules.FirstOrDefault(x => x.Id == eventScheduleId);
                if (schedule == null)
                    return false;

                for (int i = 0; i < usersNotApplyed.Count; i++)
                {
                    var user = userItems.FirstOrDefault(x => x.UserId == usersNotApplyed[i]);

                    await _db.EventApplicationCollection.InsertOneAsync(new EventApplication()
                    {
                        ApplicationStatus = ApplicationStatus.Approved,
                        CreatedAt = DateTimeOffset.UtcNow,
                        EventId = eventId,
                        EventDate = schedule.EventDate,
                        UserId = usersNotApplyed[i],
                        ScheduleId = eventScheduleId,
                        PrepQuiz = null,
                        PrepQuizAnswers = null,
                        PrepQuizAnswersList = null,
                        RequestedDate = DateTimeOffset.UtcNow,
                        ResolutionDate = null,
                        GradeBaseValues = user.GradeBaseValues
                    }, cancellationToken: cancellationToken);
                }
                
                return true;
            }
        }
    }
}
