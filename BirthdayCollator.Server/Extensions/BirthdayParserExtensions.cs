using BirthdayCollator.Server.Processing.Parsers;
using BirthdayCollator.Server.Resources;

namespace BirthdayCollator.Server.Extensions;

public static class BirthdayParserExtensions
{
    public static IServiceCollection AddBirthdayParsers(this IServiceCollection services)
    {
        services.AddScoped<IWikiParser, WikiParser>();
        services.AddScoped<IDatePageParser, DatePageParser>();
        services.AddScoped<GenariansPageParser>();
        services.AddScoped<GenarianPageLoader>();
        return services;
    }
}