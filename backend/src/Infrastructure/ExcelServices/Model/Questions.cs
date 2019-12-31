using System.Collections.Generic;

namespace Infrastructure.ExcelServices.Model
{
    public class Question
    {
        public double QuestaoId { get; set; }
        public string ModuloId { get; set; }
        public string AssuntoId { get; set; }
        public string NivelId { get; set; }
        public string DescricaoQuestao { get; set; }
        public double TempoResposta { get; set; }
        public List<Respostas> Respostas { get; set; }
    }

    public class Respostas
    {
        public string Resposta { get; set; }
        public double ValorResposta { get; set; }
        public string[] ConceitosCertos { get; set; }
    }
}