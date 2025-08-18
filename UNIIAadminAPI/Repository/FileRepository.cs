namespace UniiaAdmin.WebApi.Repository;

using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;
using System.Threading.Tasks;
using UniiaAdmin.Data.Common;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.Data.Interfaces.FileInterfaces;
using UniiaAdmin.WebApi.Interfaces;

public class FileRepository : IFileRepository
{
	private readonly ApplicationContext _applicationContext;
	private readonly MongoDbContext _mongoDbContext;
	private readonly IFileEntityService _fileService;
	private readonly IMapper _mapper;

	public FileRepository(
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

	public async Task<Result<K>> GetFileAsync<T, K>(int id)
		where T : class, IFileEntity
		where K : class, IMongoFileEntity
	{
		var fileId = await _applicationContext.Set<T>().Where(a => a.Id == id)
												  .Select(a => a.FileId)
												  .FirstOrDefaultAsync();

		var result = await _fileService.GetFileAsync(fileId, _mongoDbContext.Set<K>());

		return result;
	}

	public async Task<Result<K>> CreateAsync<T, K>(T model, IFormFile? file)
		where T : class, IFileEntity
		where K : class, IMongoFileEntity, new()
	{
		if (file != null)
		{
			var result = await _fileService.SaveFileAsync(file, _mongoDbContext.Set<K>(), MediaTypeNames.Application.Pdf);

			if (!result.IsSuccess)
			{
				return result;
			}

			model.FileId = result.Value!.Id.ToString();
		}

		await _applicationContext.Set<T>().AddAsync(model);

		await _applicationContext.SaveChangesAsync();

		return Result<K>.SuccessNoContent();
	}

	public async Task<Result<K>> UpdateAsync<T, K>(T model, T existedModel, IFormFile? file)
		where T : class, IFileEntity
		where K : class, IMongoFileEntity, new()
	{
		_mapper.Map(model, existedModel);

		if (file != null)
		{
			var result = await _fileService.UpdateFileAsync(file, model.FileId, _mongoDbContext.Set<K>(), MediaTypeNames.Application.Pdf);

			if (!result.IsSuccess)
			{
				return result;
			}

			model.FileId = result.Value!.Id.ToString();

			return Result<K>.SuccessNoContent();
		}

		await _applicationContext.SaveChangesAsync();

		return Result<K>.SuccessNoContent();
	}

	public async Task DeleteAsync<T, K>(T model)
		where T : class, IFileEntity
		where K : class, IMongoFileEntity
	{
		await _fileService.DeleteFileAsync(model.FileId, _mongoDbContext.Set<K>());

		_applicationContext.Set<T>().Remove(model);

		await _applicationContext.SaveChangesAsync();
	}
}
