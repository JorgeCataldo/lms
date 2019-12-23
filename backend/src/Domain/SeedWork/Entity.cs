using System;
using Domain.Extensions;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace Domain.SeedWork
{
    public abstract class Entity
    {
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset DeletedAt { get; set; }

        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId CreatedBy { get; set; }
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId UpdatedBy { get; set; }
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId DeletedBy { get; set; }

        private int? _requestedHashCode;

        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId Id { get; protected set; }

        protected Entity()
        {
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public bool IsTransient()
        {
            return Id == default;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Entity))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (GetType() != obj.GetType())
                return false;
            var item = (Entity)obj;
            if (item.IsTransient() || IsTransient())
                return false;
            return item.Id == Id;
        }

        public override int GetHashCode()
        {
            // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
            if (IsTransient()) return base.GetHashCode();
            
            if (!_requestedHashCode.HasValue)
                _requestedHashCode = Id.GetHashCode() ^ 31;
            // XOR for random distribution. See:
            // http://blogs.msdn.com/b/ericlippert/archive/2011/02/28/guidelines-and-rules-for-gethashcode.aspx
            return _requestedHashCode.Value;

        }
        public static bool operator ==(Entity left, Entity right)
        {
            return left?.Equals(right) ?? Equals(right, null);
        }
        public static bool operator !=(Entity left, Entity right)
        {
            return !(left == right);
        }
    }
}
