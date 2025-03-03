using System;
using UniiaAdmin.Data.Dtos;

namespace UniiaAdmin.Data.Models
{
    public class University
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? ShortName { get; set; }
        public string? PhotoId { get; set; }
        public string? SmallPhotoId { get; set; }
        public List<string>? Domens { get; set; }
        public List<Faculty>? Faculties { get; set; }

        public void Update(UniversityDto university)
        {
            Name = string.IsNullOrWhiteSpace(university.Name) ? Name : university.Name;
            ShortName = string.IsNullOrWhiteSpace(university.ShortName) ? ShortName : university.ShortName;
        }
    }
}
