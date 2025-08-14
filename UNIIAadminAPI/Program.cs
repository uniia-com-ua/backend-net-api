using AutoMapper;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System.Text;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.Data.Interfaces.FileInterfaces;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Extentions;
using UniiaAdmin.WebApi.FileServices;
using UniiaAdmin.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

var services = builder.Services;
var configuration = builder.Configuration;

services.AddControllers()
	.AddDataAnnotationsLocalization();

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
					Array.Empty<string>()
			}
	});
});

services.AddSwaggerWithJwtAuth();

services.AddHttpClient();

services.AddDistributedMemoryCache();

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

services.AddTransient<IHealthCheckService, HealthCheckService>();

services.AddTransient<IPaginationService, PaginationService>();

services.AddSingleton(provider => 
{
    var mongoClient = new MongoClient(Environment.GetEnvironmentVariable("MONGODB_CONNECTION")!);
    var mongoDatabase = mongoClient.GetDatabase(Environment.GetEnvironmentVariable("MONGODB_NAME")!);

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

services.AddAuthorizationBuilder()
		.SetDefaultPolicy(new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
		.RequireAuthenticatedUser()
		.Build());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.DisplayRequestDuration());
}

await app.Services.ApplyMigrationsAsync();

app.UseHttpsRedirection();

app.AddLocalization();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();