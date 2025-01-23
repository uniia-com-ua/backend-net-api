using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GridFS;
using System.ComponentModel.DataAnnotations;
using UNIIAadminAPI.Enums;

using UNIIAadminAPI.Serializers;

namespace UNIIAadminAPI.Models
{
    public class Publication
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [Required]
        [MaxLength(200)]
        [Display(Name = "Назва")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Опис")]
        public string Annotation { get; set; }
        public string AnnotationFormat { get; set; }

        [Display(Name = "Тип")]
        public ObjectId? PublicationTypeId { get; set; }

        [Display(Name = "Мова")]
        public ObjectId? PublicationLanguageId { get; set; }

        [Required]
        [Display(Name = "Рік видання")]
        public int PublicationYear { get; set; }

        [Required]
        [Display(Name = "Кількість сторінок")]
        public int Pages { get; set; }

        [MaxLength(255)]
        [Display(Name = "Видавництво")]
        public string Publisher { get; set; }

        [MaxLength(20)]
        [Display(Name = "ISBN")]
        public string ISBN { get; set; }

        [MaxLength(100)]
        [Display(Name = "DOI")]
        public string DOI { get; set; }

        [MaxLength(100)]
        [Display(Name = "Ліцензія")]
        public string LicenseURL { get; set; }

        [MaxLength(100)]
        [Display(Name = "URL")]
        public string URL { get; set; }
        public ObjectId FileId { get; set; }

        [Display(Name = "Статус")]
        public PublicationStatus Status { get; set; }

        [Display(Name = "Автори")]
        public ObjectId[]? Authors { get; set; }

        [Display(Name = "Предмети")]
        public ObjectId[]? Subjects { get; set; }

        [Display(Name = "Ключові слова")]
        public ObjectId[]? Keywords { get; set; }

        public Publication()
        {

        }

        public Publication(PublicationDto updatePublicationDto, ObjectId fileId)
        {
            Title = updatePublicationDto.Title;
            Annotation = updatePublicationDto.Annotation;
            AnnotationFormat = updatePublicationDto.AnnotationFormat;
            PublicationTypeId = updatePublicationDto.PublicationType;
            PublicationLanguageId = updatePublicationDto.Language;
            PublicationYear = updatePublicationDto.PublicationYear;
            Pages = updatePublicationDto.Pages;
            Publisher = updatePublicationDto.Publisher;
            ISBN = updatePublicationDto.Isbn;
            DOI = updatePublicationDto.Doi;
            LicenseURL = updatePublicationDto.LicenseUrl;
            URL = updatePublicationDto.Url;
            Status = updatePublicationDto.Status;
            Authors = updatePublicationDto.Authors;
            Subjects = updatePublicationDto.Subjects;
            Keywords = updatePublicationDto.Keywords;
            FileId = fileId;
        }
        public void UpdateByDtoModel(PublicationDto updatePublicationDto)
        {
            Title = updatePublicationDto.Title;
            Annotation = updatePublicationDto.Annotation;
            AnnotationFormat = updatePublicationDto.AnnotationFormat;
            PublicationTypeId = updatePublicationDto.PublicationType;
            PublicationLanguageId = updatePublicationDto.Language;
            PublicationYear = updatePublicationDto.PublicationYear;
            Pages = updatePublicationDto.Pages;
            Publisher = updatePublicationDto.Publisher;
            ISBN = updatePublicationDto.Isbn;
            DOI = updatePublicationDto.Doi;
            LicenseURL = updatePublicationDto.LicenseUrl;
            URL = updatePublicationDto.Url;
            Status = updatePublicationDto.Status;
            Authors = updatePublicationDto.Authors;
            Subjects = updatePublicationDto.Subjects;
            Keywords = updatePublicationDto.Keywords;
        }
    }
}
