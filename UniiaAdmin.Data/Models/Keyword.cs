using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using UniiaAdmin.Data.Interfaces.FileInterfaces;

namespace UniiaAdmin.Data.Models
{
    public class Keyword : IEntity
	{
        public int Id { get; set; }

        [MaxLength(48)]
        public string? Word { get; set; }

		[JsonIgnore]
		[BindNever]
		public List<Publication>? Publications { get; set; }
	}
}
