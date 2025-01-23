using UNIIAadminAPI.Models;

namespace UNIIAadminAPI.Serializers
{
    public class PublicationLanguageDto
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public PublicationLanguageDto(PublicationLanguage publicationLanguage)
        {
            Id = publicationLanguage.Id.ToString();
            Name = publicationLanguage.Name;
        }
        public PublicationLanguageDto() 
        { 

        }
    }
}
