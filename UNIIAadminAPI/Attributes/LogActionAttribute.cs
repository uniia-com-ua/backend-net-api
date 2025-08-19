using Microsoft.AspNetCore.Mvc.Filters;
using MongoDB.Bson;
using System.Security.Claims;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;

namespace UniiaAdmin.WebApi.Attributes
{
	[AttributeUsage(AttributeTargets.Method)]
	public class LogActionAttribute : Attribute, IAsyncActionFilter
	{
		private readonly string _modelName;
		private readonly string _modelAction;

		public LogActionAttribute(string modelName, string modelAction)
		{
			_modelName = modelName.ToLower();
			_modelAction = modelAction.ToLower();
		}

		public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			await next();

			var dbContext = context.HttpContext.RequestServices.GetService<IMongoUnitOfWork>();

			var httpContext = context.HttpContext;

			var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);

			if (userIdClaim == null || dbContext == null)
				return;

			if (!(context.ActionArguments.TryGetValue("id", out var modelIdObj) ||
				  httpContext.Items.TryGetValue("id", out modelIdObj)))
			{
				return;
			}

			if (modelIdObj is not int modelid)
			{
				return;
			}

			await dbContext.AddAsync<LogActionModel>(new()
			{
				Id = ObjectId.GenerateNewId(),
				UserId = userIdClaim.Value,
				ModelId = modelid,
				ModelAction = _modelAction,
				ModelName = _modelName,
				ChangedTime = DateTime.UtcNow,
			});

			await dbContext.SaveChangesAsync();
		}
	}
}
