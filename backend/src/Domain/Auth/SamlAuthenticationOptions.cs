using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Auth
{
    public class SamlAuthenticationOptions
    {
        public string SamlCertificate{ get; set; }
        public string SamlEndpoint { get; set; }
        public string AppIdURI { get; set; }
        public string RedirectUrl { get; set; }
    }
}
