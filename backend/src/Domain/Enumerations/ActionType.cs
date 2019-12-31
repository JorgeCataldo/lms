using System.Collections.Generic;
using Domain.SeedWork;
using Tg4.Infrastructure.Functional;

namespace Domain.Enumerations
{
    public class ActionType : Enumeration
    {
        public static readonly ActionType Click = new ActionType(1, "Click");
        public static readonly ActionType Access = new ActionType(2, "Access");
        public static readonly ActionType Finish = new ActionType(3, "Finish");
        public static readonly ActionType CloseTab = new ActionType(4, "CloseTab");
        public static readonly ActionType VideoPlay = new ActionType(5, "VideoPlay");
        public static readonly ActionType LevelUp = new ActionType(6, "LevelUp");

        private ActionType(int id, string type) : base(id, type) { }

        public static Result<ActionType> Create(int id)
        {
            return List.ContainsKey(id) ? Result.Ok(List[id]) : Result.Fail<ActionType>("Tipo de Ação inexistente.");
        }

        public static readonly IDictionary<int, ActionType> List = new Dictionary<int, ActionType> {
            { Click.Id, Click },
            { Access.Id, Access },
            { Finish.Id, Finish },
            { CloseTab.Id, CloseTab },
            { VideoPlay.Id, VideoPlay },
            { LevelUp.Id, LevelUp }
        };
    }
}
