using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

using UNIIAadminAPI.Serializers;
using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using System.Security.Cryptography.X509Certificates;

namespace UNIIAadminAPI.Models
{
    public class PublicationLanguage
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [StringLength(32)]
        [Display(Name = "Назва")]
        public string Name { get; set; }

        public PublicationLanguage() 
        {
            
        }

        public PublicationLanguage(PublicationLanguageDto publicationLanguageDto)
        {
            Name = publicationLanguageDto.Name;
        }
        public void UpdateByDtoModel(PublicationLanguageDto publicationLanguageDto)
        {
            Name = publicationLanguageDto.Name;
        }
    }
}
