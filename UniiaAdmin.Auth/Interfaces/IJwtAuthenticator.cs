using System.Security.Claims;

namespace UniiaAdmin.Auth.Interfaces
{
	public interface IJwtAuthenticator
	{
		string GenerateAccessToken(IEnumerable<Claim>? claims);
		string GenerateRefreshToken();
		ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
		Task SaveRefreshTokenAsync(string email, string refreshToken);
		Task<bool> IsRefreshTokenValidAsync(string email, string? refreshToken);
	}
}
