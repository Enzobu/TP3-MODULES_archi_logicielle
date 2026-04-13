using Hotel.Billing;
using Hotel.Billing.Contracts;
using Hotel.Booking;
using Hotel.Booking.Contracts;
using Hotel.Housekeeping;
using Hotel.Housekeeping.Contracts;
using Hotel.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("=== Le Mas des Oliviers — Hotel Management ===\n");

var rooms = new List<RoomView>
{
    new() { Id = "101", Type = RoomType.Standard, Capacity = 2, BasePrice = 80m },
    new() { Id = "102", Type = RoomType.Standard, Capacity = 2, BasePrice = 80m },
    new() { Id = "201", Type = RoomType.Suite, Capacity = 2, BasePrice = 200m },
    new() { Id = "301", Type = RoomType.Family, Capacity = 4, BasePrice = 120m }
};

var services = new ServiceCollection();
services.AddBookingModule();
services.AddBillingModule();
services.AddHousekeepingModule();
services.AddInfrastructure(rooms);

using var serviceProvider = services.BuildServiceProvider();

var bookingService = serviceProvider.GetRequiredService<IBookingService>();
var billingService = serviceProvider.GetRequiredService<IBillingService>();
var housekeepingService = serviceProvider.GetRequiredService<IHousekeepingService>();

Console.WriteLine("--- Creating reservations ---\n");

var alice = bookingService.CreateReservation(new BookingRequest
{
    GuestName = "Alice Martin",
    RoomType = RoomType.Standard,
    CheckIn = new DateTime(2025, 6, 15),
    CheckOut = new DateTime(2025, 6, 18),
    GuestCount = 2,
    Email = "alice@example.com",
    Phone = "+33612345001"
});

Console.WriteLine();

var bob = bookingService.CreateReservation(new BookingRequest
{
    GuestName = "Bob Dupont",
    RoomType = RoomType.Suite,
    CheckIn = new DateTime(2025, 6, 15),
    CheckOut = new DateTime(2025, 6, 22),
    GuestCount = 2,
    Email = "bob@example.com",
    Phone = "+33612345002"
});

Console.WriteLine();

var durand = bookingService.CreateReservation(new BookingRequest
{
    GuestName = "Famille Durand",
    RoomType = RoomType.Family,
    CheckIn = new DateTime(2025, 6, 20),
    CheckOut = new DateTime(2025, 6, 25),
    GuestCount = 4,
    Email = "durand@example.com",
    Phone = "+33612345003"
});

Console.WriteLine("\n--- Attempting conflicting reservation ---\n");

try
{
    bookingService.CreateReservation(new BookingRequest
    {
        GuestName = "Charlie Noir",
        RoomType = RoomType.Standard,
        CheckIn = new DateTime(2025, 6, 16),
        CheckOut = new DateTime(2025, 6, 19),
        GuestCount = 2,
        Email = "charlie@example.com",
        Phone = "+33612345004"
    });
}
catch (Exception exception)
{
    Console.WriteLine($"  Expected conflict: {exception.Message}");
}

Console.WriteLine("\n--- Check-in ---\n");
bookingService.CheckIn(alice.Id);

Console.WriteLine("\n--- Invoice for Bob ---\n");
var invoice = billingService.GetInvoice(bob.Id);
invoice.Print();

Console.WriteLine("\n--- Housekeeping schedule for June 18 ---\n");
var schedule = housekeepingService.GetSchedule(new DateTime(2025, 6, 18));
if (schedule.Count == 0)
{
    Console.WriteLine("  No cleaning tasks for this date.");
}
else
{
    foreach (var task in schedule)
        Console.WriteLine(
            $"  [{task.Type}] Room {task.RoomId} (Reservation: {task.ReservationId})");
}

Console.WriteLine("\n--- Check-out ---\n");
bookingService.CheckOut(alice.Id);

Console.WriteLine("\n=== Done ===");
