using Microsoft.Extensions.FileProviders;

namespace Prodora.WebUI.Middlewares
{
	public static class AplicationBuilderExtension
	{
		public static IApplicationBuilder CustomStaticFiles(this IApplicationBuilder app)
		{
			var path = Path.Combine(Directory.GetCurrentDirectory(), "node_modules");

			var options = new StaticFileOptions

			{
				FileProvider = new PhysicalFileProvider(path),
				RequestPath = "/node_modules",
			};

			app.UseStaticFiles(options);
			return app;
		} 
	}
}
