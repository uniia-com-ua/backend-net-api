using MongoDB.Bson;
using MongoDB.Driver;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.Data.Models;

namespace UNIIAadminAPI.Services
{
    public class AuthService : IDisposable, IAuthService
    {
        private readonly MongoDbContext _db;

        private bool disposedValue;

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

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this._db.Dispose();
                }

                this.disposedValue = true;
            }
        }
    }
}
