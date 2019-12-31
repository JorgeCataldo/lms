using System.Collections.Generic;
using Domain.SeedWork;

namespace Domain.ValueObjects
{
    public class ContactInfo : ValueObject
    {
        public ContactInfo(string email, string phone)
        {
            Email = email;
            Phone = phone;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Email;
            yield return Phone;
        }

        // public Email Email { get; }
        public string Email { get; }
        public string Phone { get; }
    }
}
