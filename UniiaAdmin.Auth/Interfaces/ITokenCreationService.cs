using System.Security.Claims;
using UniiaAdmin.Data.Models.AuthModels;

namespace UniiaAdmin.Auth.Interfaces
{
    public interface ITokenCreationService
    {
        public Task<UserTokens> CreateTokensAsync(List<Claim> claims, HttpContext httpContext);
    }
}
