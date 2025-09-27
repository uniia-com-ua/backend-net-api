namespace UniiaAdmin.WebApi.Repository.UnitOfWork;

using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Interfaces.FileInterfaces;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;

public class MongoUnitOfWork : IMongoUnitOfWork
{
	private readonly MongoDbContext _mongoDbContext;

	public MongoUnitOfWork(MongoDbContext mongoDbContext)
	{
		_mongoDbContext = mongoDbContext;
	}

	public async Task AddAsync<T>(T model) where T : class => await _mongoDbContext.Set<T>().AddAsync(model);

	public async Task<T?> FindFileAsync<T>(ObjectId id) where T : class, IMongoFileEntity => await _mongoDbContext.Set<T>().FindAsync(id);

	public async Task SaveChangesAsync() => await _mongoDbContext.SaveChangesAsync();

	public IQueryable<T> Query<T>() where T : class => _mongoDbContext.Set<T>();

	public IQueryable<T> Query<T>(Expression<Func<T, bool>> predicate) where T : class => _mongoDbContext.Set<T>().Where(predicate);

	public Task<bool> AnyAsync<T>(ObjectId id) where T : class, IMongoFileEntity => _mongoDbContext.Set<T>().AnyAsync(e => e.Id == id);

	public void Attach<T>(T model) where T : class => _mongoDbContext.Set<T>().Attach(model);

	public void Remove<T>(T model) where T : class => _mongoDbContext.Set<T>().Remove(model);

	public void Update<T>(T model) where T : class => _mongoDbContext.Set<T>().Update(model);

	public async Task<bool> CanConnectAsync() => await _mongoDbContext.Database.CanConnectAsync();

	public async Task CreateDatabaseAsync() => await _mongoDbContext.Database.EnsureCreatedAsync();
}
