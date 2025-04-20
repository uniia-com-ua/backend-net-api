using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System.Text;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Interfaces.FileInterfaces;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.FileServices;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

var services = builder.Services;
var configuration = builder.Configuration;

services.AddControllers();

services.AddEndpointsApiExplorer();

services.AddSwaggerGen(options =>
{
	options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
	{
		Name = "Authorization",
		Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
		Scheme = "Bearer",
		BearerFormat = "JWT",
		In = Microsoft.OpenApi.Models.ParameterLocation.Header,
		Description = "JWT Authorization header using the Bearer scheme."
	});
	options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement {
			{
				new Microsoft.OpenApi.Models.OpenApiSecurityScheme {
						Reference = new Microsoft.OpenApi.Models.OpenApiReference {
							Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
								Id = "Bearer"
						}
					},
					new string[] {}
			}
	});
});

services.AddHttpClient();

builder.Services.AddDistributedMemoryCache();

services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

configuration.AddJsonFile("appsettings.json");

services.AddDbContext<AdminContext>(options =>
    options.UseNpgsql(Environment.GetEnvironmentVariable("PostgresAdminConnection")));

services.AddDbContext<ApplicationContext>(options =>
    options.UseNpgsql(Environment.GetEnvironmentVariable("PostgresApplicationConnection")));

services.AddIdentity<AdminUser, IdentityRole>().AddEntityFrameworkStores<AdminContext>();

services.AddDbContext<MongoDbContext>(options 
    => options.UseMongoDB(Environment.GetEnvironmentVariable("MongoDbConnection")!, "test"));

services.AddSingleton<IFileValidatorFactory, FileValidatorFactory>();

services.AddSingleton<IFileValidationService, FileValidationService>();

services.AddSingleton<IFileProcessingService, FileProcessingService>();

services.AddScoped<IFileEntityService, FileEntityService>();

services.AddSingleton(provider => 
{
    var mongoClient = new MongoClient(Environment.GetEnvironmentVariable("MongoDbConnection")!);
    var mongoDatabase = mongoClient.GetDatabase("test");

    return new GridFSBucket(mongoDatabase);
});

services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
		ValidAudience = builder.Configuration["JWT:ValidAudience"],
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_TOKEN_KEY")!))
	};
});

services.AddAuthorization(options =>
{
	options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
		.RequireAuthenticatedUser()
		.Build();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.DisplayRequestDuration());
}

using (var scope = app.Services.CreateScope())
{
    var adminContext = scope.ServiceProvider.GetRequiredService<AdminContext>();

    var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

    await adminContext.Database.EnsureCreatedAsync();
    
    await applicationContext.Database.EnsureCreatedAsync();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();