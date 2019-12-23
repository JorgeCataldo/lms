using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;

namespace Domain.ValueObjects.Serializers
{
    public class DurationSerializer : SerializerBase<Duration>
    {
        public override void Serialize(MongoDB.Bson.Serialization.BsonSerializationContext context,
                                       MongoDB.Bson.Serialization.BsonSerializationArgs args, Duration value)
        {
            if (value == null)
                context.Writer.WriteNull();
            else
                context.Writer.WriteInt32(value.Minutes);
        }

        public override Duration Deserialize(MongoDB.Bson.Serialization.BsonDeserializationContext context,
            MongoDB.Bson.Serialization.BsonDeserializationArgs args)
        {
            if (context.Reader == null || context.Reader.CurrentBsonType == BsonType.Null)
            {
                context.Reader.ReadNull();
                return Duration.Load(0);
            }
            return Duration.Load( context.Reader.ReadInt32() );
        }
    }
}