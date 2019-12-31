using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Extensions
{
    public class ObjectIdConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value.GetType().IsArray)
            {
                writer.WriteStartArray();
                foreach (var item in (Array)value)
                {
                    serializer.Serialize(writer, item);
                }
                writer.WriteEndArray();
            }
            else
                serializer.Serialize(writer, value.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);

            if (this.CanConvert(token.GetType()))
            {
                var objectIds = new List<ObjectId>();

                if (token.Type == JTokenType.Array)
                {
                    foreach (var item in token.ToObject<string[]>())
                    {
                        objectIds.Add(new ObjectId(item));
                    }
                    return objectIds;
                }

                if (token.ToObject<string>().Equals("MongoDB.Bson.ObjectId[]"))
                {
                    return objectIds;
                }
                else
                    return new ObjectId(token.ToObject<string>());
            }

            return new ObjectId("000000000000000000000000");
        }

        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(ObjectId));
        }
    }
}
