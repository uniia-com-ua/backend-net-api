using System.ComponentModel.DataAnnotations;
using UniiaAdmin.Data.Interfaces.FileInterfaces;

namespace UniiaAdmin.Data.Models
{
    public class PublicationType : IEntity
	{
        public int Id { get; set; }

        [Required]
        [MaxLength(32)]
        public string? Name { get; set; }
    }
}
