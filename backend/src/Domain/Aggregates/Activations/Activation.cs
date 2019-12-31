using Domain.Extensions;
using Domain.SeedWork;
using MongoDB.Bson;
using Newtonsoft.Json;
using System.Collections.Generic;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.Users.User;

namespace Domain.Aggregates.Activations
{
    public class Activation
    {
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId Id { get; set; }
        public ActivationTypeEnum Type { get; set; }
        public bool Active { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public decimal? Percentage { get; set; }

        public static Result<Activation> Create(
            bool active = false, ActivationTypeEnum type = ActivationTypeEnum.Custom, 
            string title = "", string text = "", decimal percentage = 100
        )
        {
            return Result.Ok(
                new Activation()
                {
                    Type = type,
                    Active = active,
                    Title = title,
                    Text = text,
                    Percentage = percentage
                }
            );
        }
    }

    public enum ActivationTypeEnum
    {
        Nps = 1,
        Custom = 2
    }
}
