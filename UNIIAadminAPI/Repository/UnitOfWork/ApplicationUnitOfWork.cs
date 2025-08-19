namespace UniiaAdmin.WebApi.Repository.UnitOfWork;

using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using System.Linq.Expressions;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.Data.Interfaces.FileInterfaces;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;

public class ApplicationUnitOfWork : IApplicationUnitOfWork
{
	private readonly ApplicationContext _applicationContext;

	public ApplicationUnitOfWork(ApplicationContext applicationContext)
	{
		_applicationContext = applicationContext;
	}

	public async Task AddAsync<T>(T model) where T : class => await _applicationContext.Set<T>().AddAsync(model);

	public async Task<T?> FindAsync<T>(int id) where T : class, IEntity => await _applicationContext.Set<T>().FindAsync(id);

	public async Task SaveChangesAsync() => await _applicationContext.SaveChangesAsync();

	public IQueryable<T> Query<T>() where T : class => _applicationContext.Set<T>();

	public async Task<bool> AnyAsync<T>(int id) where T : class, IEntity => await _applicationContext.Set<T>().AnyAsync(e => e.Id == id);

	public void Attach<T>(T model) where T : class => _applicationContext.Set<T>().Attach(model);

	public void Remove<T>(T model) where T : class => _applicationContext.Set<T>().Remove(model);

	public void Update<T>(T model) where T : class => _applicationContext.Set<T>().Update(model);

	public async Task<bool> CanConnectAsync() => await _applicationContext.Database.CanConnectAsync();

	public async Task CreateAsync() => await _applicationContext.Database.EnsureCreatedAsync();
}
