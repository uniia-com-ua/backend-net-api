using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniiaAdmin.Data.Models;

namespace UniiaAdmin.Data.Interfaces
{
    public interface ILogActionService
    {
        public Task LogActionAsync<T>(AdminUser? adminUser, int modelId, string action) where T : class;
    }
}
