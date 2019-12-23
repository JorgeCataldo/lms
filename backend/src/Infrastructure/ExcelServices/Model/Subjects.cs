using System.Collections.Generic;

namespace Infrastructure.ExcelServices.Model
{
    public class Subject
    {
        public string ModuloId { get; set; }
        public string AssuntoId { get; set; }
        public string DescricaoAssunto { get; set; }
        public string ObjetivoPedagogico { get; set; }
        public string Conceitos { get; set; }
        public List<Question> Questoes { get; set; }
        public List<Content> Content { get; set; }
        public List<string> ConceitosLista { get; set; }

        public Subject()
        {
            Questoes = new List<Question>();
            Content = new List<Content>();
            ConceitosLista = new List<string>();
        }
    }
}