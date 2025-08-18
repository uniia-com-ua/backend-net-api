using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniiaAdmin.Data.Constants;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Interfaces;

namespace UniiaAdmin.WebApi.Extentions;

public static class DatabaseExtention
{
	public static async Task ApplyMigrationsAsync(this IServiceProvider services)
	{
		using (var scope = services.CreateScope())
		{
			var databaseInitilizerService = scope.ServiceProvider.GetRequiredService<IDatabaseInitilizerService>();

			await databaseInitilizerService.InitializeAsync();
		}
	}
}

