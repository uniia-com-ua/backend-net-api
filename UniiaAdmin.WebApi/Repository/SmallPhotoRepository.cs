namespace UniiaAdmin.WebApi.Repository;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;
using UniiaAdmin.Data.Common;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Interfaces.FileInterfaces;
using UniiaAdmin.WebApi.Interfaces;
using UniiaAdmin.WebApi.Interfaces.FileInterfaces;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;

public class SmallPhotoRepository : ISmallPhotoRepository
{
	private readonly IApplicationUnitOfWork _applicationUnitOfWork;
	private readonly IFileEntityService _fileService;
	private readonly IMapper _mapper;

	public SmallPhotoRepository(
			IApplicationUnitOfWork applicationUnitOfWork,
			IMapper mapper,
			IFileEntityService fileService)
	{
		_applicationUnitOfWork = applicationUnitOfWork;
		_mapper = mapper;
		_fileService=fileService;
	}
	public async Task<Result<K>> GetSmallPhotoAsync<T, K>(int id)
			where T : class, ISmallPhotoEntity
			where K : class, IMongoFileEntity
	{
		var photoId = await _applicationUnitOfWork.FindSmallPhotoIdAsync<T>(id);

		return await _fileService.GetFileAsync<K>(photoId);
	}

	public async Task<Result<K>> CreateAsync<T, K>(T model, IFormFile? photo, IFormFile? smallPhoto)
			where T : class, ISmallPhotoEntity
			where K : class, IMongoFileEntity, new()
	{
		if (photo != null)
		{
			var result = await _fileService.SaveFileAsync<K>(photo, MediaTypeNames.Image.Jpeg);

			if (!result.IsSuccess)
			{
				return result;
			}

			model.PhotoId = result.Value!.Id.ToString();
		}

		if (smallPhoto != null)
		{
			var result = await _fileService.SaveFileAsync<K>(smallPhoto, MediaTypeNames.Image.Jpeg);

			if (!result.IsSuccess)
			{
				return result;
			}

			model.SmallPhotoId = result.Value!.Id.ToString();
		}

		await _applicationUnitOfWork.AddAsync(model);

		await _applicationUnitOfWork.SaveChangesAsync();

		return Result<K>.SuccessNoContent();
	}

	public async Task<Result<K>> UpdateAsync<T, K>(T model, T existedModel, IFormFile? photo, IFormFile? smallPhoto)
			where T : class, ISmallPhotoEntity
			where K : class, IMongoFileEntity, new()
	{
		_mapper.Map(model, existedModel);

		if (photo != null)
		{
			var result = await _fileService.UpdateFileAsync<K>(photo, model.PhotoId, MediaTypeNames.Image.Jpeg);

			if (!result.IsSuccess)
			{
				return result;
			}

			model.PhotoId = result.Value!.Id.ToString();
		}

		if (smallPhoto != null)
		{
			var result = await _fileService.UpdateFileAsync<K>(smallPhoto, model.SmallPhotoId, MediaTypeNames.Image.Jpeg);

			if (!result.IsSuccess)
			{
				return result;
			}

			model.SmallPhotoId = result.Value!.Id.ToString();
		}

		await _applicationUnitOfWork.SaveChangesAsync();

		return Result<K>.SuccessNoContent();
	}

	public async Task DeleteAsync<T, K>(T model)
			where T : class, ISmallPhotoEntity
			where K : class, IMongoFileEntity
	{
		await _fileService.DeleteFileAsync<K>(model.PhotoId);

		await _fileService.DeleteFileAsync<K>(model.SmallPhotoId);

		_applicationUnitOfWork.Remove(model);

		await _applicationUnitOfWork.SaveChangesAsync();
	}
}
