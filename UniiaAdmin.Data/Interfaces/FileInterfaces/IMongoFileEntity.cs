using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniiaAdmin.Data.Interfaces.FileInterfaces
{
    public interface IMongoFileEntity
    {
        public ObjectId Id { get; set; }
        public byte[]? File { get; set; }
    }
}
