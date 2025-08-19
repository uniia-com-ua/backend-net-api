namespace UniiaAdmin.WebApi.Repository;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UniiaAdmin.Data.Common;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Interfaces.FileInterfaces;
using UniiaAdmin.WebApi.Interfaces;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;

public class PhotoProvider : IPhotoProvider
{
	private readonly IApplicationUnitOfWork _applicationUnitOfWork;
	private readonly IFileEntityService _fileService;

	public PhotoProvider(
		IApplicationUnitOfWork applicationUnitOfWork,
		IFileEntityService fileService)
	{
		_applicationUnitOfWork = applicationUnitOfWork;
		_fileService = fileService;
	}

	public async Task<Result<K>> GetPhotoAsync<T, K>(int id)
		where T : class, IPhotoEntity
		where K : class, IMongoFileEntity
	{
		var photoId = await _applicationUnitOfWork.FindPhotoIdAsync<T>(id);

		if (photoId == null)
			return Result<K>.Failure(new KeyNotFoundException());

		return await _fileService.GetFileAsync<K>(photoId);
	}
}
