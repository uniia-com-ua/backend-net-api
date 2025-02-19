using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using UniiaAdmin.Data.Enums;

namespace UniiaAdmin.Data.Models
{
    public class Publication
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

        public string? FileId { get; set; }

        public PublicationStatus Status { get; set; }

        public int PublicationTypeId { get; set; }

        public PublicationType? PublicationType { get; set; }

        public int PublicationLanguageId { get; set; }

        public PublicationLanguage? Language { get; set; }

        public List<Author>? Authors { get; set; }

        public List<Subject>? Subjects { get; set; }

        public List<Keyword>? Keywords { get; set; }

        public List<CollectionPublication>? CollectionPublications { get; set; }

        public List<PublicationRoleClaim>? PublicationRoleClaims { get; set; }

        public void Update(PublicationDto newPublicationDto)
        {
            Title = string.IsNullOrWhiteSpace(newPublicationDto.Title) ? Title : newPublicationDto.Title;

            Annotation = string.IsNullOrWhiteSpace(newPublicationDto.Annotation) ? Annotation : newPublicationDto.Annotation;

            AnnotationFormat = string.IsNullOrWhiteSpace(newPublicationDto.AnnotationFormat) ? AnnotationFormat : newPublicationDto.AnnotationFormat;

            CreatedDate = newPublicationDto.CreatedDate == default ? CreatedDate : newPublicationDto.CreatedDate;

            LastModifiedDate = newPublicationDto.LastModifiedDate == default ? LastModifiedDate : newPublicationDto.LastModifiedDate;

            PublicationYear = newPublicationDto.PublicationYear == 0 ? PublicationYear : newPublicationDto.PublicationYear;

            Pages = newPublicationDto.Pages == 0 ? Pages : newPublicationDto.Pages;

            Publisher = string.IsNullOrWhiteSpace(newPublicationDto.Publisher) ? Publisher : newPublicationDto.Publisher;

            ISBN = string.IsNullOrWhiteSpace(newPublicationDto.ISBN) ? ISBN : newPublicationDto.ISBN;

            DOI = string.IsNullOrWhiteSpace(newPublicationDto.DOI) ? DOI : newPublicationDto.DOI;

            LicenseURL = string.IsNullOrWhiteSpace(newPublicationDto.LicenseURL) ? LicenseURL : newPublicationDto.LicenseURL;

            URL = string.IsNullOrWhiteSpace(newPublicationDto.URL) ? URL : newPublicationDto.URL;

            FileId = string.IsNullOrWhiteSpace(newPublicationDto.FileId) ? FileId : newPublicationDto.FileId;

            Status = newPublicationDto.Status == default ? Status : newPublicationDto.Status;

            PublicationTypeId = newPublicationDto.PublicationTypeId == 0 ? PublicationTypeId : newPublicationDto.PublicationTypeId;

            PublicationType = newPublicationDto.PublicationType ?? PublicationType;

            PublicationLanguageId = newPublicationDto.PublicationLanguageId == 0 ? PublicationLanguageId : newPublicationDto.PublicationLanguageId;

            Language = newPublicationDto.Language ?? Language;

            Subjects = newPublicationDto.Subjects ?? Subjects;

            Keywords = newPublicationDto.Keywords ?? Keywords;

            CollectionPublications = newPublicationDto.CollectionPublications ?? CollectionPublications;
        }

    }
}
