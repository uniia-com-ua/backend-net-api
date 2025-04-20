using MongoDB.Bson;
using UniiaAdmin.Auth.Interfaces;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Models;

namespace UNIIAadmin.Auth.Services
{
    public class AuthService : IAuthService
    {
        private readonly MongoDbContext _db;

        public AuthService(MongoDbContext mongoDatabase) 
        {
            _db = mongoDatabase;
        }

        public async Task AddLoginInfoToHistory(AdminUser adminUser, HttpContext httpContext)
        {
            httpContext.Request.Headers.TryGetValue("X-Real-IP", out var ipAdress);

            ipAdress.ToString();

            AdminLogInHistory logInHistoryItem = new()
            {
                Id = ObjectId.GenerateNewId(),
                UserId = adminUser.Id,
                LogInType = "Credential login",
                LogInTime = DateTime.UtcNow,
                IpAdress = ipAdress,
                UserAgent = httpContext.Request.Headers.UserAgent
            };

            await _db.AdminLogInHistories.AddAsync(logInHistoryItem);

            await _db.SaveChangesAsync();
        }
    }
}
