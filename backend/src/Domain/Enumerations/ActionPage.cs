using System.Collections.Generic;
using Domain.SeedWork;
using Tg4.Infrastructure.Functional;

namespace Domain.Enumerations
{
    public class ActionPage : Enumeration
    {
        public static readonly ActionPage Module = new ActionPage(1, "Module");
        public static readonly ActionPage Content= new ActionPage(2, "Content");
        public static readonly ActionPage Exam = new ActionPage(3, "Exam");
        public static readonly ActionPage Event = new ActionPage(4, "Event");
        public static readonly ActionPage Subject = new ActionPage(5, "Subject");

        private ActionPage(int id, string type) : base(id, type) { }

        public static Result<ActionPage> Create(int id)
        {
            return List.ContainsKey(id) ? Result.Ok(List[id]) : Result.Fail<ActionPage>("Página inexistente.");
        }

        public static readonly IDictionary<int, ActionPage> List = new Dictionary<int, ActionPage> {
            { Module.Id, Module },
            { Content.Id, Content },
            { Exam.Id, Exam },
            { Event.Id, Event },
            { Subject.Id, Subject }
        };
    }
}
