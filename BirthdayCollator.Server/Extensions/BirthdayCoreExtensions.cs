using BirthdayCollator.Server.Processing.Builders;

namespace BirthdayCollator.Server.Extensions;

public static class BirthdayCoreExtensions
{
    public static IServiceCollection AddBirthdayCore(this IServiceCollection services)
    {
        services.AddSingleton<IYearRangeProvider, YearRangeProvider>();
        services.AddSingleton<Func<string, string>>(sp => WikiUrlBuilder.NormalizeWikiHref);
        return services;
    }
}