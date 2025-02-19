using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using UniiaAdmin.Data.Enums;

namespace UniiaAdmin.Data.Models
{
    public class User : IdentityUser
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? AdditionalName { get; set; }

        public DateTime BirthDate { get; set; }

        public DateTime RegistrationDate { get; set; }

        public DateTime DateOfApprovement { get; set; }

        public string? PreferredLanguage { get; set; }

        public AccountStatus AccountStatus { get; set; }

        public List<Favorite>? Favorites { get; set; }
    }
}