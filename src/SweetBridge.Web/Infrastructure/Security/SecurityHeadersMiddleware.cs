namespace SweetBridge.Web.Infrastructure.Security;

/// <summary>
/// Заголовки безопасности, добавляемые к каждому ответу.
/// CSP жёсткий: inline-скрипты и inline-стили запрещены, поэтому вся разметка
/// и стили вынесены во внешние файлы.
/// </summary>
public static class SecurityHeadersMiddleware
{
    private const string ContentSecurityPolicy =
        "default-src 'self'; "
        + "script-src 'self'; "
        + "style-src 'self'; "
        + "img-src 'self' data:; "
        + "font-src 'self'; "
        + "connect-src 'self'; "
        + "object-src 'none'; "
        + "base-uri 'self'; "
        + "form-action 'self'; "
        + "frame-ancestors 'none'";

    private const string PermissionsPolicy =
        "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()";

    /// <summary>
    /// Добавляет заголовки безопасности к каждому ответу.
    /// </summary>
    /// <param name="app">Конвейер приложения.</param>
    /// <returns>Тот же конвейер.</returns>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.Use(async (context, next) =>
        {
            context.Response.OnStarting(() =>
            {
                Apply(context.Response.Headers);

                return Task.CompletedTask;
            });

            await next();
        });
    }

    private static void Apply(IHeaderDictionary headers)
    {
        headers["Content-Security-Policy"] = ContentSecurityPolicy;
        headers["X-Content-Type-Options"] = "nosniff";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        headers["Permissions-Policy"] = PermissionsPolicy;
        headers["X-Frame-Options"] = "DENY";
        headers["Cross-Origin-Opener-Policy"] = "same-origin";
    }
}