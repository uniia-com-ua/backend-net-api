using System.ComponentModel.DataAnnotations;

namespace UniiaAdmin.Data.Models
{
    public class Subject
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(64)]
        public string? Name { get; set; }
    }
}