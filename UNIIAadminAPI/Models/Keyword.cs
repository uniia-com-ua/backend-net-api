using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using UNIIAadminAPI.Serializers;

namespace UNIIAadminAPI.Models
{
    public class Keyword
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [MaxLength(48)]
        [Display(Name = "Ключове Слово")]
        public string Word { get; set; }

        public Keyword() 
        { 

        }
        public Keyword(KeywordDto keywordDto) 
        { 
            Word = keywordDto.Word;
        }
        public void UpdateByDtoModel(KeywordDto keywordDto)
        {
            Word = keywordDto.Word;
        }
    }
}
