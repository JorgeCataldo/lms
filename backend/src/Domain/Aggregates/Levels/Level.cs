using System.Collections.Generic;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Levels
{
    public class Level
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string BadgeUrl { get; set; }

        private Level(int id, string description, string badgeUrl)
        {
            Id = id;
            Description = description;
            BadgeUrl = badgeUrl;
        }

        public static Result<List<Level>> GetLevels()
        {
            return Result.Ok(
                new List<Level>() {
                    new Level(0, "Iniciante", "level0.png"),
                    new Level(1, "Intermediário", "level1.png"),
                    new Level(2, "Avançado", "level2.png")
                }
            );
        }

        public static Result<List<Level>> GetAllLevels()
        { 
            return Result.Ok(
                new List<Level>() {
                    new Level(0, "Iniciante", "level0.png"),
                    new Level(1, "Intermediário", "level1.png"),
                    new Level(2, "Avançado", "level2.png"),
                    new Level(3, "Expert", "level3.png")
                }
            );
        }
    }
}
