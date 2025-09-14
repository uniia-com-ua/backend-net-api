using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using UniiaAdmin.Data.Constants;

namespace UniiaAdmin.WebApi.Extentions;

public static class SeedExtention
{
	public static void SeedPolicies(this IServiceCollection service)
	{
		service.AddAuthorization(options =>
		{
			options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme).RequireAuthenticatedUser().Build();

			foreach (var modelName in Enum.GetValues(typeof(PermissionResource)))
			{
				foreach (var action in Enum.GetValues(typeof(CrudActions)))
				{
					var policyName = $"{modelName}.{action}";
					options.AddPolicy(policyName, policy =>
						policy.RequireAuthenticatedUser()
						.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
								  .RequireAssertion(context =>
									  context.User.IsInRole(CustomRoles.AdminRole) ||
									  context.User.HasClaim(c =>
										  c.Type == CustomClaimTypes.Permission &&
										  c.Value == policyName)
								  ));
				}
			}
		});
	}
}

