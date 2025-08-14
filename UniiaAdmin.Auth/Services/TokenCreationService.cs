using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using System.Security.Claims;
using UniiaAdmin.Auth.Interfaces;
using UniiaAdmin.Data.Constants;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Models;
using UniiaAdmin.Data.Models.AuthModels;

namespace UniiaAdmin.Auth.Services
{
    public class TokenCreationService : ITokenCreationService
	{
		private readonly UserManager<AdminUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly IJwtAuthenticator _jwtAuthenticator;
		private readonly IAuthService _authService;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IClaimUserService _claimUserService;
		private readonly MongoDbContext _mongoDbContext;	

		public TokenCreationService(
			IJwtAuthenticator jwtAuthenticator,
			UserManager<AdminUser> userManager,
			IHttpClientFactory httpClientFactory,
			IClaimUserService claimUserService,
			MongoDbContext mongoDbContext,
			RoleManager<IdentityRole> roleManager,
			IAuthService authService)
		{
			_jwtAuthenticator = jwtAuthenticator;
			_userManager = userManager;
			_httpClientFactory = httpClientFactory;
			_claimUserService = claimUserService;
			_mongoDbContext = mongoDbContext;
			_roleManager = roleManager;
			_authService=authService;
		}

		public async Task<UserTokens> CreateTokensAsync(List<Claim> claims, HttpContext httpContext)
		{
			var email = claims.First(c => c.Type == ClaimTypes.Email).Value;

			var user = await _userManager.FindByEmailAsync(email);

			if (user == null)
			{
				using var httpClient = _httpClientFactory.CreateClient();

				var profilePictureTask = _claimUserService.GetUserPictureFromClaims(claims!, httpClient);

				user = new AdminUser()
				{
					Email = email,
					UserName = email,
					IsOnline = true,
					Name = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)!.Value,
					Surname = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)!.Value,
				};

				AdminUserPhoto photo = new()
				{
					Id = ObjectId.GenerateNewId(),
					File = await profilePictureTask
				};

				user.ProfilePictureId = photo.Id.ToString();

				await _mongoDbContext.UserPhotos.AddAsync(photo);

				await _mongoDbContext.SaveChangesAsync();

				await _userManager.CreateAsync(user);

				await _userManager.AddToRoleAsync(user, CustomRoles.AdminRole);
			}

			user.LastSingIn = DateTime.UtcNow;

			var userRoles = await _userManager.GetRolesAsync(user);

			List<Claim> roleClaims = [];

			var newClaims = new ClaimsIdentity(
			[
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
			], JwtBearerDefaults.AuthenticationScheme);

			foreach (var userRole in userRoles)
			{
				newClaims.AddClaim(new Claim(ClaimTypes.Role, userRole));

				var role = await _roleManager.FindByNameAsync(userRole);

				var userRoleClaims = await _roleManager.GetClaimsAsync(role!);

				roleClaims.AddRange(userRoleClaims);
			}

			newClaims.AddClaims(roleClaims);

			var access_token = _jwtAuthenticator.GenerateAccessToken(newClaims.Claims);

			if (string.IsNullOrEmpty(access_token))
			{
				return new UserTokens();
			}

			var refresh_token = _jwtAuthenticator.GenerateRefreshToken();

			await _jwtAuthenticator.SaveRefreshTokenAsync(user, refresh_token);

			await _authService.AddLoginInfoToHistory(user, httpContext);

			return new UserTokens
			{
				AccessToken = access_token,
				RefreshToken = refresh_token
			};
		}
	}
}
