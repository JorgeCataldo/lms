using System.Collections.Generic;

namespace Infrastructure.ExcelServices.Model
{
    public class Modules
    {
        public string ModuloId { get; set; }
        public string DescricaoModulo { get; set; }
        public string Ementa { get; set; }
        public List<Subject> Assuntos { get; set; }

        public Modules()
        {
            Assuntos = new List<Subject>();
        }
    }
}