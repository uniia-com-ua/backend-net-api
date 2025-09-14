using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using UniiaAdmin.Data.Interfaces.FileInterfaces;

namespace UniiaAdmin.Data.Models
{
    public class PublicationLanguage : IEntity
	{
        public int Id { get; set; }

        [MaxLength(32)]
        public string? Name { get; set; }

		[JsonIgnore]
		[BindNever]
		public List<Publication>? Publications { get; set; }
	}
}
