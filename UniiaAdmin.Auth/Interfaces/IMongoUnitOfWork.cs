namespace UniiaAdmin.Auth.Interfaces;

using MongoDB.Bson;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using UniiaAdmin.Data.Interfaces.FileInterfaces;
using UniiaAdmin.Data.Models;

public interface IMongoUnitOfWork
{
	public Task<T?> FindFileAsync<T>(ObjectId id) where T : class, IMongoFileEntity;

	public Task<List<AdminLogInHistory>?> GetLogInHistory(string? userId, int skip, int take);

	public Task AddAsync<T>(T model) where T : class;

	public Task CreateAsync();

	public Task<bool> CanConnectAsync();

	public Task SaveChangesAsync();
}
