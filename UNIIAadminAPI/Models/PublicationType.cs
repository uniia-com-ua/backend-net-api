using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;
using UNIIAadminAPI.Serializers;
using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using System.Security.Cryptography.X509Certificates;

namespace UNIIAadminAPI.Models
{
    public class PublicationType
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [Required]
        [MaxLength(32)]
        [Display(Name = "Назва")]
        public string Name { get; set; }

        public PublicationType(PublicationTypeDto publicationTypeDto)
        {
            Name = publicationTypeDto.Name;
        }
        public void UpdateByDtoModel(PublicationTypeDto publicationTypeDto)
        {
            Name = publicationTypeDto.Name;
        }
    }
}
