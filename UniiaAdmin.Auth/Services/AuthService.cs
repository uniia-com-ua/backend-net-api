using MongoDB.Bson;
using UniiaAdmin.Auth.Interfaces;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Models;

namespace UNIIAadmin.Auth.Services
{
    public class AuthService : IAuthService
    {
        private readonly IMongoUnitOfWork _mongoUnitOfWork;

        public AuthService(IMongoUnitOfWork mongoDatabase) 
        {
			_mongoUnitOfWork = mongoDatabase;
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

            await _mongoUnitOfWork.AddAsync(logInHistoryItem);

            await _mongoUnitOfWork.SaveChangesAsync();
        }
    }
}
