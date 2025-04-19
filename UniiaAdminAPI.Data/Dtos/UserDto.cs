using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniiaAdmin.Data.Dtos
{
    public class UserDto
    {
        public string? Id { get; set; }

        public string? Email { get; set; }

        public DateTime LastSingIn { get; set; }

        public bool IsOnline { get; set; }

        public string? Name { get; set; }

        public string? Surname { get; set; }

        public string? UserDescription { get; set; }

        public List<string> Roles { get; set; } = new List<string>();
    }
}