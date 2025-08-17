using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniiaAdmin.Data.Constants;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Models;

namespace UniiaAdmin.WebApi.Extentions;

public static class DatabaseExtention
{
	public static async Task ApplyMigrationsAsync(this IServiceProvider services)
	{
		using (var scope = services.CreateScope())
		{
			var adminContext = scope.ServiceProvider.GetRequiredService<AdminContext>();

			var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

			await adminContext.Database.EnsureCreatedAsync();

			await applicationContext.Database.EnsureCreatedAsync();
		}
	}
}

