using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniiaAdmin.Data.Interfaces;

namespace UniiaAdmin.Data.Models
{
    public class AdminUserPhoto : IMongoFileEntity
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public byte[]? File { get; set; }
    }
}
