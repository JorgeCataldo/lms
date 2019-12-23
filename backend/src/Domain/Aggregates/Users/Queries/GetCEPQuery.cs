using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using Domain.ExternalService;
using Domain.IdentityStores.Settings;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Users.Queries
{
    public class GetCEPQuery
    {
        public class Contract : CommandContract<Result<CEPItem>>
        {
            [Required]
            public string Cep { get; set; }
        }

        public class CEPItem
        {
            public string Cep { get; set; }
            public string Logradouro { get; set; }
            public string Complemento { get; set; }
            public string Bairro { get; set; }
            public string Localidade { get; set; }
            public string Uf { get; set; }
            public string Unidade { get; set; }
            public string Ibge { get; set; }
            public string Gia { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<CEPItem>>
        {
            public Handler() 
            {
            }

            public async Task<Result<CEPItem>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (string.IsNullOrEmpty(request.Cep))
                    return Result.Fail<CEPItem>("Codigo inválido");

                var response = await GetAccessToken(request.Cep);
                string content = await response.Content.ReadAsStringAsync();

                if (response.StatusCode != HttpStatusCode.OK)
                    return Result.Fail<CEPItem>("Ocorreu um erro ao se comunicar com o viacep");

                var parsed = JObject.Parse(content);

                var cep = new CEPItem()
                {
                    Cep = (string)parsed["cep"],
                    Logradouro = (string)parsed["logradouro"],
                    Complemento = (string)parsed["complemento"],
                    Bairro = (string)parsed["bairro"],
                    Localidade= (string)parsed["localidade"],
                    Uf = (string)parsed["uf"],
                    Unidade = (string)parsed["unidade"],
                    Ibge = (string)parsed["ibge"],
                    Gia = (string)parsed["gia"],
                };

                return Result.Ok(cep);
            }

            private async Task<HttpResponseMessage> GetAccessToken(string cep)
            {
                return await ViaCEP.CEP(
                    cep
                );
            }
        }
    }
}