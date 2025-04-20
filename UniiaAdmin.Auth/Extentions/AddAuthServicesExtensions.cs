using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using UniiaAdmin.Auth.Interfaces;
using UniiaAdmin.Data.Models.AuthModels;

namespace UniiaAdmin.Auth.Extentions
{
    public static class AddAuthServicesExtensions
	{
		public static void AddAuthServices(this IServiceCollection Services, IConfiguration Configuration)
		{
			Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			.AddGoogle(googleOptions =>
			{
				googleOptions.ClientId = Environment.GetEnvironmentVariable("OAUTH2_CLIENT_ID")!;
				googleOptions.ClientSecret = Environment.GetEnvironmentVariable("OAUTH2_CLIENT_SECRET")!;
				googleOptions.CallbackPath = "/admin/api/auth/google-callback";

				googleOptions.Events.OnCreatingTicket = async context =>
				{
					var claims = new List<Claim>
						{
							new(ClaimTypes.Name,context.User.GetProperty("name").GetString() ?? ""),
							new(ClaimTypes.GivenName, context.User.GetProperty("given_name").GetString() ?? ""),
							new(ClaimTypes.Surname, context.User.GetProperty("family_name").GetString() ?? ""),
							new(ClaimTypes.Email, context.User.GetProperty("email").GetString() ?? ""),
							new("Picture", context.User.GetProperty("picture").GetString() ?? "")
						};

					var tokenService = context.HttpContext.RequestServices.GetRequiredService<ITokenCreationService>();

					var userTokens = await tokenService.CreateTokensAsync(claims, context.HttpContext);

					context.HttpContext.Items["UserTokens"] = userTokens;
				};

				googleOptions.Events.OnTicketReceived = context =>
				{
					var userTokens = context.HttpContext.Items["UserTokens"] as UserTokens;

					context.Response.Redirect($"tokens?accessToken={Uri.EscapeDataString(userTokens!.AccessToken)}&refreshToken={Uri.EscapeDataString(userTokens!.RefreshToken)}");

					context.HandleResponse();

					return Task.CompletedTask;
				};
			}).AddJwtBearer(options =>
			{
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ValidIssuer = Configuration["JWT:ValidIssuer"],
					ValidAudience = Configuration["JWT:ValidAudience"],
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_TOKEN_KEY")!))
				};
			});

			Services.AddAuthorization(options =>
			{
				options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
					.RequireAuthenticatedUser()
					.Build();
			});
		}
	}
}
