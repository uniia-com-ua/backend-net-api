using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using AspNetCore.Identity.MongoDbCore.Models;
using System.Security.Claims;
using UNIIAadminAPI.Enums;
using UNIIAadminAPI.Models;
using MongoDbGenericRepository;
using MongoDB.Driver;

namespace UNIIAadminAPI.Services
{
    public class RequireClaimAttribute(ClaimsEnum claimValue) : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var claimValueToString = claimValue.ToString();

            var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<MongoIdentityUser>>();

            IMongoDbContext db = context.HttpContext.RequestServices.GetRequiredService<IMongoDbContext>();

            var user = await userManager.GetUserAsync(context.HttpContext.User);

            if (user == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            if(await userManager.IsInRoleAsync(user, "Admin"))
            {
                await next();
            }

            var filter = Builders<MongoIdentityRole>.Filter.In(r => r.Id, user.Roles) &
                     Builders<MongoIdentityRole>.Filter.ElemMatch(r => r.Claims, cl => cl.Value == claimValueToString);

            if (!(db.GetCollection<MongoIdentityRole>().Find(filter).Any() || user.Claims.Any(cl => cl.Value.Equals(claimValueToString))))
            {
                context.Result = new ForbidResult();
                return;
            }

            await next();
        }
    }
}