using System.Collections.Generic;
using Domain.Enumerations;
using Domain.SeedWork;

namespace Domain.ValueObjects
{
    public class Telephone : ValueObject
    {
        public string Number { get; set; }
        public TelephoneType Type { get; set; }

        private Telephone() { }

        public Telephone(TelephoneType telephoneType, string telephone)
        {
            Number = telephone;
            Type = telephoneType;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Number;
            yield return Type;
        }
    }
}
