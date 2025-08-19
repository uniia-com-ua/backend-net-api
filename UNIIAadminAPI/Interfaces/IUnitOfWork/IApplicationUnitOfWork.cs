namespace UniiaAdmin.WebApi.Interfaces.IUnitOfWork;

using System.Linq.Expressions;
using UniiaAdmin.Data.Interfaces.FileInterfaces;

public interface IApplicationUnitOfWork
{
	public Task<T?> FindAsync<T>(int id) where T : class, IEntity;

	public IQueryable<T> Query<T>() where T : class;

	public Task AddAsync<T>(T model) where T : class;

	public Task<bool> AnyAsync<T>(int id) where T : class, IEntity;

	public void Attach<T>(T model) where T : class;

	public void Remove<T>(T model) where T : class;

	public void Update<T>(T model) where T : class;

	public Task CreateAsync();

	public Task<bool> CanConnectAsync();

	public Task SaveChangesAsync();
}
