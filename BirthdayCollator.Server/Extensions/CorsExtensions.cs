namespace BirthdayCollator.Server.Extensions;

public static class CorsExtensions
{
    public static IServiceCollection AddBirthdayCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                //policy.WithOrigins("http://localhost:5178", 
                //                   "http://localhost:5173",
                //                   "http://localhost:5174",
                //                   "http://localhost:5176")
                //      .AllowAnyHeader()
                //      .AllowAnyMethod();
                policy.SetIsOriginAllowed(_ => true)
               .AllowAnyHeader()
               .AllowAnyMethod();
            });
        });

        return services;
    }
}
