using MediatR;
using MongoDB.Bson;
using System;
using System.Collections.Generic;

namespace Domain.Base
{
    public class AuthenticationData
    {
        public ObjectId UserId { get; set; }
        public ObjectId LoggedUserId { get; set; }
        public ObjectId? ImpersonatedUserId { get; set; }
        public string ImpersonatedUserRole { get; set; }
    }
    public class CommandContract<T> : IRequest<T>
    {
        public AuthenticationData AuthenticationData { get; set; }
    }
}
