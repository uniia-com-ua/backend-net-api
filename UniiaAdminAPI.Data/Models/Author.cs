using System.ComponentModel.DataAnnotations;
using UniiaAdmin.Data.Dtos;

namespace UniiaAdmin.Data.Models
{
    public class Author
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string? FullName { get; set; }

        [Required]
        [MaxLength(48)]
        public string? ShortName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(50)]
        public string? OrcidId { get; set; }

        public string? Bio { get; set; }

        public string? PhotoId { get; set; } 

        [MaxLength(64)]
        public string? Url { get; set; }

        public void Update(AuthorDto authordto)
        {
            FullName = string.IsNullOrWhiteSpace(authordto.FullName) ? FullName : authordto.FullName;
            ShortName = string.IsNullOrWhiteSpace(authordto.ShortName) ? ShortName : authordto.ShortName;
            Email = string.IsNullOrWhiteSpace(authordto.Email) ? Email : authordto.Email;
            OrcidId = string.IsNullOrWhiteSpace(authordto.OrcidId) ? OrcidId : authordto.OrcidId;
            Bio = string.IsNullOrWhiteSpace(authordto.Bio) ? Bio : authordto.Bio;
            Url = string.IsNullOrWhiteSpace(authordto.Url) ? Url : authordto.Url;
        }
    }
}
