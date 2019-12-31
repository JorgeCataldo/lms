using System.Collections.Generic;
using Domain.SeedWork;

namespace Domain.ValueObjects
{
    public class Address : ValueObject
    {
        public string Street { get; private set; }
        public string Address2 { get; private set; }
        public string District { get; private set; }
        public string City { get; private set; }
        public string State { get; private set; }
        public string Country { get; private set; }
        public string ZipCode { get; private set; }

        private Address() { }

        public Address(string street, string address2, string city, string state, string country, string zipcode, string district = null)
        {
            Street = street;
            Address2 = address2;
            District = district;
            City = city;
            State = state;
            Country = country;
            ZipCode = zipcode;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Street;
            yield return Address2;
            yield return District;
            yield return City;
            yield return State;
            yield return Country;
            yield return ZipCode;
        }
    }
}
