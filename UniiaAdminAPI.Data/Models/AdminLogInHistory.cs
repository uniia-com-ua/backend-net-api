using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace UniiaAdmin.Data.Models
{
    public class AdminLogInHistory
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public string? UserId { get; set; }

        public string? IpAdress { get; set; }

        public string? LogInType { get; set; }

        public DateTime LogInTime { get; set; }

        public string? UserAgent { get; set; }
    }
}
