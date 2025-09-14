﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniiaAdmin.Data.Models
{
    public class Student
    {
        [Key]
        public string? UserId { get; set; }

        [Display(Name = "Університетський Email")]
        public string? UniversityEmail { get; set; }

        [Display(Name = "Факультет")]
        public int FacultyId { get; set; }

        [Display(Name = "Факультет")]
        public Faculty? Faculty { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
