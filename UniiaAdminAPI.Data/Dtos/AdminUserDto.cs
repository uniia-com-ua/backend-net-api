namespace UniiaAdmin.Data.Dtos
{
    public class AdminUserDto
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