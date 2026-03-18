using BirthdayCollator.Server.Processing.Enrichment;
using BirthdayCollator.Server.Processing.Sources;
using BirthdayCollator.Server.Processing.Parsers;  

namespace BirthdayCollator.Server.Extensions;

public static class BirthdaySourceExtensions
{
    public static IServiceCollection AddBirthdaySources(this IServiceCollection services)
    {
        // Internal Helpers
        services.AddScoped<ImdbFetcher>();
        services.AddScoped<ImdbParser>();

        // Concrete Sources
        services.AddScoped<YearBirthSource>();
        services.AddScoped<DateBirthSource>();
        services.AddScoped<CategoryBirthSource>();
        services.AddScoped<GenariansBirthSource>();
        services.AddScoped<OnThisDaySource>();
        services.AddScoped<ImdbSource>();

        services.AddScoped<IBirthSource>(sp =>
            new LeapYearSourceDecorator(sp.GetRequiredService<YearBirthSource>()));

        services.AddScoped<IBirthSource>(sp =>
            new LeapYearSourceDecorator(sp.GetRequiredService<DateBirthSource>()));

        services.AddScoped<IBirthSource>(sp =>
            new LeapYearSourceDecorator(sp.GetRequiredService<CategoryBirthSource>()));

        services.AddScoped<IBirthSource>(sp =>
            new LeapYearSourceDecorator(sp.GetRequiredService<GenariansBirthSource>()));

        services.AddScoped<IBirthSource>(sp =>
            new LeapYearSourceDecorator(sp.GetRequiredService<OnThisDaySource>()));

        services.AddScoped<IBirthSource>(sp =>
            new LeapYearSourceDecorator(sp.GetRequiredService<ImdbSource>()));

        return services;
    }
}