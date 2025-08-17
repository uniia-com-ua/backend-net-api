using System.ComponentModel.DataAnnotations;
using UniiaAdmin.Data.Interfaces.FileInterfaces;

namespace UniiaAdmin.Data.Models
{
    public class Subject : IEntity
	{
        public int Id { get; set; }

        [Required]
        [MaxLength(64)]
        public string? Name { get; set; }
    }
}