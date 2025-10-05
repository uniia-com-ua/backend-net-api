using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniiaAdmin.Data.Models
{
    public class Student
    {
        [Key]
        public string? UserId { get; set; }

        public string? UniversityEmail { get; set; }

        public int FacultyId { get; set; }

        public Faculty? Faculty { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
