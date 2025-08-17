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

services.AddLocalization();

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

services.AddTransient<IEntityQueryService, EntityQueryService>();

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

services.SeedPolicies();

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