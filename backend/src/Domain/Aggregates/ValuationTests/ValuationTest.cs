using Domain.Enumerations;
using Domain.SeedWork;
using MongoDB.Bson;
using System.Collections.Generic;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.ValuationTests
{
    public class ValuationTest : Entity, IAggregateRoot
    {
        public string Title { get; set; }
        public ValuationTestTypeEnum Type { get; set; }
        public List<ObjectId> Questions { get; set; }
        public List<TestModule> TestModules { get; set; }
        public List<TestTrack> TestTracks { get; set; }

        public class TestModule
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public int? Percent { get; set; }
            public ValuationTestModuleTypeEnum? Type { get; set; }
        }

        public class TestTrack
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public int? Percent { get; set; }
            public int? Order { get; set; }
        }

        public static Result<ValuationTest> Create(
            ObjectId userId, string title, ValuationTestTypeEnum type,
            List<ObjectId> questions, List<TestModule> modules, List<TestTrack> tracks
        ) {

            var module = new ValuationTest()
            {
                Id = ObjectId.GenerateNewId(),
                CreatedBy = userId,
                Title = title,
                Type = type,
                Questions = questions,
                TestModules = modules,
                TestTracks = tracks
            };

            return Result.Ok(module);
        }

        private ValuationTest() : base()
        {
            Questions = new List<ObjectId>();
        }
    }
}