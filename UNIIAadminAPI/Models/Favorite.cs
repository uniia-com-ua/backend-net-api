using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace UNIIAadminAPI.Models
{
    public class Favorite
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public Guid UserId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId PublicationId { get; set; }

        [Required]
        [Display(Name = "Додано")]
        public DateTime AddedOn { get; set; }
    }
}
