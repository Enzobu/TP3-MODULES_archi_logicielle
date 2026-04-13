using Hotel.Booking.Contracts;

namespace Hotel.Booking;

internal sealed class BookingService : IBookingService
{
    private readonly IReservationStore _reservationStore;
    private readonly RoomAssigner _roomAssigner;
    private readonly IBookingConfirmationSender _confirmationSender;
    private int _counter;

    public BookingService(
        IReservationStore reservationStore,
        RoomAssigner roomAssigner,
        IBookingConfirmationSender confirmationSender)
    {
        _reservationStore = reservationStore;
        _roomAssigner = roomAssigner;
        _confirmationSender = confirmationSender;
    }

    public ReservationView CreateReservation(BookingRequest request)
    {
        if (request.CheckOut <= request.CheckIn)
            throw new ArgumentException("Check-out must be after check-in");

        var room = _roomAssigner.FindAvailableRoom(
            request.RoomType,
            request.CheckIn,
            request.CheckOut,
            request.GuestCount)
            ?? throw new Exception(
                $"No {request.RoomType} room available for {request.CheckIn:d} → {request.CheckOut:d}");

        if (request.GuestCount > room.Capacity)
            throw new Exception(
                $"Room {room.Id} capacity is {room.Capacity}, requested {request.GuestCount}");

        _counter++;
        var reservation = new ReservationView
        {
            Id = $"R-{_counter:D3}",
            GuestName = request.GuestName,
            RoomId = room.Id,
            RoomType = request.RoomType,
            CheckIn = request.CheckIn,
            CheckOut = request.CheckOut,
            GuestCount = request.GuestCount,
            GuestEmail = request.Email,
            GuestPhone = request.Phone,
            CancellationPolicyName = request.CancellationPolicy
        };

        _reservationStore.Add(reservation);
        _confirmationSender.SendBookingConfirmation(
            request.Email,
            request.GuestName,
            reservation.Id,
            request.CheckIn,
            request.CheckOut,
            room.Id);

        Console.WriteLine(
            $"  Reservation {reservation.Id} created for {request.GuestName} in room {room.Id}");

        return reservation;
    }

    public void CheckIn(string reservationId)
    {
        var reservation = _reservationStore.GetById(reservationId)
            ?? throw new Exception($"Reservation {reservationId} not found");

        if (reservation.Status != "Confirmed")
            throw new Exception($"Cannot check in: status is {reservation.Status}");

        reservation.Status = "CheckedIn";
        _reservationStore.Update(reservation);
        Console.WriteLine(
            $"  Guest {reservation.GuestName} checked in to room {reservation.RoomId}");
    }

    public void CheckOut(string reservationId)
    {
        var reservation = _reservationStore.GetById(reservationId)
            ?? throw new Exception($"Reservation {reservationId} not found");

        if (reservation.Status != "CheckedIn")
            throw new Exception($"Cannot check out: status is {reservation.Status}");

        reservation.Status = "CheckedOut";
        _reservationStore.Update(reservation);
        Console.WriteLine(
            $"  Guest {reservation.GuestName} checked out of room {reservation.RoomId}");
    }
}

internal sealed class RoomAssigner
{
    private readonly IRoomInventory _roomInventory;
    private readonly IReservationStore _reservationStore;

    public RoomAssigner(IRoomInventory roomInventory, IReservationStore reservationStore)
    {
        _roomInventory = roomInventory;
        _reservationStore = reservationStore;
    }

    public RoomView? FindAvailableRoom(
        RoomType type,
        DateTime checkIn,
        DateTime checkOut,
        int guestCount)
    {
        var existingReservations = _reservationStore.GetAll();
        var availableRooms = _roomInventory.GetAvailable(checkIn, checkOut, existingReservations);

        return availableRooms.FirstOrDefault(room =>
            room.Type == type && room.Capacity >= guestCount);
    }
}
