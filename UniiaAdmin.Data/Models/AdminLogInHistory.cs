using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace UniiaAdmin.Data.Models
{
    public class AdminLogInHistory
    {
        [BsonId]
        [JsonIgnore]
        public ObjectId Id { get; set; }

		[JsonIgnore]
		public string? UserId { get; set; }

        public string? IpAdress { get; set; }

        public string? LogInType { get; set; }

        public DateTime LogInTime { get; set; }

        public string? UserAgent { get; set; }
    }
}
