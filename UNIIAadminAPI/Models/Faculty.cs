using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;
using UNIIAadminAPI.Serializers;
using Amazon.Runtime.Internal.Endpoints.StandardLibrary;

namespace UNIIAadminAPI.Models
{
    public class Faculty
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Повна назва")]
        public string FullName { get; set; }

        [Required]
        [MaxLength(60)]
        [Display(Name = "Скорочено")]
        public string ShortName { get; set; }

        [Display(Name = "Вкладка в темах")]
        public bool CatalogView { get; set; }
        
        [BsonIgnoreIfNull]
        public ObjectId[] ? Subjects { get; set; } = null;

        public Faculty() 
        { 

        }

        public Faculty(FacultyDto facultyDto)
        {
            FullName = facultyDto.fullName;
            ShortName = facultyDto.shortName;
            CatalogView = facultyDto.catalogView;
        }
        public void UpdateByDtoModel(FacultyDto facultyDto)
        {
            FullName = facultyDto.fullName;
            ShortName = facultyDto.shortName;
            CatalogView = facultyDto.catalogView;
        }
    }
}
