using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;

namespace Domain.ValueObjects.Serializers
{
    public class CpfSerializer: SerializerBase<Cpf>
    {
        public override void Serialize(MongoDB.Bson.Serialization.BsonSerializationContext context,
                                       MongoDB.Bson.Serialization.BsonSerializationArgs args, Cpf value)
        {
            context.Writer.WriteString(value == null || string.IsNullOrWhiteSpace(value.Value) ? "" : value.Value);
        }

        public override Cpf Deserialize(MongoDB.Bson.Serialization.BsonDeserializationContext context,
            MongoDB.Bson.Serialization.BsonDeserializationArgs args)
        {
            //if(context.Reader.ReadBsonType() == BsonType.Null) return null;
            var value = context.Reader.ReadString();
            return Cpf.Load(value);
        }
    }
}