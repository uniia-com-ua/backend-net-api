namespace UniiaAdmin.WebApi.Repository;

using AutoMapper;
using UniiaAdmin.Data.Interfaces.FileInterfaces;
using UniiaAdmin.WebApi.Interfaces;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;

public class GenericRepository : IGenericRepository
{
	private readonly IApplicationUnitOfWork _applicationUnitOfWork;
	private readonly IMapper _mapper;

	public GenericRepository(
			IApplicationUnitOfWork applicationUnitOfWork,
			IMapper mapper)
	{
		_applicationUnitOfWork = applicationUnitOfWork;
		_mapper = mapper;
	}

	public async Task CreateAsync<T>(T model) where T : class, IEntity
	{
		model.Id = 0;

		await _applicationUnitOfWork.AddAsync(model);

		await _applicationUnitOfWork.SaveChangesAsync();
	}

	public async Task UpdateAsync<T>(T model, T existedModel) where T : class, IEntity
	{
		_mapper.Map(model, existedModel);

		await _applicationUnitOfWork.SaveChangesAsync();
	}

	public async Task DeleteAsync<T>(T model) where T : class
	{
		_applicationUnitOfWork.Remove(model);

		await _applicationUnitOfWork.SaveChangesAsync();
	}
}
