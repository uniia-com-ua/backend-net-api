using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using UniiaAdmin.Data.Enums;
using UniiaAdmin.Data.Interfaces.FileInterfaces;

namespace UniiaAdmin.Data.Models
{
    public class Publication : IFileEntity
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string? Title { get; set; }

        [Required]
        public string? Annotation { get; set; }

        public string? AnnotationFormat { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        [Required]
        public int PublicationYear { get; set; }

        [Required]
        public int Pages { get; set; }

        [MaxLength(255)]
        public string? Publisher { get; set; }

        [MaxLength(20)]
        public string? ISBN { get; set; }

        [MaxLength(100)]
        public string? DOI { get; set; }

        [MaxLength(100)]
        public string? LicenseURL { get; set; }

        [MaxLength(100)]
        public string? URL { get; set; }

        [BindNever]
        [JsonIgnore]
        public string? FileId { get; set; }

        public PublicationStatus Status { get; set; }

        public int PublicationTypeId { get; set; }

		[BindNever]
		public PublicationType? PublicationType { get; set; }

        public int PublicationLanguageId { get; set; }

		[BindNever]
		public PublicationLanguage? Language { get; set; }

		[BindNever]
		public List<Author>? Authors { get; set; }

		[BindNever]
		public List<Subject>? Subjects { get; set; }

		[BindNever]
		public List<Keyword>? Keywords { get; set; }

		[BindNever]
		[JsonIgnore]
		public List<CollectionPublication>? CollectionPublications { get; set; }

		[BindNever]
		public List<PublicationRoleClaim>? PublicationRoleClaims { get; set; }
    }
}
