using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Questions;
using Domain.Aggregates.Questions.Commands;
using Domain.Data;
using Domain.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;
using OfficeOpenXml;
using Domain.Aggregates.Levels;

namespace Domain.Aggregates.Modules.Commands
{
    public class ImportDraftQdb
    {
        public class Contract : CommandContract<Result<bool>>
        {
            public string File { get; set; }
            public string ModuleId { get; set; }
            public bool AddQuestions { get; set; } = true;
            public string UserId { get; set; }
            public string UserRole { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<bool>>
        {
            private readonly IDbContext _db;
            private readonly IMediator _mediator;
            private readonly IConfiguration _config;

            public Handler(IDbContext db, IMediator mediator, IConfiguration config)
            {
                _db = db;
                _mediator = mediator;
                _config = config;
            }


            public async Task<Result<bool>> Handle(Contract request, CancellationToken cancellationToken)
            {
                try
                {
                    if (request.UserRole == "Secretary")
                        return Result.Fail<bool>("Acesso Negado");

                    var moduleId = ObjectId.Parse(request.ModuleId);
                    var query = await _db.ModuleCollection.FindAsync(x =>
                        x.Id == moduleId,
                        cancellationToken: cancellationToken
                    );

                    var module = await query.FirstOrDefaultAsync(cancellationToken: cancellationToken);
                    if (module == null)
                        return Result.Fail<bool>("Modulo não encontrado");

                    if (request.UserRole == "Student")
                    {
                        var userId = ObjectId.Parse(request.UserId);

                        if (!Module.IsInstructor(module, userId).Data)
                            return Result.Fail<bool>("Acesso Negado");
                    }

                    var subjectConceptDictionary = new Dictionary<string, List<string>>();
                    foreach (var subject in module.Subjects)
                    {
                        var subjectConcepts = subject.Concepts.Select(x => x.Name).ToList();
                        subjectConceptDictionary.Add(subject.Id.ToString(), subjectConcepts);
                    }
                    
                    var questions = new List<Question>();

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
                    
                                if(!subjectConceptDictionary.ContainsKey(subjectId))
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
                                if(quest.Text == null || quest.Text.Length < 10)
                                    return Result.Fail<bool>(
                                        $"Texto da pergunta da linha {currentLine} não pode ser menor que 10 caracteres");

                                var durationStr = baseWorksheet.Cells[currentLine, 4].Value?.ToString();
                                if (durationStr != null && int.TryParse(durationStr, out int duration))
                                {
                                    quest.Duration = duration;
                                }
                                
                                if(baseWorksheet.Cells[currentLine, 5].Value != null)
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
                                if(notContainedConcepts.Any())
                                    return Result.Fail<bool>($"Os conceitos ({notContainedConcepts.Aggregate((x,y)=>x+","+y)}) das repostas da linha {currentLine} não estão contidos no assunto");
                                
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
                
                                var newObject = Question.Create(quest.Text, quest.Level, quest.Duration, quest.Concepts,
                                    answers,
                                    ObjectId.Parse(request.ModuleId), ObjectId.Parse(subjectId));
                                
                                if (newObject.IsFailure)
                                    return Result.Fail<bool>($"Erro na linha {currentLine}: {newObject.Error}");

                                questions.Add(newObject.Data);

                                currentLine++;
                            }

                        }
                    }

                    if (!request.AddQuestions)
                    {
                        await _db.QuestionCollection.DeleteManyAsync(x => x.ModuleId == moduleId,
                            cancellationToken: cancellationToken);
                    }
                    await _db.QuestionCollection.InsertManyAsync(questions, cancellationToken: cancellationToken);

                    return Result.Ok(true);
                }
                catch (Exception err)
                {
                    Console.WriteLine(err.Message);
                    throw;
                }
            }

            private Result<AddOrModifyQuestionCommand.ContractAnswer> getAnswerFromWorksheet(ExcelWorksheet baseWorksheet, int currentLine, int answerCol, string resposta)
            {
                var answerText = baseWorksheet.Cells[currentLine, answerCol].Value?.ToString();
                var answerVal = baseWorksheet.Cells[currentLine, answerCol + 1].Value?.ToString();
                var answerConcepts = baseWorksheet.Cells[currentLine, answerCol + 2].Value?.ToString();
                
                if(string.IsNullOrEmpty(answerText))
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
        }
    }
}
