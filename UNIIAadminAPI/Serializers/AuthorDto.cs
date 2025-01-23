using UNIIAadminAPI.Models;

namespace UNIIAadminAPI.Serializers
{
    public class AuthorDto
    {
        public string Short_name { get; set; }
        public string Full_name { get; set; }
        public string Email { get; set; }
        public string Orcid_id { get; set; }
        public string Bio { get; set; }
        public string Url { get; set; }
        public IFormFile Photo_file{ get; set; }
        public AuthorDto(Author author, IFormFile file) 
        { 
            Short_name = author.ShortName; 
            Full_name = author.FullName;
            Orcid_id = author.OrcidId; 
            Bio = author.Bio; 
            Url = author.Url;
            Photo_file = file;
        }
        public AuthorDto() 
        {

        }
    }
}
