using Amazon.Runtime.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using MongoDbGenericRepository;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using UNIIAadminAPI.Models;

namespace UNIIAadminAPI.Services
{
    public class AuthService(HttpContext httpContext)
    {
        private readonly IMongoDbContext _db = httpContext.RequestServices.GetRequiredService<IMongoDbContext>();
        private readonly HttpContext _httpContext = httpContext;

        public async Task AddLoginInfoToHistory(AdminUser adminUser)
        {
            _httpContext.Request.Headers.TryGetValue("X-Real-IP", out var ipAdress);

            ipAdress.ToString();

            LogInHistory logInHistoryItem = new()
            {
                UserId = adminUser.Id,
                LogInType = "Credential login",
                LogInTime = DateTime.UtcNow,
                IpAdress = ipAdress,
                UserAgent = _httpContext.Request.Headers.UserAgent
            };

            await _db.GetCollection<LogInHistory>().InsertOneAsync(logInHistoryItem);
        }
    }
}
