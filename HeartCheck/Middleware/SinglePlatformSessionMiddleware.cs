using System.Security.Claims;
using HeartCheck.Data;
using HeartCheck.Models;
using HeartCheck.Services;
using MongoDB.Bson;

namespace HeartCheck.Middleware
{
    public class SinglePlatformSessionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SinglePlatformSessionMiddleware> _logger;

        public SinglePlatformSessionMiddleware(
            RequestDelegate next,
            ILogger<SinglePlatformSessionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IUserRepository userRepository)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var platform = context.User.FindFirstValue(JwtService.PlatformClaim) ?? "Web";
                var sessionId = context.User.FindFirstValue(JwtService.SessionIdClaim);

                if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(sessionId))
                {
                    var user = await userRepository.GetByIdAsync(userId);

                    var activeSession = FindActiveSession(user, platform);

                    if (activeSession != null &&
                        !string.Equals(activeSession, sessionId, StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogWarning(
                            "Sesión rechazada para el usuario {UserId} en la plataforma {Platform}: " +
                            "el token recibido no coincide con la sesión activa.",
                            userId, platform);

                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsJsonAsync(new
                        {
                            message = "Sesión caducada: se ha iniciado sesión en otro dispositivo de esta misma categoría."
                        });
                        return;
                    }
                }
            }

            await _next(context);
        }

        private static string? FindActiveSession(User? user, string platform)
        {
            if (user?.ActiveSessions == null || user.ActiveSessions.Count == 0)
            {
                return null;
            }

            var exact = user.ActiveSessions.FirstOrDefault(kv =>
                string.Equals(kv.Key, platform, StringComparison.OrdinalIgnoreCase));

            return exact.Key is null ? null : exact.Value;
        }
    }
}
