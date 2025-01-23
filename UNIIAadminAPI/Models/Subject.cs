using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

using UNIIAadminAPI.Serializers;
using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using System.Security.Cryptography.X509Certificates;

namespace UNIIAadminAPI.Models
{
    public class Subject
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [Required]
        [StringLength(64)]
        [Display(Name = "Назва")]
        public string Name { get; set; }

        [Display(Name = "Дотичні факультети")]
        [BsonIgnoreIfNull]
        public ObjectId[]? Faculties { get; set; } = null;

        public Subject() 
        { 

        }

        public Subject(SubjectDto subjectDto)
        {
            Name = subjectDto.Name;
        }

        public void UpdateByDtoModel(SubjectDto subjectDto)
        {
            Name = subjectDto.Name;
        }
    }
}