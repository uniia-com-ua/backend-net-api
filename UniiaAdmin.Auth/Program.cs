using DotNetEnv;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniiaAdmin.Auth.Extentions;
using UniiaAdmin.Auth.Interfaces;
using UniiaAdmin.Auth.Repos;
using UniiaAdmin.Auth.Services;
using UniiaAdmin.Data.Constants;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Models;
using UNIIAadmin.Auth.Services;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// TODO: change known proxies and networks to known ones
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | 
                              ForwardedHeaders.XForwardedProto |
                              ForwardedHeaders.XForwardedHost;
    
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

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
builder.Services.AddTransient<IAuthService, AuthService>();
builder.Services.AddScoped<IMongoUnitOfWork, MongoUnitOfWork>();
builder.Services.AddScoped<IAdminUserRepository, AdminUserRepository>();
builder.Services.AddScoped<IAdminUnitOfWork, AdminUnitOfWork>();
builder.Services.AddScoped<IHealthCheckService, HealthCheckService>();


builder.Services.AddAutoMapper(_ => { }, AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddDbContext<AdminContext>(options =>
	options.UseNpgsql(Environment.GetEnvironmentVariable("POSTGRES_ADMIN_CONNECTION")));

builder.Services.AddDbContext<MongoDbContext>(options
	=> options.UseMongoDB(Environment.GetEnvironmentVariable("MONGODB_CONNECTION")!, Environment.GetEnvironmentVariable("MONGODB_NAME")!));

builder.Services.AddIdentity<AdminUser, IdentityRole>().AddEntityFrameworkStores<AdminContext>();

var app = builder.Build();

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI(options => options.DisplayRequestDuration());
}

if (!app.Environment.IsEnvironment(CustomEnviroments.Test))
{
	using (var scope = app.Services.CreateScope())
	{
		var adminContext = scope.ServiceProvider.GetRequiredService<IAdminUnitOfWork>();
		var mongoUnitOfWork = scope.ServiceProvider.GetRequiredService<IMongoUnitOfWork>();

		await adminContext.CreateAsync();
		await mongoUnitOfWork.CreateAsync();
	}

	app.UseHttpsRedirection();

	await app.Services.SeedRoleClaimsAsync();

	await app.Services.SeedUsersAsync();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

await app.RunAsync();
