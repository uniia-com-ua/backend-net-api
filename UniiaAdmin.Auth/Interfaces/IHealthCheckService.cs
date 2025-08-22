namespace UniiaAdmin.Auth.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniiaAdmin.Data.Models;

public interface IHealthCheckService
{
	public Task<bool> CanMongoConnectAsync();

	public Task<bool> CanAdminConnectAsync();
}
