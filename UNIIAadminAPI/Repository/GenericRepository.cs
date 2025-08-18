namespace UniiaAdmin.WebApi.Repository;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;
using UniiaAdmin.Data.Common;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.Data.Interfaces.FileInterfaces;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Interfaces;

public class GenericRepository : IGenericRepository
{
	private readonly ApplicationContext _applicationContext;
	private readonly IMapper _mapper;

	public GenericRepository(
			ApplicationContext applicationContext,
			IMapper mapper)
	{
		_applicationContext = applicationContext;
		_mapper = mapper;
	}

	public async Task CreateAsync<T>(T model) where T : class, IEntity
	{
		model.Id = 0;

		await _applicationContext.Set<T>().AddAsync(model);

		await _applicationContext.SaveChangesAsync();
	}

	public async Task UpdateAsync<T>(T model, T existedModel) where T : class, IEntity
	{
		_mapper.Map(model, existedModel);

		await _applicationContext.SaveChangesAsync();
	}

	public async Task DeleteAsync<T>(T model) where T : class
	{
		_applicationContext.Set<T>().Remove(model);

		await _applicationContext.SaveChangesAsync();
	}
}
