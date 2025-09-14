namespace UniiaAdmin.Auth.Interfaces;

using Microsoft.AspNetCore.Identity;
using UniiaAdmin.Data.Models;

public interface IAdminUnitOfWork
{
	public Task<bool> CreateAsync();

	public Task<bool> CanConnectAsync();
}
