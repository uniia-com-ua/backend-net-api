using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using UniiaAdmin.WebApi.Interfaces;
using UniiaAdmin.WebApi.Resources;


namespace UniiaAdmin.WebApi.Extentions;
public static class OptionsConfigurationExtention
{
	public static IMvcBuilder AddCustomModelStateValidation(this IMvcBuilder services)
	{
		services.ConfigureApiBehaviorOptions(options =>
				{
					options.InvalidModelStateResponseFactory = context =>
					{
						var errors = context.ModelState
							.Where(e => e.Value?.Errors.Count > 0)
							.Select(e => new
							{
								Field = e.Key,
								Errors = e.Value!.Errors.Select(er => er.ErrorMessage).ToArray()
							});

						var localizer = context.HttpContext
							.RequestServices
							.GetRequiredService<IStringLocalizer<ErrorMessages>>();

						return new BadRequestObjectResult(new
						{
							Message = localizer["ModelNotValid"].Value,
							Details = errors
						});
					};
			});

		return services;
	}
}
