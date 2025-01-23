using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;
using UNIIAadminAPI.Serializers;

namespace UNIIAadminAPI.Models
{
    public class Author
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [MaxLength(100)]
        [Display(Name = "Повне ім'я")]
        public string FullName { get; set; }

        [Required]
        [MaxLength(48)]
        [Display(Name = "Скорочено")]
        public string ShortName { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [MaxLength(50)]
        public string OrcidId { get; set; }

        public string Bio { get; set; }

        [MaxLength(200)]
        public ObjectId PhotoId { get; set; } 

        [MaxLength(64)]
        [Display(Name = "Посилання")]
        public string Url { get; set; }

        public Author()
        {

        }
        public Author(AuthorDto authorDto, ObjectId photoId)
        {
            FullName = authorDto.Full_name;
            ShortName = authorDto.Short_name;
            Email = authorDto.Email;
            OrcidId = authorDto.Orcid_id;
            Bio = authorDto.Bio;
            PhotoId = photoId;
            Url = authorDto.Url;
        }
        public void UpdateByDtoModel(AuthorDto authorDto)
        {
            FullName = authorDto.Full_name;
            ShortName = authorDto.Short_name;
            Email = authorDto.Email;
            OrcidId = authorDto.Orcid_id;
            Bio = authorDto.Bio;
            Url = authorDto.Url;
        }
    }
}
