namespace UniiaAdmin.WebApi.Repository;

using AutoMapper;
using Microsoft.AspNetCore.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using UniiaAdmin.Data.Common;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Interfaces.FileInterfaces;
using UniiaAdmin.WebApi.Interfaces;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;

public class PhotoRepository : IPhotoRepository
{
	private readonly IApplicationUnitOfWork _applicationUnitOfWork;
	private readonly IFileEntityService _fileService;
	private readonly IMapper _mapper;

	public PhotoRepository(
			IApplicationUnitOfWork applicationUnitOfWork,
			IMapper mapper,
			IFileEntityService fileService)
	{
		_applicationUnitOfWork = applicationUnitOfWork;
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
			var result = await _fileService.SaveFileAsync<K>(photo, MediaTypeNames.Image.Jpeg);

			if (!result.IsSuccess)
			{
				return result;
			}

			model.PhotoId = result.Value!.Id.ToString();
		}

		await _applicationUnitOfWork.AddAsync(model);

		await _applicationUnitOfWork.SaveChangesAsync();

		return Result<K>.SuccessNoContent();
	}

	public async Task<Result<K>> UpdateAsync<T, K>(T model, T existedModel, IFormFile? photo)
		where T : class, IPhotoEntity
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

			existedModel.PhotoId = result.Value!.Id.ToString();
		}

		await _applicationUnitOfWork.SaveChangesAsync();

		return Result<K>.SuccessNoContent();
	}

	public async Task DeleteAsync<T, K>(T model)
		where T : class, IPhotoEntity
		where K : class, IMongoFileEntity
	{
		await _fileService.DeleteFileAsync<K>(model.PhotoId);

		_applicationUnitOfWork.Remove(model);

		await _applicationUnitOfWork.SaveChangesAsync();
	}
}
