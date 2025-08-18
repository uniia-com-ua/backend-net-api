namespace UniiaAdmin.WebApi.Extentions;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using UniiaAdmin.Data.Constants;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.Data.Interfaces.FileInterfaces;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.FileServices;
using UniiaAdmin.WebApi.Interfaces;
using UniiaAdmin.WebApi.Repository;
using UniiaAdmin.WebApi.Services;

public static class ServicesExtention
{
	public static void AddServices(this IServiceCollection services)
	{
		services.AddAutoMapper(_ => { }, AppDomain.CurrentDomain.GetAssemblies());

		services.AddDbContext<AdminContext>(options =>
			options.UseNpgsql(Environment.GetEnvironmentVariable("POSTGRES_ADMIN_CONNECTION")));

		services.AddDbContext<ApplicationContext>(options =>
			options.UseNpgsql(Environment.GetEnvironmentVariable("POSTGRES_APPLICATION_CONNECTION")));

		services.AddIdentity<AdminUser, IdentityRole>().AddEntityFrameworkStores<AdminContext>();

		services.AddDbContext<MongoDbContext>(options
			=> options.UseMongoDB(Environment.GetEnvironmentVariable("MONGODB_CONNECTION")!, Environment.GetEnvironmentVariable("MONGODB_NAME")!));

		services.AddSingleton<IFileValidatorFactory, FileValidatorFactory>();

		services.AddSingleton<IFileValidationService, FileValidationService>();

		services.AddSingleton<IFileProcessingService, FileProcessingService>();

		services.AddScoped<IFileEntityService, FileEntityService>();

		services.AddScoped<IFileRepository, FileRepository>();

		services.AddScoped<IGenericRepository, GenericRepository>();

		services.AddScoped<IPhotoProvider, PhotoProvider>();

		services.AddScoped<IPhotoRepository, PhotoRepository>();

		services.AddScoped<IQueryRepository, QueryRepository>();

		services.AddScoped<ISmallPhotoRepository, SmallPhotoRepository>();

		services.AddTransient<IHealthCheckService, HealthCheckService>();

		services.AddTransient<IPaginationService, PaginationService>();

		services.AddTransient<IEntityQueryService, EntityQueryService>();

		services.AddScoped<IGenericRepository, GenericRepository>();

		services.AddTransient<IDatabaseInitilizerService, DatabaseInitializerService>();

		services.AddSingleton(provider =>
		{
			var mongoClient = new MongoClient(Environment.GetEnvironmentVariable("MONGODB_CONNECTION")!);
			var mongoDatabase = mongoClient.GetDatabase(Environment.GetEnvironmentVariable("MONGODB_NAME")!);

			return new GridFSBucket(mongoDatabase);
		});
	}
}
