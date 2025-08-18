namespace UniiaAdmin.WebApi.Repository;

using AutoMapper;
using Microsoft.AspNetCore.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using UniiaAdmin.Data.Common;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.Data.Interfaces.FileInterfaces;
using UniiaAdmin.WebApi.Interfaces;

public class PhotoRepository : IPhotoRepository
{
	private readonly ApplicationContext _applicationContext;
	private readonly MongoDbContext _mongoDbContext;
	private readonly IFileEntityService _fileService;
	private readonly IMapper _mapper;

	public PhotoRepository(
			ApplicationContext applicationContext,
			MongoDbContext mongoDbContext,
			IMapper mapper,
			IFileEntityService fileService)
	{
		_applicationContext = applicationContext;
		_mongoDbContext = mongoDbContext;
		_mapper = mapper;
		_fileService=fileService;
	}
	public async Task<Result<K>> CreateAsync<T, K>(T model, IFormFile? photo)
		where T : class, IPhotoEntity
		where K : class, IMongoFileEntity, new()
	{
		model.Id = 0;

		if (photo != null)
		{
			var result = await _fileService.SaveFileAsync(photo, _mongoDbContext.Set<K>(), MediaTypeNames.Image.Jpeg);

			if (!result.IsSuccess)
			{
				return result;
			}

			model.PhotoId = result.Value!.Id.ToString();
		}

		await _applicationContext.Set<T>().AddAsync(model);

		await _applicationContext.SaveChangesAsync();

		return Result<K>.SuccessNoContent();
	}

	public async Task<Result<K>> UpdateAsync<T, K>(T model, T existedModel, IFormFile? photo)
		where T : class, IPhotoEntity
		where K : class, IMongoFileEntity, new()
	{
		_mapper.Map(model, existedModel);

		if (photo != null)
		{
			var result = await _fileService.UpdateFileAsync(photo, model.PhotoId, _mongoDbContext.Set<K>(), MediaTypeNames.Image.Jpeg);

			if (!result.IsSuccess)
			{
				return result;
			}

			existedModel.PhotoId = result.Value!.Id.ToString();
		}

		await _applicationContext.SaveChangesAsync();

		return Result<K>.SuccessNoContent();
	}

	public async Task DeleteAsync<T, K>(T model)
		where T : class, IPhotoEntity
		where K : class, IMongoFileEntity
	{
		await _fileService.DeleteFileAsync(model.PhotoId, _mongoDbContext.Set<K>());

		_applicationContext.Set<T>().Remove(model);

		await _applicationContext.SaveChangesAsync();
	}
}
