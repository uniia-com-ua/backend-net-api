using MongoDB.Bson;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Models;

namespace UNIIAadminAPI.Services
{
    public class AuthService(HttpContext httpContext) : IDisposable
    {
        private readonly MongoDbContext _db = httpContext.RequestServices.GetRequiredService<MongoDbContext>();
        private readonly HttpContext _httpContext = httpContext;
        private bool disposedValue;

        public async Task AddLoginInfoToHistory(AdminUser adminUser)
        {
            _httpContext.Request.Headers.TryGetValue("X-Real-IP", out var ipAdress);

            ipAdress.ToString();

            AdminLogInHistory logInHistoryItem = new()
            {
                Id = ObjectId.GenerateNewId(),
                UserId = adminUser.Id,
                LogInType = "Credential login",
                LogInTime = DateTime.UtcNow,
                IpAdress = ipAdress,
                UserAgent = _httpContext.Request.Headers.UserAgent
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
