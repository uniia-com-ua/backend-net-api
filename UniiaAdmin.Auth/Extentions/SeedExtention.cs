using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using UniiaAdmin.Data.Constants;

namespace UniiaAdmin.Auth.Extentions;

public static class SeedExtention
{
	public static async Task SeedRoleClaimsAsync(this IServiceProvider services)
	{
		using (var scope = services.CreateScope())
		{
			var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

			if (!await roleManager.RoleExistsAsync(CustomRoles.AdminRole))
			{
				await roleManager.CreateAsync(new(CustomRoles.AdminRole));
			}

			var adminRole = await roleManager.FindByNameAsync(CustomRoles.AdminRole);

			var claims = await roleManager.GetClaimsAsync(adminRole!);

			foreach (var modelName in Enum.GetValues(typeof(PermissionResource)))
			{
				foreach (var action in Enum.GetValues(typeof(CrudActions)))
				{
					if (!claims.Any(cl => cl.Value == $"{modelName}.{action}"))
					{
						await roleManager.AddClaimAsync(adminRole!, new Claim(CustomClaimTypes.Permission, $"{modelName}.{action}"));
					}
				}
			}
		}
	}
}
