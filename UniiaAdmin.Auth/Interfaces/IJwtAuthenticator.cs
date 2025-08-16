using System.Security.Claims;
using UniiaAdmin.Data.Models;

namespace UniiaAdmin.Auth.Interfaces
{
	public interface IJwtAuthenticator
	{
		string GenerateAccessToken(IEnumerable<Claim>? claims);
		string GenerateRefreshToken();
		public Task<ClaimsPrincipal?> GetPrincipalFromExpiredToken(string? token);
		public Task SaveRefreshTokenAsync(AdminUser user, string refreshToken);
		public Task<bool> IsRefreshTokenValidAsync(string id, string? refreshToken);
	}
}
