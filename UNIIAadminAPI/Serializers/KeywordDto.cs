using UNIIAadminAPI.Models;

namespace UNIIAadminAPI.Serializers
{
    public class KeywordDto
    {
        public string Id { get; set; }
        public string Word { get; set; }
        public KeywordDto(Keyword keyword) 
        { 
            Id = keyword.Id.ToString();
            Word = keyword.Word;
        }
        public KeywordDto()
        {

        }
    }
}
