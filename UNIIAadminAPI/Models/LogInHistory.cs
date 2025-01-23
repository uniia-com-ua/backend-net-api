using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace UNIIAadminAPI.Models
{
    public class LogInHistory
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public ObjectId UserId { get; set; }

        [Display(Name = "IP адреса")]
        public string? IpAdress { get; set; }

        [Display(Name = "Тип логіну")]
        public string? LogInType { get; set; }

        [Display(Name = "Час входу")]
        public DateTime LogInTime { get; set; }

        [Display(Name = "User agent")]
        public string? UserAgent { get; set; }
    }
}
