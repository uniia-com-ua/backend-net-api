namespace UniiaAdmin.WebApi.Repository;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;
using UniiaAdmin.Data.Common;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.Data.Interfaces.FileInterfaces;
using UniiaAdmin.WebApi.Interfaces;

public class SmallPhotoRepository : ISmallPhotoRepository
{
	private readonly ApplicationContext _applicationContext;
	private readonly MongoDbContext _mongoDbContext;
	private readonly IFileEntityService _fileService;
	private readonly IMapper _mapper;

	public SmallPhotoRepository(
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
	public async Task<Result<K>> GetSmallPhotoAsync<T, K>(int id)
			where T : class, ISmallPhotoEntity
			where K : class, IMongoFileEntity
	{
		var photoId = await _applicationContext.Set<T>()
			.Where(a => a.Id == id)
			.Select(a => a.SmallPhotoId)
			.FirstOrDefaultAsync();

		return await _fileService.GetFileAsync(photoId, _mongoDbContext.Set<K>());
	}

	public async Task<Result<K>> CreateAsync<T, K>(T model, IFormFile? photo, IFormFile? smallPhoto)
			where T : class, ISmallPhotoEntity
			where K : class, IMongoFileEntity, new()
	{
		if (photo != null)
		{
			var result = await _fileService.SaveFileAsync(photo, _mongoDbContext.Set<K>(), MediaTypeNames.Image.Jpeg);

			if (!result.IsSuccess)
			{
				return result;
			}

			model.PhotoId = result.Value!.Id.ToString();
		}

		if (smallPhoto != null)
		{
			var result = await _fileService.SaveFileAsync(smallPhoto, _mongoDbContext.Set<K>(), MediaTypeNames.Image.Jpeg);

			if (!result.IsSuccess)
			{
				return result;
			}

			model.SmallPhotoId = result.Value!.Id.ToString();
		}

		await _applicationContext.Set<T>().AddAsync(model);

		await _applicationContext.SaveChangesAsync();

		return Result<K>.SuccessNoContent();
	}

	public async Task<Result<K>> UpdateAsync<T, K>(T model, T existedModel, IFormFile? photo, IFormFile? smallPhoto)
			where T : class, ISmallPhotoEntity
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

			model.PhotoId = result.Value!.Id.ToString();
		}

		if (smallPhoto != null)
		{
			var result = await _fileService.UpdateFileAsync(smallPhoto, model.SmallPhotoId, _mongoDbContext.Set<K>(), MediaTypeNames.Image.Jpeg);

			if (!result.IsSuccess)
			{
				return result;
			}

			model.SmallPhotoId = result.Value!.Id.ToString();
		}

		await _applicationContext.SaveChangesAsync();

		return Result<K>.SuccessNoContent();
	}

	public async Task DeleteAsync<T, K>(T model)
			where T : class, ISmallPhotoEntity
			where K : class, IMongoFileEntity
	{
		await _fileService.DeleteFileAsync(model.PhotoId, _mongoDbContext.Set<K>());

		await _fileService.DeleteFileAsync(model.SmallPhotoId, _mongoDbContext.Set<K>());

		_applicationContext.Set<T>().Remove(model);

		await _applicationContext.SaveChangesAsync();
	}
}
