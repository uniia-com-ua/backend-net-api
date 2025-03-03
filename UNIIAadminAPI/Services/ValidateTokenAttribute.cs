using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using UniiaAdmin.Data.Models;
using UniiaAdmin.Data.Interfaces;

namespace UNIIAadminAPI.Services
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ValidateTokenAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var tokenService = context.HttpContext.RequestServices.GetRequiredService<ITokenService>();

            var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<AdminUser>>();

            var user = await userManager.GetUserAsync(context.HttpContext.User);

            if (user == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var result = await tokenService.ValidateGoogleTokenAsync(user, userManager);

            if (!result)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            context.HttpContext.Items["User"] = user;

            await next();
        }
    }
}
