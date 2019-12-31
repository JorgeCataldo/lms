using System.Collections.Generic;
using Domain.ValueObjects.Serializers;
using MongoDB.Bson.Serialization.Attributes;
using Tg4.Infrastructure.Functional;

namespace Domain.ValueObjects
{
    [BsonSerializer(typeof(CpfSerializer))]
    public class Cpf : ValueObject
    {
        public string Value { get; }

        private Cpf(string value)
        {
            Value = value;
        }

        public static Cpf Load(string cpf){
            return new Cpf(cpf);
        }

        public static Result<Cpf> Create(string value){
            return Validate(value) ? Result.Ok(new Cpf(value)) : Result.Fail<Cpf>("CPF inválido."); 
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        private static bool Validate(string cpf)
        {
            if (string.IsNullOrEmpty(cpf))
                return false;

            cpf = cpf.Replace(".", "").Replace("-", "");

            if (cpf.Length != 11)
                return false;

            var digits = new int[11];
            var equalDigitsSequence = 0;
            var partialResult1 = 0;
            var partialResult2 = 0;

            for (var i = 0; i < cpf.Length; i++)
            {
                if (!int.TryParse(cpf[i].ToString(), out digits[i]))
                    return false;

                if (i > 0 && cpf[i] == cpf[i - 1])
                    equalDigitsSequence++;

                if (i <= 8)
                    partialResult1 += digits[i] * (10 - i);

                if (i <= 9)
                    partialResult2 += digits[i] * (11 - i);
            }

            if (equalDigitsSequence == 10)
                return false;

            partialResult1 = (partialResult1 * 10) % 11;
            partialResult1 = partialResult1 == 10 ? 0 : partialResult1;

            partialResult2 = (partialResult2 * 10) % 11;
            partialResult2 = partialResult2 == 10 ? 0 : partialResult2;

            if (partialResult1 == int.Parse(cpf[9].ToString()) && partialResult2 == int.Parse(cpf[10].ToString()))
                return true;

            return false;
        }
    }
}
