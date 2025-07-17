using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Prodora.WebUI.Middlewares
{
    public class FirstVisitRedirectMiddleware
    {
        private readonly RequestDelegate _next;

        public FirstVisitRedirectMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            // Kullanıcı giriş yapmamışsa ve ana sayfaya erişmek istiyorsa login'e yönlendir
            if (!context.User.Identity.IsAuthenticated)
            {
                var path = context.Request.Path.Value.ToLower();
                if (path == "/" || path == "/home" || path == "/home/index")
                {
                    context.Response.Redirect("/Account/Login");
                    return;
                }
            }
            await _next(context);
        }
    }
} 