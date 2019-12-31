using System.Collections.Generic;
using System.Text.RegularExpressions;
using Domain.ValueObjects.Serializers;
using MongoDB.Bson.Serialization.Attributes;
using Tg4.Infrastructure.Functional;

namespace Domain.ValueObjects
{
    [BsonSerializer(typeof(EmailSerializer))]
    public class Email: ValueObject
    {        
        private const string TheEmailPattern = @"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*" + "@" + @"((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))\z";
        
        public string Value { get; set; }

        private Email(string email)
        {
            Value = email;
        }

        public static Email Load(string email)
        {
            return new Email(email);
        }
        
        public static Result<Email> Create(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Result.Fail<Email>("Email não pode ser vazio");
 
            if (email.Length > 100)
                return Result.Fail<Email>("Email grande demais");

            if (!Regex.IsMatch(email, TheEmailPattern))
                return Result.Fail<Email>("Email inválido");
            
            return Result.Ok(new Email(email));
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
