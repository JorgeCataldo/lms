using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Questions;
using Domain.Aggregates.Questions.Commands;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;
using OfficeOpenXml;
using Domain.Aggregates.Levels;
using MongoDB.Driver.Linq;
using System.Linq;
using Performance.Domain.Aggregates.AuditLogs;
using Newtonsoft.Json;
using Domain.Aggregates.ModulesDrafts;
using Domain.Aggregates.Modules;

namespace Domain.Aggregates.QuestionsDraft.Commands
{
    public class ImportDraftQdb
    {
        public class Contract : CommandContract<Result>
        {
            public string File { get; set; }
            public string ModuleId { get; set; }
            public bool AddQuestions { get; set; } = true;
            public string UserId { get; set; }
            public string UserRole { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result>
        {
            private readonly IDbContext _db;
            private readonly IMediator _mediator;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
                _mediator = mediator;
            }

            public async Task<Result> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole == "Secretary")
                    return Result.Fail("Acesso Negado");

                var moduleId = ObjectId.Parse(request.ModuleId);
                var module = await _db.ModuleDraftCollection.AsQueryable()
                    .Where(x => (
                            x.DeletedAt == null || x.DeletedAt == DateTimeOffset.MinValue
                        ) && !x.DraftPublished && (
                            x.Id == moduleId || x.ModuleId == moduleId
                        )
                    )
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                if (module == null)
                {
                    var originalModule = await GetModule(request.ModuleId);
                    if (originalModule == null)
                        return Result.Fail("Módulo não existe");

                    module = await CreateModuleDraft(
                        request, originalModule, cancellationToken
                    );
                }

                var subjectConceptDictionary = new Dictionary<string, List<string>>();
                foreach (var subject in module.Subjects)
                {
                    var subjectConcepts = subject.Concepts.Select(x => x.Name).ToList();
                    subjectConceptDictionary.Add(subject.Id.ToString(), subjectConcepts);
                }

                var questions = new List<QuestionDraft>();

