using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using System.Security.Claims;

namespace CreateInvoiceSystem.API.Middleware
{
    public class SessionActivityMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SessionActivityMiddleware> _logger;

        public SessionActivityMiddleware(RequestDelegate next, ILogger<SessionActivityMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IUserRepository userRepository)
        {
            await _next(context);

            if (context.Response.IsSuccessStatusCode() && context.User?.Identity?.IsAuthenticated == true)
            {
                await UpdateSessionActivityAsync(context, userRepository);
            }
        }

        private async Task UpdateSessionActivityAsync(HttpContext context, IUserRepository userRepository)
        {
            try
            {
                var userIdClaim = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!int.TryParse(userIdClaim, out var userId))
                {
                    _logger.LogWarning("Nie udało się pobrać userId z Claims");
                    return;
                }

                var refreshTokenClaim = context.User?.FindFirst("refresh_token")?.Value;

                if (string.IsNullOrEmpty(refreshTokenClaim))
                {
                    _logger.LogDebug("Refresh token nie znaleziony w claims - pomijanie aktualizacji LastActivityAt");
                    return;
                }

                if (!Guid.TryParse(refreshTokenClaim, out var refreshToken))
                {
                    _logger.LogWarning("Nie udało się sparserować refresh tokenu: {RefreshToken}", refreshTokenClaim);
                    return;
                }

                var session = await userRepository.GetSessionByTokenAsync(refreshToken, default);

                if (session == null)
                {
                    _logger.LogWarning("Sesja nie znaleziona dla userId: {UserId}", userId);
                    return;
                }

                if (DateTime.UtcNow - session.LastActivityAt >= TimeSpan.FromMinutes(1))
                {
                    session.LastActivityAt = DateTime.UtcNow;
                    await userRepository.UpdateSessionActivityAsync(session, default);

                    _logger.LogDebug("Zaktualizowano LastActivityAt dla userId: {UserId}", userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas aktualizacji aktywności sesji");
            }
        }
    }

    public static class SessionActivityMiddlewareExtensions
    {
        public static IApplicationBuilder UseSessionActivityTracking(this IApplicationBuilder app)
        {
            return app.UseMiddleware<SessionActivityMiddleware>();
        }
    }

    public static class HttpResponseExtensions
    {
        public static bool IsSuccessStatusCode(this HttpResponse response)
        {
            return response.StatusCode >= 200 && response.StatusCode < 300;
        }
    }
}
