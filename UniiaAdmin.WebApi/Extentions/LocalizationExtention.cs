using System.Globalization;

namespace UniiaAdmin.WebApi.Extentions;

public static class LocalizationExtention
{
	public static void AddLocalization(this WebApplication app)
	{
		var supportedCultures = new[] { CultureInfo.InvariantCulture };

		app.UseRequestLocalization(new RequestLocalizationOptions
		{
			DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(CultureInfo.InvariantCulture),
			SupportedCultures = supportedCultures,
			SupportedUICultures = supportedCultures
		});
	}
}

