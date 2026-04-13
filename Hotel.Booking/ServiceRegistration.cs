using Hotel.Booking.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Hotel.Booking;

public static class ServiceRegistration
{
    public static IServiceCollection AddBookingModule(this IServiceCollection services)
    {
        services.AddSingleton<RoomAssigner>();
        services.AddSingleton<IBookingService, BookingService>();
        return services;
    }
}
