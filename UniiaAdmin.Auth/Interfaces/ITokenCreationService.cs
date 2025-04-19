using System.Security.Claims;
using UniiaAdmin.Data.Models;

namespace UniiaAdmin.Auth.Interfaces
{
    public interface ITokenCreationService
    {
        public Task<UserTokens> CreateTokensAsync(List<Claim> claims);
    }
}
