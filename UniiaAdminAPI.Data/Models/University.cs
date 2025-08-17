using System.Text.Json.Serialization;
using UniiaAdmin.Data.Interfaces.FileInterfaces;

namespace UniiaAdmin.Data.Models
{
    public class University : IEntity
	{
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? ShortName { get; set; }

        [JsonIgnore]
        public string? PhotoId { get; set; }
		
        [JsonIgnore]
		public string? SmallPhotoId { get; set; }

        [JsonIgnore]
        public List<string>? Domens { get; set; }

		[JsonIgnore]
		public List<Faculty>? Faculties { get; set; }
    }
}
