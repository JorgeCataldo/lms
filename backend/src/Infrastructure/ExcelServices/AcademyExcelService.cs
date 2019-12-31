using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.ExcelServices.Extensions;
using Infrastructure.ExcelServices.Model;
using OfficeOpenXml;

namespace Infrastructure.ExcelServices
{
    public class AcademyExcelService
    {

        public AcademyExcelService()
        {
        }

        public AcademyExcelModel GenericWorkSheetData(Stream file)
        {
            using (var xlPackage = new ExcelPackage(file))
            {
                var baseWorksheet1 = xlPackage.Workbook.Worksheets[0];
                var baseWorksheet2 = xlPackage.Workbook.Worksheets[1];
                var baseWorksheet3 = xlPackage.Workbook.Worksheets[2];
                var baseWorksheet4 = xlPackage.Workbook.Worksheets[3];
                var baseWorksheet5 = xlPackage.Workbook.Worksheets[4];

                var listDificulty = baseWorksheet1.ConvertSheetToObjects<Dificulty>().ToList();
                var listModulo = baseWorksheet2.ConvertSheetToObjects<Modules>().ToList();
                var listAssunto = baseWorksheet3.ConvertSheetToObjects<Subject>().ToList();
                var listConteudo = baseWorksheet4.ConvertSheetToObjects<Content>().ToList();
                //var listQuestoes = baseWorksheet5.ConvertSheetToObjects<Question>().ToList();

                var model = new AcademyExcelModel()
                {
                    Assuntos = listAssunto,
                    Conteudos = listConteudo,
                    Modulos = listModulo,
                    NivelDificuldade = listDificulty,
                    //Questoes = listQuestoes
                };

                try
                {
                    foreach (var mod in model.Modulos)
                    {
                        mod.DescricaoModulo = this.ToCamelCase(mod.DescricaoModulo);
                    }
                    foreach (var assunto in listAssunto)
                    {
                        assunto.DescricaoAssunto = this.ToCamelCase(assunto.DescricaoAssunto);
                        assunto.ConceitosLista = string.IsNullOrWhiteSpace(assunto.Conceitos)
                            ? new List<string>()
                            : assunto.Conceitos.Split(',')
                            .Select(x => this.ToCamelCase(x.Replace(".", "").Trim())).ToList();

                        foreach (var conteudo in listConteudo)
                        {
                            if (conteudo.AssuntoId != assunto.AssuntoId || conteudo.ModuloId != assunto.ModuloId) continue;
                            
                            conteudo.DescricaoConteudo = this.ToCamelCase(conteudo.DescricaoConteudo);
                            assunto.Content.Add(conteudo);
                            conteudo.ConceitosLista = string.IsNullOrWhiteSpace(conteudo.Conceitos)
                                ? new List<string>()
                                : conteudo.Conceitos.Split(',')
                                    .Select(x => this.ToCamelCase(x.Replace(".", "").Trim())).ToList();
                        }

                        //foreach (var questao in listQuestoes)
                        //{
                        //    if (questao.AssuntoId == assunto.AssuntoId)
                        //    {
                        //        assunto.Questoes.Add(questao);
                        //        foreach (var resposta in questao.Respostas)
                        //        {
                        //            resposta.ConceitosCertos = resposta.ConceitosCertos
                        //                .Select(x => this.ToCamelCase(x.Trim())).ToArray();
                        //        }
                        //    }
                        //}
                    }

                    foreach (var modulos in listModulo)
                    {
                        foreach (var assuntos in listAssunto)
                        {
                            if (assuntos.ModuloId == modulos.ModuloId)
                                modulos.Assuntos.Add(assuntos);
                        }
                    }


                    return model;
                }
                catch (Exception err)
                {
                    Console.Write(err.Message);
                    throw;
                }
            }
        }

        private string ToCamelCase(string title)
        {
            TextInfo textInfo = new CultureInfo("pt-BR", false).TextInfo;

            title = textInfo.ToTitleCase(title.ToLower()); 
            return title ;
        }
    }
}
