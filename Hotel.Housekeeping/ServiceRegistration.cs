using Hotel.Housekeeping.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Hotel.Housekeeping;

public static class ServiceRegistration
{
    public static IServiceCollection AddHousekeepingModule(this IServiceCollection services)
    {
        services.AddSingleton<ICleaningPolicy, StandardCleaningPolicy>();
        services.AddSingleton<IHousekeepingService, HousekeepingService>();
        return services;
    }
}
