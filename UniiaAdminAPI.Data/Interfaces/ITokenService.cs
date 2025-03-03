using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniiaAdmin.Data.Models;

namespace UniiaAdmin.Data.Interfaces
{
    public interface ITokenService
    {
        public Task<bool> ValidateGoogleTokenAsync(AdminUser user, UserManager<AdminUser> userManager);

        public string EncryptToken(string input);
    }
}
