using AspNetCore.Identity.MongoDbCore.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Security.Claims;
using UNIIAadminAPI.Models;
using UNIIAadminAPI.Services;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var configuration = builder.Configuration;

services.AddEndpointsApiExplorer();
services.AddSwaggerGen();
services.AddHttpClient();

services.AddDistributedMemoryCache();

services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(15);
});

services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "session";
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
    options.Cookie.IsEssential = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

configuration.AddJsonFile("appsettings.json");

var client = new MongoClient(Environment.GetEnvironmentVariable("MONGODB_CONNECTION"));

services.AddSingleton(client.GetDatabase("test"));

services.AddSingleton(client);

services.AddIdentity<MongoIdentityUser, MongoIdentityRole>()
    .AddMongoDbStores<MongoIdentityUser, MongoIdentityRole, Guid>(Environment.GetEnvironmentVariable("MONGODB_CONNECTION"), "test")
    .AddDefaultTokenProviders();

services
    .AddAuthentication(options =>
    {
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.Cookie.Name = "authSeanse";
    })
    .AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = Environment.GetEnvironmentVariable("OAUTH2_CLIENT_ID")!;
        googleOptions.ClientSecret = Environment.GetEnvironmentVariable("OAUTH2_CLIENT_SECRET")!;
        googleOptions.CallbackPath = "/admin/api/auth/google-callback";
        googleOptions.SaveTokens = true;
        googleOptions.AccessType = "offline";
        googleOptions.Events.OnCreatingTicket = (context) =>
        {
            var picture = context.User.GetProperty("picture").GetString();

            context.Identity!.AddClaim(new Claim("picture", picture!));

            return Task.CompletedTask;
        };
    });

services.AddScoped(provider =>
{
    var clientId = Environment.GetEnvironmentVariable("OAUTH2_CLIENT_ID")!;
    var clientSecret = Environment.GetEnvironmentVariable("OAUTH2_CLIENT_SECRET")!; 
    var tokenKey = Environment.GetEnvironmentVariable("OAUTH2_TOKEN_KEY")!;

    return new TokenService(clientId, clientSecret, tokenKey);
});

services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(builder =>
{
    builder.WithOrigins("https://localhost:7193")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
});

app.UseSession();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();