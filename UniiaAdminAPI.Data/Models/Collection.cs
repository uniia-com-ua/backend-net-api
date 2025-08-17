using UniiaAdmin.Data.Enums;
using UniiaAdmin.Data.Interfaces.FileInterfaces;

namespace UniiaAdmin.Data.Models
{
    public class Collection : IEntity
	{
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime LastModified { get; set; }

        public int CoverImage { get; set; }

        public CollectionStatus CollectionStatus { get; set; }
        
        public int ViewCount { get; set; }

        public string? Slug { get; set; }

        public AccessType AccessType { get; set; }

        public List<User>? SharedWith { get; set; }

        public string? UserId { get; set; }

        public User? User { get; set; }

        public List<CollectionPublication>? CollectionPublications { get; set; }
    }
}
