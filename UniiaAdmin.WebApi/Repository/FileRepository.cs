namespace UniiaAdmin.WebApi.Repository;

using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;
using System.Threading.Tasks;
using UniiaAdmin.Data.Common;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Interfaces.FileInterfaces;
using UniiaAdmin.WebApi.Interfaces;
using UniiaAdmin.WebApi.Interfaces.FileInterfaces;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;

public class FileRepository : IFileRepository
{
	private readonly IApplicationUnitOfWork _applicationUnitOfWork;
	private readonly IFileEntityService _fileService;
	private readonly IMapper _mapper;

	public FileRepository(
			IApplicationUnitOfWork applicationUnitOfWork,
			IMapper mapper,
			IFileEntityService fileService)
	{
		_applicationUnitOfWork = applicationUnitOfWork;
		_mapper = mapper;
		_fileService=fileService;
	}

	public async Task<Result<K>> GetFileAsync<T, K>(int id)
		where T : class, IFileEntity
		where K : class, IMongoFileEntity
	{
		var fileId = await _applicationUnitOfWork.FindFileIdAsync<T>(id);

		var result = await _fileService.GetFileAsync<K>(fileId);

		return result;
	}

	public async Task<Result<K>> CreateAsync<T, K>(T model, IFormFile? file)
		where T : class, IFileEntity
		where K : class, IMongoFileEntity, new()
	{
		if (file != null)
		{
			var result = await _fileService.SaveFileAsync<K>(file, MediaTypeNames.Application.Pdf);

			if (!result.IsSuccess)
			{
				return result;
			}

			model.FileId = result.Value!.Id.ToString();
		}

		await _applicationUnitOfWork.AddAsync(model);

		await _applicationUnitOfWork.SaveChangesAsync();

		return Result<K>.SuccessNoContent();
	}

	public async Task<Result<K>> UpdateAsync<T, K>(T model, T existedModel, IFormFile? file)
		where T : class, IFileEntity
		where K : class, IMongoFileEntity, new()
	{
		_mapper.Map(model, existedModel);

		if (file != null)
		{
			var result = await _fileService.UpdateFileAsync<K>(file, model.FileId, MediaTypeNames.Application.Pdf);

			if (!result.IsSuccess)
			{
				return result;
			}

			model.FileId = result.Value!.Id.ToString();

			return Result<K>.SuccessNoContent();
		}

		await _applicationUnitOfWork.SaveChangesAsync();

		return Result<K>.SuccessNoContent();
	}

	public async Task DeleteAsync<T, K>(T model)
		where T : class, IFileEntity
		where K : class, IMongoFileEntity
	{
		await _fileService.DeleteFileAsync<K>(model.FileId);

		_applicationUnitOfWork.Remove(model);

		await _applicationUnitOfWork.SaveChangesAsync();
	}
}
