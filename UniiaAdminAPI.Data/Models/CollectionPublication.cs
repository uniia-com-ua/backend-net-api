namespace UniiaAdmin.Data.Models
{
    public class CollectionPublication
    {
        public int CollectionId { get; set; }
        
        public int PublicationId { get; set; }

        public PublicationDto? Publication { get; set; }

        public Collection? Collection { get; set; }

        public DateTime AddedTime { get; set; }

        public string? AddedBy { get; set; }

        public string? Notes { get; set; }

        public int OrderIndex { get; set; }
    }
}
