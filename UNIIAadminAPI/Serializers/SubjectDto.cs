using UNIIAadminAPI.Models;

namespace UNIIAadminAPI.Serializers
{
    public class SubjectDto
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public SubjectDto(Subject subject)
        {
            Id = subject.Id.ToString();
            Name = subject.Name;
        }
        public SubjectDto()
        {

        }
    }
}
