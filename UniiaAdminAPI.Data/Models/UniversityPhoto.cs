using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using UniiaAdmin.Data.Interfaces;

namespace UniiaAdmin.Data.Models
{
    public class UniversityPhoto : IMongoFileEntity
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public byte[]? File { get; set; }
    }
}
