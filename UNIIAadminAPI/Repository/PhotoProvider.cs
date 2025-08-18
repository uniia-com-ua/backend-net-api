namespace UniiaAdmin.WebApi.Repository;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UniiaAdmin.Data.Common;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.Data.Interfaces.FileInterfaces;
using UniiaAdmin.WebApi.Interfaces;

public class PhotoProvider : IPhotoProvider
{
	private readonly ApplicationContext _applicationContext;
	private readonly MongoDbContext _mongoDbContext;
	private readonly IFileEntityService _fileService;
	private readonly IMapper _mapper;

	public PhotoProvider(
		ApplicationContext applicationContext,
		MongoDbContext mongoDbContext,
		IMapper mapper,
		IFileEntityService fileService)
	{
		_applicationContext = applicationContext;
		_mongoDbContext = mongoDbContext;
		_mapper = mapper;
		_fileService = fileService;
	}

	public async Task<Result<K>> GetPhotoAsync<T, K>(int id)
		where T : class, IPhotoEntity
		where K : class, IMongoFileEntity
	{
		var photoId = await _applicationContext.Set<T>()
			.Where(a => a.Id == id)
			.Select(a => a.PhotoId)
			.FirstOrDefaultAsync();

		return await _fileService.GetFileAsync(photoId, _mongoDbContext.Set<K>());
	}
}
