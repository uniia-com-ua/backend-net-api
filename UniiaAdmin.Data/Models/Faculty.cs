using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using UniiaAdmin.Data.Interfaces.FileInterfaces;

namespace UniiaAdmin.Data.Models
{
    public class Faculty : IEntity
	{
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string? FullName { get; set; }

        [Required]
        [MaxLength(60)]
        public string? ShortName { get; set; }

        public int UniversityId { get; set; }

		[BindNever]
		[JsonIgnore]
        public University? University { get; set; }

		[BindNever]
		[JsonIgnore]
		public virtual ICollection<Specialty>? Specialties { get; set; }
	}
}
