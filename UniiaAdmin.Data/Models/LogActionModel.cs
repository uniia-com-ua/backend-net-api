using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace UniiaAdmin.Data.Models
{
    public class LogActionModel
    {
        [BsonId]
        [JsonIgnore]
        public ObjectId Id { get; set; }

        public string? UserId { get; set; }

        public int ModelId { get; set; }

        public string? ModelName { get; set; }

        public string? ModelAction { get; set; }

        public DateTime ChangedTime { get; set; }
    }
}
