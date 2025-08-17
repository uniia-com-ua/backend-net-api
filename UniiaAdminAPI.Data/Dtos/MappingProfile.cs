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
