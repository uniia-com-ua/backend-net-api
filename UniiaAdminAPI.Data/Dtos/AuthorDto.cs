using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UniiaAdmin.Data.Dtos
{
    public class AuthorDto
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string? FullName { get; set; }

        [MaxLength(48)]
        public string? ShortName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(50)]
        public string? OrcidId { get; set; }

        public string? Bio { get; set; }

        [MaxLength(64)]
        public string? Url { get; set; }
    }
}
