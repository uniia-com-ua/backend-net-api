using System.Text.Json;
using System.Xml.Linq;
using UNIIAadminAPI.Models;

namespace UNIIAadminAPI.Serializers
{
    public class FacultyDto
    {
        public string fullName { get; set; }
        public string shortName { get; set; }
        public bool catalogView { get; set; }
        public FacultyDto() 
        { 

        }
        public FacultyDto(Faculty faculty)
        {
            fullName = faculty.FullName;
            shortName = faculty.ShortName;
            catalogView = faculty.CatalogView;
        }
    }
}
