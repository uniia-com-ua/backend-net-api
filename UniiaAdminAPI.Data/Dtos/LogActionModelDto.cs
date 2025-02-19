using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniiaAdmin.Data.Models
{
    public class LogActionModelDto
    {
        public string? UserId { get; set; }
        public int ModelId { get; set; }
        public string? ModelName { get; set; }
        public string? ModelAction { get; set; }
        public DateTime ChangedTime { get; set; }
    }
}
