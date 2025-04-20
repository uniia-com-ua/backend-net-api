using Microsoft.AspNetCore.Http;
using UniiaAdmin.Data.Models;

namespace UniiaAdmin.Auth.Interfaces
{
    public interface IAuthService
    {
        public Task AddLoginInfoToHistory(AdminUser adminUser, HttpContext httpContext);
    }
}
