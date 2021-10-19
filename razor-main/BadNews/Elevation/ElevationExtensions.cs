using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BadNews.Elevation
{
    public static class ElevationExtensions
    {
        public static bool IsElevated(this HttpRequest request)
        {
            bool isElevated = request.Cookies.TryGetValue(ElevationConstants.CookieName, out var value)
                && value == ElevationConstants.CookieValue;
            return isElevated;
        }

        public static bool IsElevated(this ViewContext viewContext)
        {
            return viewContext.HttpContext.Request.IsElevated();
        }
    }
}
