using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using UniiaAdmin.Data.Dtos;
using UniiaAdmin.Data.Enums;

namespace UniiaAdmin.Data.Models
{
    public class PublicationDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }

        public string? Annotation { get; set; }

        public string? AnnotationFormat { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public int PublicationYear { get; set; }

        public int Pages { get; set; }

        public string? Publisher { get; set; }

        public string? ISBN { get; set; }

        public string? DOI { get; set; }

        public string? LicenseURL { get; set; }

        public string? URL { get; set; }

        public string? FileId { get; set; }

        public PublicationStatus Status { get; set; }

        public int PublicationTypeId { get; set; }

        public PublicationType? PublicationType { get; set; }

        public int PublicationLanguageId { get; set; }

        public PublicationLanguage? Language { get; set; }

        public List<AuthorDto>? Authors { get; set; }

        public List<Subject>? Subjects { get; set; }

        public List<Keyword>? Keywords { get; set; }

        public List<CollectionPublication>? CollectionPublications { get; set; }
    }
}
