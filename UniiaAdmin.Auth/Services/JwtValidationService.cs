using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using UniiaAdmin.Auth.Interfaces;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Models;

namespace UniiaAdmin.Auth.Services
{
	public class JwtValidationService : IJwtAuthenticator
	{
		private readonly IConfiguration _configuration;
		private readonly UserManager<AdminUser> _userManager;

		public JwtValidationService(IConfiguration configuration,
			UserManager<AdminUser> adminContext)
		{
			_configuration = configuration;
			_userManager = adminContext;
		}

		public string GenerateAccessToken(IEnumerable<Claim>? claims)
		{
			if (claims == null) 
			{
				return string.Empty;
			}

			var filteredClaims = claims.Where(c => c.Type != JwtRegisteredClaimNames.Aud);

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_TOKEN_KEY")!));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
				issuer: _configuration["JWT:ValidIssuer"]!,
				audience: _configuration["JWT:ValidAudience"]!,
				claims: filteredClaims,
				expires: DateTime.UtcNow.AddMinutes(double.Parse(_configuration["JWT:TokenValidityInMinutes"]!)),
				signingCredentials: creds
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		public string GenerateRefreshToken()
		{
			var randomBytes = new byte[64];
			using var rng = RandomNumberGenerator.Create();
			rng.GetBytes(randomBytes);
			return Convert.ToBase64String(randomBytes);
		}

		public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
		{
			var tokenValidationParameters = new TokenValidationParameters
			{
				ValidateAudience = false,
				ValidateIssuer = false,
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT_TOKEN_KEY"]!)),
				ValidateLifetime = false
			};

			var tokenHandler = new JwtSecurityTokenHandler();
			var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

			if (securityToken is not JwtSecurityToken jwtToken || jwtToken.Header.Alg != SecurityAlgorithms.HmacSha256)
				return null;

			return principal;
		}

		public async Task SaveRefreshTokenAsync(string email, string refreshToken)
		{
			var user = await _userManager.FindByEmailAsync(email);

			if (user == null)
			{
				return;
			}

			user.RefreshTokenExpiryTime = DateTime.UtcNow
													.AddDays(double.Parse(_configuration["JWT:RefreshTokenValidityInDays"]!))
													.ToString();

			await _userManager.UpdateAsync(user);

			await _userManager.SetAuthenticationTokenAsync(user!, _configuration["JWT:ValidIssuer"]!, "refresh_token", refreshToken);
		}

		public async Task<bool> IsRefreshTokenValidAsync(string email, string? refreshToken)
		{
			var user = await _userManager.FindByEmailAsync(email);

			if (user == null)
			{
				return await Task.FromResult<bool>(false);
			}

			var userToken = await _userManager.GetAuthenticationTokenAsync(user, _configuration["JWT:ValidIssuer"]!, "refresh_token");

			if (string.IsNullOrEmpty(userToken))
			{
				return false;
			}

			if (!userToken.Equals(refreshToken))
			{
				return false;
			}

			var tokenExpiryDate = DateTime.Parse(user.RefreshTokenExpiryTime!, CultureInfo.InvariantCulture);

			return DateTime.UtcNow < tokenExpiryDate;
		}
	}
}
