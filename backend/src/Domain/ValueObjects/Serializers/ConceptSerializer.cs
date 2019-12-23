using MongoDB.Bson.Serialization.Serializers;

namespace Domain.ValueObjects.Serializers
{
    public class ConceptSerializer : SerializerBase<Concept>
    {
        public override void Serialize(MongoDB.Bson.Serialization.BsonSerializationContext context,
                                       MongoDB.Bson.Serialization.BsonSerializationArgs args, Concept value)
        {
            context.Writer.WriteString(string.IsNullOrWhiteSpace(value.Name) ? "" : value.Name);
        }

        public override Concept Deserialize(MongoDB.Bson.Serialization.BsonDeserializationContext context,
            MongoDB.Bson.Serialization.BsonDeserializationArgs args)
        {
            var value = context.Reader.ReadString();
            return Concept.Load(value);
        }
    }
}