using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace UNIIAadminAPI.Models
{
    public class BrowsingHistory
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public Guid UserId { get; set; }
        public ObjectId PublicationId { get; set; }

        [Required]
        [Display(Name = "Часова позначка")]
        public DateTime Timestamp { get; set; }
    }
}
