using AspNetCore.Identity.MongoDbCore.Models;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;
using UNIIAadminAPI.Models;

namespace UNIIAadminAPI.Serializers
{
    public class AdminUserDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastSingIn { get; set; }
        public bool IsOnline { get; set; }
        public string UserDescription { get; set; }

        public AdminUserDto() { }

        public AdminUserDto(AdminUser adminUser, MongoIdentityUser identityUser)
        {
            Id = adminUser.Id.ToString();
            Name = adminUser.Name;
            Surname = adminUser.Surname;
            Email = identityUser.Email;
            CreatedOn = identityUser.CreatedOn;
            LastSingIn = adminUser.LastSingIn;
            IsOnline = adminUser.IsOnline;
            UserDescription = adminUser.UserDescription;
        }
    }
}
