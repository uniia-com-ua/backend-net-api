using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UniiaAdmin.Data.Models
{
    public class HistoryLog
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public int UserId { get; set; }

        public int PublicationId { get; set; }

        public ushort LastViewedPage { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
