using BirthdayCollator.Server.Infrastructure.Throttling;
using BirthdayCollator.Server.Processing.Dates;
using BirthdayCollator.Server.Processing.Entries;
using BirthdayCollator.Server.Processing.Names;

namespace BirthdayCollator.Server.Extensions;

public static class BirthdayHelperExtensions
{
    public static IServiceCollection AddBirthdayHelpers(this IServiceCollection services)
    {
        services.AddSingleton<IEntrySplitter, EntrySplitter>();
        services.AddSingleton<ILinkResolver, LinkResolver>();
        services.AddSingleton<IPersonNameResolver, PersonNameResolver>();
        services.AddSingleton<IHtmlBirthSectionExtractor, HtmlBirthSectionExtractor>();
        services.AddSingleton<IBirthDateParser, BirthDateParser>();
        services.AddSingleton<IThrottleRegistry, ThrottleRegistry>();
        return services;
    }
}