                request.File = request.File.Substring(request.File.IndexOf(",", StringComparison.Ordinal) + 1);
                Byte[] bytes = Convert.FromBase64String(request.File);
                using (Stream stream = new MemoryStream(bytes))
                {
                    using (var xlPackage = new ExcelPackage(stream))
                    {
                        var baseWorksheet = xlPackage.Workbook.Worksheets[0];
                        var currentLine = 2;

                        while (baseWorksheet.Cells[currentLine, 1].Value != null)
                        {
                            /*
                                IDExterno Assunto
                                ID Nível
                                Descrição Questão
                                Tempo médio de Resposta em minutos
                                Resposta 1
                                Valor Resposta 1
                                Conceitos certos Resposta 1
                                Resposta 2
                                Valor Resposta 2
                                Conceitos certos Resposta 2
                                Resposta 3
                                Valor Resposta 3
                                Conceitos certos Resposta 3
                                Resposta 4
                                Valor Resposta 4
                                Conceitos certos Resposta 4
                                Resposta 5
                                Valor Resposta 5
                                Conceitos certos Resposta 5                                   
                                */

                            var subjectId = baseWorksheet.Cells[currentLine, 1].Value.ToString();

                            if (!subjectConceptDictionary.ContainsKey(subjectId))
                                return Result.Fail<bool>("Assunto não encontrado");
                            var subjectConcepts =
                                subjectConceptDictionary[subjectId];

                            var quest = new AddOrModifyQuestionCommand.Contract();
                            if (!int.TryParse(baseWorksheet.Cells[currentLine, 2].Value.ToString(), out int level))
                            {
                                var availableLevels = Level.GetLevels().Data.Select(x => x.Id.ToString())
                                    .Aggregate((x, y) => x + "," + y);
                                return Result.Fail<bool>(
                                    $"Não foi possível definir o nível de dificuldade da pergunta da linha {currentLine}. Valores possíveis são ({availableLevels})");
                            }
                            quest.Level = level;

                            quest.Text = baseWorksheet.Cells[currentLine, 3].Value?.ToString();
                            if (quest.Text == null || quest.Text.Length < 10)
                                return Result.Fail<bool>(
                                    $"Texto da pergunta da linha {currentLine} não pode ser menor que 10 caracteres");

                            var durationStr = baseWorksheet.Cells[currentLine, 4].Value?.ToString();
                            if (durationStr != null && int.TryParse(durationStr, out int duration))
                            {
                                quest.Duration = duration;
                            }

                            if (baseWorksheet.Cells[currentLine, 5].Value != null)
                            {
                                var answer1 =
                                    this.getAnswerFromWorksheet(baseWorksheet, currentLine, 5, "Resposta 1");
                                if (answer1.IsFailure)
                                    return Result.Fail<bool>(answer1.Error);
                                quest.Answers.Add(answer1.Data);
                            }

                            if (baseWorksheet.Cells[currentLine, 8].Value != null)
                            {
                                var answer2 =
                                    this.getAnswerFromWorksheet(baseWorksheet, currentLine, 8, "Resposta 2");
                                if (answer2.IsFailure)
                                    return Result.Fail<bool>(answer2.Error);
                                quest.Answers.Add(answer2.Data);
                            }

                            if (baseWorksheet.Cells[currentLine, 11].Value != null)
                            {
                                var answer3 =
                                    this.getAnswerFromWorksheet(baseWorksheet, currentLine, 11, "Resposta 3");
                                if (answer3.IsFailure)
                                    return Result.Fail<bool>(answer3.Error);
                                quest.Answers.Add(answer3.Data);
                            }

                            if (baseWorksheet.Cells[currentLine, 14].Value != null)
                            {
                                var answer4 =
                                    this.getAnswerFromWorksheet(baseWorksheet, currentLine, 14, "Resposta 4");
                                if (answer4.IsFailure)
                                    return Result.Fail<bool>(answer4.Error);
                                quest.Answers.Add(answer4.Data);
                            }

                            if (baseWorksheet.Cells[currentLine, 17].Value != null)
                            {
                                var answer5 =
                                    this.getAnswerFromWorksheet(baseWorksheet, currentLine, 17, "Resposta 5");
                                if (answer5.IsFailure)
                                    return Result.Fail<bool>(answer5.Error);
                                quest.Answers.Add(answer5.Data);
                            }

                            var concepts = quest.Answers.SelectMany(x => x.Concepts).Select(x => x.Concept).Distinct().ToList();
                            quest.Concepts = concepts.ToArray();

                            var notContainedConcepts = concepts.Where(x => !subjectConcepts.Contains(x)).ToList();
                            if (notContainedConcepts.Any())
                                return Result.Fail<bool>($"Os conceitos ({notContainedConcepts.Aggregate((x, y) => x + "," + y)}) das repostas da linha {currentLine} não estão contidos no assunto");

                            // Colocando os conceitos errados da pergunta
                            foreach (var answer in quest.Answers)
                            {
                                var answerConcepts = answer.Concepts.Select(x => x.Concept);
                                var notPresentConcepts = concepts.Where(x => !answerConcepts.Contains(x)).Select(
                                    x => new AddOrModifyQuestionCommand.ContractAnswerConcept()
                                    {
                                        Concept = x.Trim(),
                                        IsRight = false
                                    });
                                answer.Concepts.AddRange(notPresentConcepts);
                            }

                            var answers = quest.Answers.Select(x => new Answer(x.Description, x.Points,
                                x.Concepts.Select(y => new AnswerConcept(y.Concept, y.IsRight)).ToList())).ToList();

                            var newObject = QuestionDraft.Create(
                                module.Id, quest.Text, quest.Level,
                                quest.Duration, quest.Concepts, answers,
                                ObjectId.Parse(request.ModuleId), ObjectId.Parse(subjectId)
                            );

                            if (newObject.IsFailure)
                                return Result.Fail<bool>($"Erro na linha {currentLine}: {newObject.Error}");

                            questions.Add(newObject.Data);

                            currentLine++;
                        }

                    }
                }

                if (!request.AddQuestions)
                {
                    var baseQuestionDraftCollection = await _db.QuestionDraftCollection
                        .AsQueryable()
                        .Where(x =>
                            x.DraftId == module.Id
                        )
                        .ToListAsync();

                    await _db.QuestionDraftCollection.DeleteManyAsync(x =>
                        x.DraftId == module.Id,
                        cancellationToken: cancellationToken
                    );

                    var deleteLog = AuditLog.Create(ObjectId.Parse(request.UserId), moduleId, baseQuestionDraftCollection.GetType().ToString(),
                    JsonConvert.SerializeObject(baseQuestionDraftCollection), EntityAction.Delete);

                    await _db.AuditLogCollection.InsertOneAsync(deleteLog);
                }

