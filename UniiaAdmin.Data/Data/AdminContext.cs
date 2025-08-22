using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UniiaAdmin.Data.Models;

namespace UniiaAdmin.Data.Data
{
    public class AdminContext : IdentityDbContext<AdminUser>
    {
        public AdminContext(DbContextOptions<AdminContext> dbContextOptions) : base(dbContextOptions) { }

        public AdminContext() : base() { }
    }
}
