namespace UniiaAdmin.Data.Models
{
    public class Favorite
    {
        public int Id { get; set; }

        public string? UserId { get; set; }

        public int PublicationId { get; set; }

        public User? User { get; set; }

        public DateTime AddedOn { get; set; }
    }
}
