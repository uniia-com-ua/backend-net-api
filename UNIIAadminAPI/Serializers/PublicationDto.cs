using MongoDB.Bson;
using UNIIAadminAPI.Enums;
using UNIIAadminAPI.Models;

namespace UNIIAadminAPI.Serializers
{
    public class PublicationDto
    {
        public string Title { get; set; }
        public string Annotation { get; set; }
        public string AnnotationFormat { get; set; }
        public ObjectId? PublicationType { get; set; }
        public int PublicationYear { get; set; }
        public ObjectId[]? Authors { get; set; }
        public ObjectId[]? Subjects { get; set; }
        public ObjectId? Language { get; set; }
        public int Pages { get; set; }
        public string Publisher { get; set; }
        public string Isbn { get; set; }
        public string Doi { get; set; }
        public string LicenseUrl { get; set; }
        public ObjectId[]? Keywords { get; set; }
        public IFormFile File { get; set; }
        public string Url { get; set; }
        public PublicationStatus Status { get; set; }

        public PublicationDto() 
        { 
            
        }

        public PublicationDto(Publication publication)
        {
            Title = publication.Title;
            Annotation = publication.Annotation;
            AnnotationFormat = publication.AnnotationFormat;
            PublicationType = publication.PublicationTypeId;
            PublicationYear = publication.PublicationYear;
            Authors = publication.Authors;
            Subjects = publication.Subjects;
            Language = publication.PublicationLanguageId;
            Pages = publication.Pages;
            Publisher = publication.Publisher;
            Isbn = publication.ISBN;
            Doi = publication.DOI;
            LicenseUrl = publication.LicenseURL;
            Keywords = publication.Keywords;
            Url = publication.URL;
            Status = publication.Status;
        }
    }
}
