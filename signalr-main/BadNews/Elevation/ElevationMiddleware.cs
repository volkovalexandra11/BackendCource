using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace BadNews.Elevation
{
    public class ElevationMiddleware
    {
        private readonly RequestDelegate next;
    
        public ElevationMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path == "/elevation")
            {
                bool isUp = context.Request.Query.ContainsKey("up");
                Elevate(context.Response, isUp);
                context.Response.Redirect("/");
            }
            else
            {
                await next(context);
            }
        }

        private void Elevate(HttpResponse response, bool up)
        {
            if (up)
            {
                response.Cookies.Append(ElevationConstants.CookieName, ElevationConstants.CookieValue,
                    new CookieOptions
                    {
                        HttpOnly = true
                    });
            }
            else
            {
                response.Cookies.Delete(ElevationConstants.CookieName);
            }
        }
    }
}
