using Hotel.Billing.Contracts;
using Hotel.Booking.Contracts;
using Hotel.Housekeeping.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Hotel.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IEnumerable<RoomView> rooms)
    {
        services.AddSingleton(new InMemoryRoomInventory(rooms));
        services.AddSingleton<IRoomInventory>(serviceProvider =>
            serviceProvider.GetRequiredService<InMemoryRoomInventory>());

        services.AddSingleton<InMemoryReservationStore>();
        services.AddSingleton<IReservationStore>(serviceProvider =>
            serviceProvider.GetRequiredService<InMemoryReservationStore>());
        services.AddSingleton<IHousekeepingScheduleDataSource>(serviceProvider =>
            serviceProvider.GetRequiredService<InMemoryReservationStore>());

        services.AddSingleton<IBillingDataSource, InMemoryBillingDataSource>();
        services.AddSingleton<IBookingConfirmationSender, EmailConfirmationSender>();
        services.AddSingleton<ICleaningTaskNotifier, SmsCleaningNotifier>();

        return services;
    }
}
