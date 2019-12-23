using System.Threading.Tasks;
using Domain.Aggregates.Users;

namespace Domain.IdentityStores.Settings
{
    public interface ITokenGenerator
    {
        Task<TokenInfo> GenerateUserToken(User user);
    }
}