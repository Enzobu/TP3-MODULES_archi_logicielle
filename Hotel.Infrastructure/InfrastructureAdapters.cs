using Hotel.Billing.Contracts;
using Hotel.Booking.Contracts;
using Hotel.Housekeeping.Contracts;

namespace Hotel.Infrastructure;

internal sealed class InMemoryReservationStore :
    IReservationStore,
    IHousekeepingScheduleDataSource
{
    private readonly Dictionary<string, ReservationView> _store = new();

    public void Add(ReservationView reservation) => _store[reservation.Id] = reservation;

    public ReservationView? GetById(string id) => _store.GetValueOrDefault(id);

    public IReadOnlyList<ReservationView> GetAll() => _store.Values.ToList();

    public void Update(ReservationView reservation) => _store[reservation.Id] = reservation;

    public IReadOnlyList<HousekeepingReservation> GetReservationsByDateRange(DateTime from, DateTime to) =>
        _store.Values
            .Where(reservation =>
                reservation.Status != "Cancelled" &&
                reservation.CheckIn < to &&
                reservation.CheckOut > from)
            .Select(reservation => new HousekeepingReservation
            {
                Id = reservation.Id,
                RoomId = reservation.RoomId,
                CheckIn = reservation.CheckIn,
                CheckOut = reservation.CheckOut
            })
            .ToList();
}

internal sealed class InMemoryRoomInventory : IRoomInventory
{
    private readonly List<RoomView> _rooms;

    public InMemoryRoomInventory(IEnumerable<RoomView> rooms)
    {
        _rooms = rooms.ToList();
    }

    public RoomView? GetById(string id) => _rooms.FirstOrDefault(room => room.Id == id);

    public IReadOnlyList<RoomView> GetAvailable(
        DateTime from,
        DateTime to,
        IReadOnlyCollection<ReservationView> existingReservations)
    {
        var bookedRoomIds = existingReservations
            .Where(reservation =>
                reservation.Status != "Cancelled" &&
                reservation.CheckIn < to &&
                reservation.CheckOut > from)
            .Select(reservation => reservation.RoomId)
            .ToHashSet();

        return _rooms
            .Where(room => !bookedRoomIds.Contains(room.Id))
            .ToList();
    }
}

internal sealed class InMemoryBillingDataSource : IBillingDataSource
{
    private readonly InMemoryReservationStore _reservationStore;
    private readonly InMemoryRoomInventory _roomInventory;

    public InMemoryBillingDataSource(
        InMemoryReservationStore reservationStore,
        InMemoryRoomInventory roomInventory)
    {
        _reservationStore = reservationStore;
        _roomInventory = roomInventory;
    }

    public BillingReservation? GetReservation(string reservationId)
    {
        var reservation = _reservationStore.GetById(reservationId);
        if (reservation is null)
            return null;

        return new BillingReservation
        {
            Id = reservation.Id,
            GuestName = reservation.GuestName,
            RoomId = reservation.RoomId,
            RoomTypeCode = reservation.RoomType.ToString(),
            CheckIn = reservation.CheckIn,
            CheckOut = reservation.CheckOut,
            GuestCount = reservation.GuestCount
        };
    }

    public BillingRoom? GetRoom(string roomId)
    {
        var room = _roomInventory.GetById(roomId);
        if (room is null)
            return null;

        return new BillingRoom
        {
            Id = room.Id,
            BasePrice = room.BasePrice
        };
    }
}

internal sealed class EmailConfirmationSender : IBookingConfirmationSender
{
    public void SendBookingConfirmation(
        string email,
        string guestName,
        string reservationId,
        DateTime checkIn,
        DateTime checkOut,
        string roomId)
    {
        Console.WriteLine($"  [EMAIL] To: {email}");
        Console.WriteLine($"    Booking confirmed for {guestName}");
        Console.WriteLine($"    Reservation: {reservationId} | Room: {roomId}");
        Console.WriteLine($"    {checkIn:d} → {checkOut:d}");
    }
}

internal sealed class SmsCleaningNotifier : ICleaningTaskNotifier
{
    public void NotifyNewTasks(IReadOnlyList<CleaningTask> tasks)
    {
        foreach (var task in tasks)
            Console.WriteLine(
                $"  [SMS] Housekeeping: {task.Type} for room {task.RoomId} on {task.Date:d}");
    }
}
