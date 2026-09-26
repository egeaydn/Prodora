using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Net.Http.Headers;
using Prodora.WebUI.Models;

namespace Prodora.WebUI.Filters;

/// <summary>
/// Recovers reset-password posts that fail antiforgery validation by preserving safe fields,
/// re-rendering the form with HTTP 400, and issuing a fresh token for retry.
/// </summary>
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
            var typedHeaders = context.HttpContext.Response.GetTypedHeaders();
            var cacheControl = typedHeaders.CacheControl;
            if (cacheControl is null)
            {
                cacheControl = new CacheControlHeaderValue();
            }
            cacheControl.NoStore = true;
            typedHeaders.CacheControl = cacheControl;
            context.ModelState.AddModelError("", "Formun güvenlik bilgileri geçersiz veya oturumun değişmiş. Form yenilendi; şifreni tekrar girip kaydet. Sorun sürerse e-postadaki bağlantıyı yeniden aç.");
            var metadata = context.HttpContext.RequestServices.GetRequiredService<IModelMetadataProvider>();
            context.Result = new ViewResult
            {
                ViewName = "ResetPassword",
                StatusCode = StatusCodes.Status400BadRequest,
                ViewData = new ViewDataDictionary<ResetPasswordModel>(metadata, context.ModelState) { Model = model }
            };
            return;
        }
        await next();
    }
}
