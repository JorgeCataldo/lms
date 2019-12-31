using System.Collections.Generic;
using Domain.SeedWork;
using Tg4.Infrastructure.Functional;

namespace Domain.Enumerations
{
    public class TelephoneType : Enumeration
    {
        public static readonly TelephoneType CellPhone = new TelephoneType(1, "CellPhone");
        public static readonly TelephoneType HomePhone = new TelephoneType(2, "HomePhone");

        private TelephoneType(int id, string type) : base(id, type){
        }

        public static Result<TelephoneType> Create(int id){
            return List.ContainsKey(id) ? Result.Ok(List[id]) : Result.Fail<TelephoneType>("Tipo telefone inexistente.");
        }

        public static readonly IDictionary<int, TelephoneType> List = new Dictionary<int, TelephoneType> { 
            {CellPhone.Id, CellPhone}, {HomePhone.Id, HomePhone} 
        };
    }
}
