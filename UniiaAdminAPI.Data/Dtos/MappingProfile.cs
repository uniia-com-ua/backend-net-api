using UniiaAdmin.Data.Models;
using AutoMapper;

namespace UniiaAdmin.Data.Dtos
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<AdminUser, AdminUserDto>();
			CreateMap<User, UserDto>();

			CreateMap<Author, AuthorDto>();
            CreateMap<AuthorDto, Author>();

            CreateMap<Faculty, FacultyDto>();
            CreateMap<FacultyDto, Faculty>();

            CreateMap<University, UniversityDto>();
            CreateMap<UniversityDto, University>();

			CreateMap<Publication, PublicationDto>();
			CreateMap<PublicationDto, Publication>();

			CreateMap<LogActionModel, LogActionModelDto>();
        }
    }
}
