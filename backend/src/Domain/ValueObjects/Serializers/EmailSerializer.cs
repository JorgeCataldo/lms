using MongoDB.Bson.Serialization.Serializers;

namespace Domain.ValueObjects.Serializers
{
    public class EmailSerializer: SerializerBase<Email>
    {
        public override void Serialize(MongoDB.Bson.Serialization.BsonSerializationContext context,
            MongoDB.Bson.Serialization.BsonSerializationArgs args, Email value)
        {
            context.Writer.WriteString(string.IsNullOrWhiteSpace(value.Value) ? "" : value.Value);
        }

        public override Email Deserialize(MongoDB.Bson.Serialization.BsonDeserializationContext context,
            MongoDB.Bson.Serialization.BsonDeserializationArgs args)
        {
            var emailValue = context.Reader.ReadString();
            return Email.Load(emailValue);
        }
    }
}