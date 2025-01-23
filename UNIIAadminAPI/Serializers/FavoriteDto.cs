using UNIIAadminAPI.Models;

namespace UNIIAadminAPI.Serializers
{
    public class FavoriteDto
    {
        public string Id { get; set; }
        public string PublicationId { get; set; }
        public FavoriteDto(Favorite favorite)
        {
            Id = favorite.Id.ToString();
            PublicationId = favorite.PublicationId.ToString();
        }
        public FavoriteDto() 
        { 

        }
    }
}
