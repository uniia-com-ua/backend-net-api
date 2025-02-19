using System.ComponentModel.DataAnnotations;

namespace UniiaAdmin.Data.Models
{
    public class PublicationType
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(32)]
        public string? Name { get; set; }
    }
}
