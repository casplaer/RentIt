namespace RentIt.Users.API.Extensions
{
    public static class HttpResponseExtentions
    {
        public static void SetAuthCookies(this HttpResponse response, string accessToken, string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None
            };

            response.Cookies.Append("AccessToken", accessToken, cookieOptions);
            response.Cookies.Append("RefreshToken", refreshToken, cookieOptions);
        }
    }
}
