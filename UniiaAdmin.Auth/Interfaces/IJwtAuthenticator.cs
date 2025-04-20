using System.Security.Claims;
using UniiaAdmin.Data.Models;

namespace UniiaAdmin.Auth.Interfaces
{
	public interface IJwtAuthenticator
	{
		string GenerateAccessToken(IEnumerable<Claim>? claims);
		string GenerateRefreshToken();
		ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
		Task SaveRefreshTokenAsync(AdminUser user, string refreshToken);
		Task<bool> IsRefreshTokenValidAsync(string id, string? refreshToken);
	}
}
