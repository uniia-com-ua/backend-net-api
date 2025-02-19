using System.ComponentModel.DataAnnotations;

namespace UniiaAdmin.Data.Models
{
    public class Keyword
    {
        public int Id { get; set; }

        [MaxLength(48)]
        public string? Word { get; set; }
    }
}
