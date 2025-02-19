using System.ComponentModel.DataAnnotations;

namespace UniiaAdmin.Data.Models
{
    public class PublicationLanguage
    {
        public int Id { get; set; }

        [MaxLength(32)]
        public string? Name { get; set; }
    }
}
