using DotNetEnv;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System.Security.Claims;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Interfaces.FileInterfaces;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.FileServices;
using UniiaAdmin.WebApi.Services;
using UNIIAadminAPI.Services;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

var services = builder.Services;
var configuration = builder.Configuration;

services.AddControllers();

services.AddEndpointsApiExplorer();

services.AddSwaggerGen();

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


services.AddSingleton<TokenService>(provider =>
{
    var clientId = Environment.GetEnvironmentVariable("OAUTH2_CLIENT_ID")!;
    var clientSecret = Environment.GetEnvironmentVariable("OAUTH2_CLIENT_SECRET")!;
    var tokenKey = Environment.GetEnvironmentVariable("OAUTH2_TOKEN_KEY")!;

    return new(clientId, clientSecret, tokenKey);
});

services.AddSingleton<IFileValidatorFactory, FileValidatorFactory>();

services.AddSingleton<IFileValidationService, FileValidationService>();

services.AddSingleton<IFileProcessingService, FileProcessingService>();

services.AddScoped<IFileEntityService, FileEntityService>();

services.AddTransient(provider =>
{
    var db = provider.GetRequiredService<MongoDbContext>();

    return new LogActionService(db);
});

services.AddSingleton(provider => 
{
    var mongoClient = new MongoClient(Environment.GetEnvironmentVariable("MongoDbConnection")!);
    var mongoDatabase = mongoClient.GetDatabase("test");

    return new GridFSBucket(mongoDatabase);
});

services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(15);
});

services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "authSessionCookie";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
    options.Cookie.IsEssential = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

services
    .AddAuthentication(options =>
    {
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.Cookie.Name = "googleAuthSession";
    })
    .AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = Environment.GetEnvironmentVariable("OAUTH2_CLIENT_ID")!;
        googleOptions.ClientSecret = Environment.GetEnvironmentVariable("OAUTH2_CLIENT_SECRET")!;
        googleOptions.CallbackPath = "/admin/api/auth/google-callback";
        googleOptions.SaveTokens = true;
        googleOptions.AccessType = "offline";

        googleOptions.Events.OnRedirectToAuthorizationEndpoint = context =>
        {
            context.Response.Redirect(context.RedirectUri + "&prompt=consent");
            return Task.CompletedTask;
        };

        googleOptions.Events.OnCreatingTicket = (context) =>
        {
            var picture = context.User.GetProperty("picture").GetString();

            context.Identity!.AddClaim(new Claim("picture", picture!));

            return Task.CompletedTask;
        };
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

app.UseSession();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();