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
using UniiaAdmin.WebApi.Interfaces;
using UniiaAdmin.WebApi.Repository;
using UniiaAdmin.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

var services = builder.Services;
var configuration = builder.Configuration;

services.AddControllers()
	.AddCustomModelStateValidation()
	.AddDataAnnotationsLocalization();

services.AddEndpointsApiExplorer();

services.AddLocalization();

services.AddSwaggerWithJwtAuth();

services.AddHttpClient();

services.AddDistributedMemoryCache();

services.AddServices();

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