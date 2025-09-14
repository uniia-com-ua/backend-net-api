using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using UniiaAdmin.Auth.Interfaces;
using UniiaAdmin.Data.Constants;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Models;

namespace UniiaAdmin.Auth.Services
{
	public class JwtValidationService : IJwtAuthenticator
	{
		private readonly IConfiguration _configuration;
		private readonly IAdminUserRepository _adminUserRepository;

		public JwtValidationService(
			IConfiguration configuration,
			IAdminUserRepository adminUserRepository)
		{
			_configuration = configuration;
			_adminUserRepository = adminUserRepository;
		}

		public string GenerateAccessToken(IEnumerable<Claim>? claims)
		{
			if (claims == null) 
			{
				return string.Empty;
			}

			var filteredClaims = claims.Where(c => c.Type != JwtRegisteredClaimNames.Aud);

			var envKey = _configuration["JWT_TOKEN_KEY"];

			var keyBytes = Encoding.UTF8.GetBytes(envKey!);
			var key = new SymmetricSecurityKey(keyBytes);

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

		public async Task<ClaimsPrincipal?> GetPrincipalFromExpiredToken(string? token)
		{
			var tokenValidationParameters = new TokenValidationParameters
			{
				ValidateAudience = false,
				ValidateIssuer = false,
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT_TOKEN_KEY"]!)),
				ValidateLifetime = false,
			};

			var tokenHandler = new JwtSecurityTokenHandler();

			var principal = await tokenHandler.ValidateTokenAsync(token, tokenValidationParameters);

			if (principal.SecurityToken is not JwtSecurityToken jwtToken
				|| jwtToken.Header.Alg != SecurityAlgorithms.HmacSha256
				|| principal.Exception != null)
				return null;

			return new ClaimsPrincipal(principal.ClaimsIdentity);
		}

		public async Task SaveRefreshTokenAsync(AdminUser user, string refreshToken)
		{
			user.RefreshTokenExpiryTime = DateTime.UtcNow
													.AddDays(double.Parse(_configuration["JWT:RefreshTokenValidityInDays"]!))
													.ToString();

			await _adminUserRepository.UpdateAsync(user);

			await _adminUserRepository.SetAuthenticationTokenAsync(user!, _configuration["JWT:ValidIssuer"]!, "refresh_token", refreshToken);
		}

		public async Task<bool> IsRefreshTokenValidAsync(string id, string? refreshToken)
		{
			var user = await _adminUserRepository.FindByIdAsync(id);

			if (user == null)
			{
				return await Task.FromResult<bool>(false);
			}

			var userToken = await _adminUserRepository.GetAuthenticationTokenAsync(user, _configuration["JWT:ValidIssuer"]!, "refresh_token");

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
