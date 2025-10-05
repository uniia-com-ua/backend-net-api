namespace UniiaAdmin.Auth.Repos;

using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using UniiaAdmin.Auth.Interfaces;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Dtos;
using UniiaAdmin.Data.Interfaces.FileInterfaces;
using UniiaAdmin.Data.Models;

public class MongoUnitOfWork : IMongoUnitOfWork
{
	private readonly MongoDbContext _mongoDbContext;

	public MongoUnitOfWork(MongoDbContext mongoDbContext)
	{
		_mongoDbContext = mongoDbContext;
	}

	public async Task<T?> FindFileAsync<T>(ObjectId id) where T : class, IMongoFileEntity => await _mongoDbContext.Set<T>().FindAsync(id);

	public async Task SaveChangesAsync() => await _mongoDbContext.SaveChangesAsync();

	public async Task<bool> CanConnectAsync() => await _mongoDbContext.Database.CanConnectAsync();

	public async Task AddAsync<T>(T model) where T : class => await _mongoDbContext.Set<T>().AddAsync(model);

	public async Task CreateAsync() => await _mongoDbContext.Database.EnsureCreatedAsync();

	public async Task<PageData<AdminLogInHistory>?> GetLogInHistory(string? userId, int skip, int take)
		=> new PageData<AdminLogInHistory> { 
			Items = await _mongoDbContext.AdminLogInHistories
			.Where(li => li.UserId == userId)
			.OrderByDescending(li => li.LogInTime)
			.Skip(skip)
			.Take(take)
			.ToListAsync(),
			TotalCount = await _mongoDbContext.AdminLogInHistories.Where(li => li.UserId == userId).CountAsync()
		};
}
