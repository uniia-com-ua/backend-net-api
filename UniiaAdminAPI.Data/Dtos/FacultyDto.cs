using System.ComponentModel.DataAnnotations;
using UniiaAdmin.Data.Models;

namespace UniiaAdmin.Data.Dtos
{
    public class FacultyDto
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? ShortName { get; set; }
        public int UniversityId { get; set; }
    }
}
