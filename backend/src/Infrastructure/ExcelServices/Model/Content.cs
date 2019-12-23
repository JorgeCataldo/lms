using System.Collections.Generic;

namespace Infrastructure.ExcelServices.Model
{
    public class Content
    {
        public string ModuloId { get; set; }
        public string AssuntoId { get; set; }
        public string ConteudoId { get; set; }
        public string DescricaoConteudo { get; set; }
        public string Nivel { get; set; }
        public string Conceitos { get; set; }
        public int TempoEstimado { get; set; }
        public string Tipo { get; set; }
        public string Link { get;set; }
        public List<string> ConceitosLista { get; set; }
        
        public Content()
        {
            ConceitosLista = new List<string>();
        }
    }
}