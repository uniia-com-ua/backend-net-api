using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace UNIIAadminAPI.Models
{
    public class User
    {
        [BsonId]
        public ObjectId Id { get; set; }
        
        [Display(Name = "Id користувача для автентифікції")]
        public Guid AuthUserId { get; set; }
        
        [Display(Name = "Ім'я")]
        public string? FirstName { get; set; }

        [Display(Name = "Прізвище")]
        public string? LastName { get; set; }

        [Display(Name = "По батькові")]
        public string? AdditionalName { get; set; }

        [Display(Name = "Факультет")]
        public ObjectId FacultyId { get; set; }

        [Display(Name = "Дата народження")]
        public DateTime BirthDate { get; set; }

        [Display(Name = "Вибрані")]
        [BsonIgnoreIfNull]
        public List<ObjectId>? Favorites { get; set; }

        [Display(Name = "Активний")]
        public bool IsActive { get; set; }
        
        [BsonIgnoreIfNull]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId[]? Authors { get; set; }
    }
}