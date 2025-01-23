using AspNetCore.Identity.MongoDbCore.Models;
using UNIIAadminAPI.Models;

namespace UNIIAadminAPI.Serializers
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Full_name { get; set; }
        public FacultyDto? Faculty { get; set; }
        public DateTime Birth_date { get; set; }
        public string Phone { get; set; }
        public bool Need_register { get; set; }
        public bool Password_set { get; set; }
        public DateTime Date_joined { get; set; }
        public IQueryable<string>? Groups { get; set; }
        public UserDto(User user, MongoIdentityUser mongoIdentityUser, IQueryable<string?> roles, Faculty faculty)
        {
            Id = mongoIdentityUser.Id;
            Email = mongoIdentityUser.Email!;
            Full_name = user.LastName + " " + user.FirstName + " " + user.AdditionalName;
            Faculty = faculty != null ? new FacultyDto(faculty) : null;
            Birth_date = user.BirthDate.Date;
            Phone = mongoIdentityUser.PhoneNumber!;
            Need_register = !mongoIdentityUser.EmailConfirmed;
            Date_joined = mongoIdentityUser.CreatedOn;
            Groups = roles?.AsQueryable();
            Password_set = mongoIdentityUser.PasswordHash != null;
        }
        public UserDto() 
        { 

        }
    }
}
