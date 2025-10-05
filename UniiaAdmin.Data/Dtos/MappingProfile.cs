using AutoMapper;
using UniiaAdmin.Data.Dtos.UserDtos;
using UniiaAdmin.Data.Models;

namespace UniiaAdmin.Data.Dtos
{
	public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			CreateMap<AdminUser, AdminUserDto>();
			CreateMap<User, UserDto>();

			CreateMap<Subject, Subject>()
				.ForMember(dest => dest.Id, opt => opt.Ignore());

			CreateMap<PublicationType, PublicationType>()
				.ForMember(dest => dest.Id, opt => opt.Ignore());

			CreateMap<PublicationLanguage, PublicationLanguage>()
				.ForMember(dest => dest.Id, opt => opt.Ignore());

			CreateMap<Keyword, Keyword>()
				.ForMember(dest => dest.Id, opt => opt.Ignore());

			CreateMap<Specialty, Specialty>()
				.ForMember(dest => dest.Id, opt => opt.Ignore());

			CreateMap<Specialty, SpecialityDto>()
				.ForMember(dest => dest.SubjectCount,
					opt => opt.MapFrom(src => src.Subjects != null ? src.Subjects.Count : 0));

			CreateMap<UserDto, User>();

			CreateMap<Author, Author>()
					   .ForMember(dest => dest.Id, opt => opt.Ignore())
					   .ForMember(dest => dest.PhotoId, opt => opt.Ignore());

			CreateMap<Faculty, Faculty>()
					   .ForMember(dest => dest.Id, opt => opt.Ignore());

			CreateMap<University, University>()
					   .ForMember(dest => dest.Id, opt => opt.Ignore())
					   .ForMember(dest => dest.PhotoId, opt => opt.Ignore())
					   .ForMember(dest => dest.SmallPhotoId, opt => opt.Ignore());

			CreateMap<Publication, Publication>()
				.ForMember(dest => dest.Id, opt => opt.Ignore())
				.ForMember(dest => dest.FileId, opt => opt.Ignore());
		}
	}
}
