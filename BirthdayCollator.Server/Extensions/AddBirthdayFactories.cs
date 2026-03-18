using BirthdayCollator.Server.Processing.Validation;

namespace BirthdayCollator.Server.Extensions;

public static class BirthdayFactoryExtensions
{
    public static IServiceCollection AddBirthdayFactories(this IServiceCollection services)
    {
        services.AddScoped<PersonFactory>();
        services.AddScoped<BirthEntryValidator>();
        return services;
    }
}