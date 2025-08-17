using System.ComponentModel.DataAnnotations;
using UniiaAdmin.Data.Interfaces.FileInterfaces;

namespace UniiaAdmin.Data.Models
{
    public class Keyword : IEntity
	{
        public int Id { get; set; }

        [MaxLength(48)]
        public string? Word { get; set; }
    }
}
