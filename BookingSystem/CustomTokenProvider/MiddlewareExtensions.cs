using Microsoft.AspNetCore.Builder;

namespace CustomTokenAuthProvider
{
    #region snippet1
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseTokenProviderMiddleware(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TokenProviderMiddleware>();
        }
    }
    #endregion
}
