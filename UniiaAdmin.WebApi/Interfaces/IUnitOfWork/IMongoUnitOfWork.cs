namespace UniiaAdmin.WebApi.Interfaces.IUnitOfWork;

using MongoDB.Bson;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using UniiaAdmin.Data.Interfaces.FileInterfaces;

public interface IMongoUnitOfWork
{
	public Task<T?> FindFileAsync<T>(ObjectId id) where T : class, IMongoFileEntity;

	public IQueryable<T> Query<T>() where T : class;

	public IQueryable<T> Query<T>(Expression<Func<T, bool>> predicate) where T : class; 

	public Task AddAsync<T>(T model) where T : class;

	public Task<bool> AnyAsync<T>(ObjectId id) where T : class, IMongoFileEntity;

	public void Attach<T>(T model) where T : class;

	public void Remove<T>(T model) where T : class;

	public void Update<T>(T model) where T : class;

	public Task CreateDatabaseAsync();

	public Task<bool> CanConnectAsync();

	public Task SaveChangesAsync();
}
