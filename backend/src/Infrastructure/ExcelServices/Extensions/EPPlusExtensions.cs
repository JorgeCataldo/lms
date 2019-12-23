using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using Infrastructure.ExcelServices.Model;
using OfficeOpenXml;

namespace Infrastructure.ExcelServices.Extensions
{
    public static class EPPLusExtensions
    {
        public static IEnumerable<T> ConvertSheetToObjects<T>(this ExcelWorksheet worksheet) where T : new()
        {
            //Get the properties of T
            var tprops = (new T())
                .GetType()
                .GetProperties()
                .ToList();

            //Cells only contains references to cells with actual data
            var notFilterGroup = worksheet.Cells
                .Where(c => worksheet.Cells.Value != null)
                .GroupBy(cell => cell.Start.Row)
                .ToList();

            if (notFilterGroup.Count == 0)
                return new List<T>();

            var groups = notFilterGroup.Where(x => x.Count() > 1).ToList();

            //Filtrar a lista gerada pelo worksheet, para pegar apenas linhas válidas
            //tratar worksheet gerado que tenha mais linhas nulas do que dados
            if (string.Equals(typeof(T).Name, "Content"))
            {
                groups = notFilterGroup
                .Where(x => x.Count(y => y.All(z => z.Value != null)) > 3).ToList();
            }

            //Assume the second row represents column data types (big assumption!)
            var columTypes = groups
                .Skip(1)
                .First()
                .Select(rcell => rcell.Value?.GetType() ?? typeof(string))
                .ToList();

            //Assume first row has the column names
            var colNames = groups
                .First()
                .Select((hcell, idx) => new { Name = hcell.Value.ToString(), index = idx })
                .ToList();

            //Everything after the header is data
            var rowvalues = groups
                .Skip(1) //Exclude header
                .Select(cg => cg.Select(c => c.Value).ToList());

            //Create the collection container
            var collection = rowvalues
                .Select(row =>
                {
                    var tnew = new T();


                    for (var i = 0; i < row.Count; i++)
                    {
                        if (row.Count(x => x == null) > 1)
                        {
                            break;
                        }
                        var colName = colNames[i];

                        var celvalue = row[colName.index];
                        var celType = columTypes[colName.index];
                        var tProp = tprops[colName.index];

                        if (string.Equals(tProp.Name, "Respostas"))
                        {
                            var listaResposta = new List<Respostas>();

                            for (var j = i; j + 3 <= row.Count; j = j + 3)
                            {
                                colName = colNames[j];
                                var conceitoString = row[colName.index] == null ? null : (string)row[colName.index + 2];
                                var resposta = new Respostas
                                {
                                    Resposta = row[colName.index] == null ? null : (string)row[colName.index],
                                    ValorResposta = row[colName.index + 1] == null ? 0 : (double)row[colName.index + 1],
                                    ConceitosCertos = conceitoString?.Split(new string[] { ";" }, StringSplitOptions.None)
                                };
                                if (resposta.ConceitosCertos != null && resposta.Resposta != null)
                                    listaResposta.Add(resposta);
                                i = i + 3;
                            }
                            tProp.SetValue(tnew, listaResposta);
                        }
                        else
                        {
                            if (celType == typeof(double))
                            {
                                var unboxedVal = (double)celvalue;
                                if (tProp.PropertyType == typeof(int))
                                    tProp.SetValue(tnew, (int)unboxedVal);
                                else if (tProp.PropertyType == typeof(double))
                                    tProp.SetValue(tnew, unboxedVal);
                                else
                                    throw new NotImplementedException(string.Format("Type '{0}' not implemented yet!", tProp.PropertyType.Name));
                            }
                            else
                            {
                                tProp.SetValue(tnew, celvalue);
                            }
                        }
                    }
                    return tnew;
                });


            //Send it back
            return collection;
        }

        public static byte[] ToExcel<T>(this IEnumerable<T> objList, string sheetName)
        {
            using (var mem = new MemoryStream())
            {
                ToExcel(objList, mem, sheetName);
                return mem.ToArray();
            }
        }

        public static void ToExcel<T>(this IEnumerable<T> objList, Stream stream, string sheetName)
        {
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            DataTable dataTable = new DataTable("Exportacao");
            foreach (PropertyInfo prop in Props)
                dataTable.Columns.Add(prop.Name);
            foreach (object item in objList)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            using (var pck = new ExcelPackage(stream))
            {
                var ws = pck.Workbook.Worksheets.Add(sheetName);
                ws.Cells["A1"].LoadFromDataTable(dataTable, true);
                pck.Save();
            }
        }
    }
}
