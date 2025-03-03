using Microsoft.AspNetCore.Http;
using UniiaAdmin.Data.Models;

namespace UniiaAdmin.Data.Interfaces
{
    public interface IAuthService
    {
        public Task AddLoginInfoToHistory(AdminUser adminUser, HttpContext httpContext);
    }
}
