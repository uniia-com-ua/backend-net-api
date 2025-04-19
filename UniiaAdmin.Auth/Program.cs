using DotNetEnv;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniiaAdmin.Auth.Extentions;
using UniiaAdmin.Auth.Interfaces;
using UniiaAdmin.Auth.Services;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Models;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
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

builder.Services.AddHttpClient();

builder.Services.AddAuthServices(builder.Configuration);

builder.Services.AddTransient<IJwtAuthenticator, JwtValidationService>();
builder.Services.AddTransient<IClaimUserService, ClaimUserService>();
builder.Services.AddTransient<ITokenCreationService, TokenCreationService>();

builder.Services.AddDbContext<AdminContext>(options =>
	options.UseNpgsql(Environment.GetEnvironmentVariable("PostgresAdminConnection")));

builder.Services.AddDbContext<MongoDbContext>(options
	=> options.UseMongoDB(Environment.GetEnvironmentVariable("MongoDbConnection")!, "test"));

builder.Services.AddIdentity<AdminUser, IdentityRole>().AddEntityFrameworkStores<AdminContext>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI(options => options.DisplayRequestDuration());
}

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.MapControllers();

await app.RunAsync();
