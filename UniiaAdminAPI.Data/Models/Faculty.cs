using System.ComponentModel.DataAnnotations;
using UniiaAdmin.Data.Dtos;

namespace UniiaAdmin.Data.Models
{
    public class Faculty
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string? FullName { get; set; }

        [Required]
        [MaxLength(60)]
        public string? ShortName { get; set; }

        public int UniversityId { get; set; }

        public University? University { get; set; }

        public void Update(FacultyDto faculty)
        {
            FullName = faculty.FullName;
            ShortName = faculty.ShortName;
            UniversityId = faculty.UniversityId;
        }
    }
}
