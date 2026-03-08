using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace BirthdayCollator.ServiceDefaults;
public static class Extensions
{
    public static TBuilder AddBasicDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.ConfigureHttpClientDefaults(http => http.AddStandardResilienceHandler());
        builder.Services.AddHealthChecks().AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);
        return builder;
    }

    public static WebApplication MapBasicEndpoints(this WebApplication app)
    {
        app.MapHealthChecks("/health");
        return app;
    }
}
