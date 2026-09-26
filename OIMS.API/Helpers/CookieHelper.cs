using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace OIMS.API.Helpers
{
    public static class CookieHelper
    {
        public static void SetAccessTokenCookie(
            HttpResponse response,
            IConfiguration configuration,
            string accessToken
        )
        {
            var accessTokenExpiryMinutes = double.Parse(
                configuration["JwtSettings:ExpiryMinutes"] ?? "60"
            );

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddMinutes(accessTokenExpiryMinutes),
            };

            response.Cookies.Append("accessToken", accessToken, cookieOptions);
        }

        public static void ClearAccessTokenCookie(HttpResponse response)
        {
            response.Cookies.Delete("accessToken");
        }

        public static string? GetAccessToken(HttpRequest request)
        {
            return request.Cookies["accessToken"];
        }
    }
}
