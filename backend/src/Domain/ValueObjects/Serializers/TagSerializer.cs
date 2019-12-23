using MongoDB.Bson.Serialization.Serializers;

namespace Domain.ValueObjects.Serializers
{
    public class TagSerializer : SerializerBase<Tag>
    {
        public override void Serialize(MongoDB.Bson.Serialization.BsonSerializationContext context,
                                       MongoDB.Bson.Serialization.BsonSerializationArgs args, Tag value)
        {
            context.Writer.WriteString(string.IsNullOrWhiteSpace(value.Name) ? "" : value.Name);
        }

        public override Tag Deserialize(MongoDB.Bson.Serialization.BsonDeserializationContext context,
            MongoDB.Bson.Serialization.BsonDeserializationArgs args)
        {
            var value = context.Reader.ReadString();
            return Tag.Load(value);
        }
    }
}