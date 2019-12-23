using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Aggregates.Questions
{
    public class AnswerConcept
    {
        public AnswerConcept(string concept, bool isRight)
        {
            Concept = concept;
            IsRight = isRight;
        }

        public string Concept { get; set; }
        public bool IsRight { get; set; }
    }
}
