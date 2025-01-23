using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace UNIIAadminAPI.Models
{
    public class AdminUser
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [Display(Name = "Id користувача для автентифікції")]
        public Guid AuthUserId { get; set; }

        [Display(Name = "Останній вхід")]
        public DateTime LastSingIn { get; set; }

        [Display(Name = "Статус онлайн")]
        public bool IsOnline { get; set; }

        [Display(Name = "Ім'я")]
        public string? Name { get; set; }

        [Display(Name = "Прізвище")]
        public string? Surname { get; set; }

        [BsonIgnoreIfNull]
        [Display(Name = "Зображення профілю")]
        public byte[]? ProfilePicture { get; set; }

        [Display(Name = "Опис профілю")]
        public string? UserDescription { get; set; }
    }
}