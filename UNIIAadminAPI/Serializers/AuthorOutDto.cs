using UNIIAadminAPI.Models;

namespace UNIIAadminAPI.Serializers
{
    public class AuthorOutDto
    {
        public string Id { get; set; }
        public string Short_name { get; set; }
        public string Url { get; set; }
        public AuthorOutDto(Author author)
        {
            Id = author.Id.ToString();
            Short_name = author.ShortName;
            Url = author.Url;
        }
        public AuthorOutDto()
        {

        }
    }
}
