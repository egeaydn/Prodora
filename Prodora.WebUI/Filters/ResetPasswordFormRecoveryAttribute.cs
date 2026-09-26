using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Prodora.WebUI.Models;

namespace Prodora.WebUI.Filters;

/// <summary>Reject a stale POST, but let the user retry with a fresh form token.</summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class ResetPasswordFormRecoveryAttribute : Attribute, IAsyncAlwaysRunResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is IAntiforgeryValidationFailedResult
            && context.HttpContext.Request.HasFormContentType)
        {
            IFormCollection form;
            try
            {
                form = await context.HttpContext.Request.ReadFormAsync(context.HttpContext.RequestAborted);
            }
            catch (Exception error) when (error is InvalidDataException or IOException or BadHttpRequestException)
            {
                // A malformed request must remain rejected, not become a rendering error.
                await next();
                return;
            }
            // Retain only the reset link and email, never the submitted password.
            var model = new ResetPasswordModel
            {
                Token = form["Token"].ToString(),
                Email = form["Email"].ToString()
            };
            context.ModelState.Clear();
            context.HttpContext.Response.Headers.CacheControl = "no-store";
            context.ModelState.AddModelError("", "Formun güvenlik bilgileri geçersiz veya oturumun değişmiş. Form yenilendi; şifreni tekrar girip kaydet. Sorun sürerse e-postadaki bağlantıyı yeniden aç ve tarayıcının çerezlere izin verdiğinden emin ol.");
            var metadata = context.HttpContext.RequestServices.GetRequiredService<IModelMetadataProvider>();
            context.Result = new ViewResult
            {
                ViewName = "ResetPassword",
                StatusCode = StatusCodes.Status400BadRequest,
                ViewData = new ViewDataDictionary<ResetPasswordModel>(metadata, context.ModelState) { Model = model }
            };
        }
        await next();
    }
}
