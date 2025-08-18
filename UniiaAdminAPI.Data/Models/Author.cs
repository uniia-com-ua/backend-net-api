using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using UniiaAdmin.Data.Interfaces.FileInterfaces;

namespace UniiaAdmin.Data.Models
{
    public class Author : IPhotoEntity
	{
        public int Id { get; set; }

        [MaxLength(100)]
        public string? FullName { get; set; }

        [Required]
        [MaxLength(48)]
        public string? ShortName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(50)]
        public string? OrcidId { get; set; }

        public string? Bio { get; set; }

        [JsonIgnore]
        [BindNever]
        public string? PhotoId { get; set; } 

        [MaxLength(64)]
        public string? Url { get; set; }
    }
}