                await _db.QuestionDraftCollection.InsertManyAsync(
                    questions, cancellationToken: cancellationToken
                );

                var creationLog = AuditLog.Create(ObjectId.Parse(request.UserId), moduleId, questions[0].GetType().ToString(), 
                    JsonConvert.SerializeObject(questions), EntityAction.Add);

                await _db.AuditLogCollection.InsertOneAsync(creationLog);

                return Result.Ok();
            }

            private Result<AddOrModifyQuestionCommand.ContractAnswer> getAnswerFromWorksheet(ExcelWorksheet baseWorksheet, int currentLine, int answerCol, string resposta)
            {
                var answerText = baseWorksheet.Cells[currentLine, answerCol].Value?.ToString();
                var answerVal = baseWorksheet.Cells[currentLine, answerCol + 1].Value?.ToString();
                var answerConcepts = baseWorksheet.Cells[currentLine, answerCol + 2].Value?.ToString();

                if (string.IsNullOrEmpty(answerText))
                    return Result.Fail<AddOrModifyQuestionCommand.ContractAnswer>(
                        $"Texto da {resposta} da linha {currentLine} não pode ser vazio");
                if (answerVal == null || !int.TryParse(answerVal, out int val))
                    return Result.Fail<AddOrModifyQuestionCommand.ContractAnswer>(
                        $"Valor da {resposta} da linha {currentLine} está inválido");

                var answer = new AddOrModifyQuestionCommand.ContractAnswer()
                {
                    Description = answerText,
                    Points = val
                };
                if (!string.IsNullOrEmpty(answerConcepts))
                {
                    answer.Concepts = answerConcepts.Split(",").Select(x =>
                        new AddOrModifyQuestionCommand.ContractAnswerConcept()
                        {
                            Concept = x.Trim(),
                            IsRight = true
                        }).ToList();
                }

                return Result.Ok(answer);
            }

            private async Task<ModuleDraft> CreateModuleDraft(
                Contract request, Module originalModule, CancellationToken token
            ) {
                var moduleId = ObjectId.Parse(request.ModuleId);

                var module = ModuleDraft.Create(
                    moduleId, originalModule.Title, originalModule.Excerpt, originalModule.ImageUrl, originalModule.Published,
                    originalModule.InstructorId, originalModule.Instructor, originalModule.InstructorMiniBio, originalModule.InstructorImageUrl,
                    originalModule.Duration, originalModule.VideoDuration, originalModule.VideoUrl,
                    originalModule.CertificateUrl, originalModule.StoreUrl, originalModule.EcommerceUrl, false,
                    originalModule.Tags, originalModule.TutorsIds
                ).Data;

                module.Subjects = originalModule.Subjects;
                module.Requirements = originalModule.Requirements;
                module.SupportMaterials = originalModule.SupportMaterials;

                await _db.ModuleDraftCollection.InsertOneAsync(
                    module, cancellationToken: token
                );

                var dbModulesQuestionsQuery = _db.QuestionCollection
                    .AsQueryable()
                    .Where(q => q.ModuleId == moduleId);

                var dbModuleQuestions = await (
                    (IAsyncCursorSource<Question>)dbModulesQuestionsQuery
                ).ToListAsync();

                if (dbModuleQuestions.Count > 0)
                {
                    var draftQuestions = dbModuleQuestions.Select(q =>
                        QuestionDraft.Create(
                            q.Id, module.Id,
                            q.Text, q.Level, q.Duration,
                            q.Concepts, q.Answers,
                            q.ModuleId, q.SubjectId
                        ).Data
                    );

                    await _db.QuestionDraftCollection.InsertManyAsync(
                        draftQuestions, cancellationToken: token
                    );
                }

                return module;
            }

            private async Task<Module> GetModule(string moduleId)
            {
                var dbModuleId = ObjectId.Parse(moduleId);

                return await _db.ModuleCollection.AsQueryable()
                    .Where(mod => mod.Id == dbModuleId)
                    .FirstOrDefaultAsync();
            }
        }
    }
}
