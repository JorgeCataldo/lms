using Domain.Aggregates.ColorPalettes;
using System.Collections.Generic;

namespace Domain.IdentityStores.Settings
{
    public class TokenUser
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Cpf { get; set; }
        public string LogoUrl { get; set; }
        public List<ColorBaseValue> ColorBaseValues { get; set; }
    }
}