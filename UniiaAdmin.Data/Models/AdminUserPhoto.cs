using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using UniiaAdmin.Data.Interfaces.FileInterfaces;

namespace UniiaAdmin.Data.Models
{
    public class AdminUserPhoto : IMongoFileEntity
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public byte[]? File { get; set; }
    }
}
