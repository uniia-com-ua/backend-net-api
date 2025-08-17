using System.ComponentModel.DataAnnotations;
using UniiaAdmin.Data.Interfaces.FileInterfaces;

namespace UniiaAdmin.Data.Models
{
    public class PublicationLanguage : IEntity
	{
        public int Id { get; set; }

        [MaxLength(32)]
        public string? Name { get; set; }
    }
}
