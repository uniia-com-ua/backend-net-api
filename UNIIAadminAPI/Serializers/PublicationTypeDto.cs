using UNIIAadminAPI.Models;

namespace UNIIAadminAPI.Serializers
{
    public class PublicationTypeDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public PublicationTypeDto(PublicationType publicationType)
        {
            Id=publicationType.Id.ToString();
            Name=publicationType.Name;
        }
        public PublicationTypeDto() 
        { 

        }
    }
}
