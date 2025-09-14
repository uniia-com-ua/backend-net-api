using Microsoft.AspNetCore.Identity;

namespace UniiaAdmin.Data.Models
{
    public class AdminUser : IdentityUser
	{
        public DateTime LastSingIn { get; set; }

        public bool IsOnline { get; set; }

        public string? Name { get; set; }

        public string? Surname { get; set; }

        public string? ProfilePictureId { get; set; }

        public string? UserDescription { get; set; }

        public string? RefreshTokenExpiryTime { get; set; }
	}
}