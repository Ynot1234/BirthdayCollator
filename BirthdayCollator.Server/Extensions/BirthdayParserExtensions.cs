namespace BirthdayCollator.Server.Extensions;

public static class BirthdayParserExtensions
{
    public static IServiceCollection AddBirthdayParsers(this IServiceCollection services)
    {
        services.AddScoped<IWikiParser, WikiParser>();
        services.AddScoped<IDatePageParser, DatePageParser>();
        services.AddScoped<GenariansPageParser>();
        services.AddScoped<GenarianFetcher>();
        return services;
    }
}