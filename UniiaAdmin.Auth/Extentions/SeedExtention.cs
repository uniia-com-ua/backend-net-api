using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using UniiaAdmin.Data.Constants;
using UniiaAdmin.Data.Models;

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

			if (!await roleManager.RoleExistsAsync(CustomRoles.GuestRole))
			{
				await roleManager.CreateAsync(new(CustomRoles.GuestRole));
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

	public static async Task SeedUsersAsync(this IServiceProvider services)
	{
		using (var scope = services.CreateScope())
		{
			var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AdminUser>>();

			var users = new List<AdminUser>
			{
				new()
				{
					Email = "artem.shyian@uniia.com.ua",
					UserName = "artem.shyian@uniia.com.ua",
					Name = "Артем",
					Surname = "Шиян",
					IsOnline = true,
				},
				new()
				{
					Email = "ivan.lazarenko@uniia.com.ua",
					UserName = "ivan.lazarenko@uniia.com.ua",
					Name = "Іван",
					Surname = "Лазаренко",
					IsOnline = true,
				},
			};

			foreach(AdminUser user in users)
			{
				await userManager.CreateAsync(user);
				await userManager.AddToRoleAsync(user, CustomRoles.AdminRole);
			}
		}
	}
}
