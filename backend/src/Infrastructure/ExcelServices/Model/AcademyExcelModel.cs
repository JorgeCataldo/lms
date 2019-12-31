using System.Collections.Generic;

namespace Infrastructure.ExcelServices.Model
{
    public class AcademyExcelModel
    {
        public List<Dificulty> NivelDificuldade { get; set; }
        public List<Modules> Modulos { get; set; }
        public List<Subject> Assuntos { get; set; }
        public List<Content> Conteudos { get; set; }
        public List<Question> Questoes { get; set; }
    }
}
