namespace Hotel.Booking.Contracts;

public enum RoomType
{
    Standard,
    Suite,
    Family
}

public sealed class BookingRequest
{
    public string GuestName { get; set; } = string.Empty;
    public RoomType RoomType { get; set; }
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public int GuestCount { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string CancellationPolicy { get; set; } = "Flexible";
}

public sealed class ReservationView
{
    public string Id { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public string RoomId { get; set; } = string.Empty;
    public RoomType RoomType { get; set; }
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public int GuestCount { get; set; }
    public string Status { get; set; } = "Confirmed";
    public string CancellationPolicyName { get; set; } = "Flexible";
    public string GuestEmail { get; set; } = string.Empty;
    public string GuestPhone { get; set; } = string.Empty;

    public int Nights => (CheckOut - CheckIn).Days;
}

public sealed class RoomView
{
    public string Id { get; set; } = string.Empty;
    public RoomType Type { get; set; }
    public int Capacity { get; set; }
    public decimal BasePrice { get; set; }
}

public interface IBookingService
{
    ReservationView CreateReservation(BookingRequest request);
    void CheckIn(string reservationId);
    void CheckOut(string reservationId);
}

public interface IReservationStore
{
    void Add(ReservationView reservation);
    ReservationView? GetById(string id);
    IReadOnlyList<ReservationView> GetAll();
    void Update(ReservationView reservation);
}

public interface IRoomInventory
{
    RoomView? GetById(string id);
    IReadOnlyList<RoomView> GetAvailable(
        DateTime from,
        DateTime to,
        IReadOnlyCollection<ReservationView> existingReservations);
}

public interface IBookingConfirmationSender
{
    void SendBookingConfirmation(
        string email,
        string guestName,
        string reservationId,
        DateTime checkIn,
        DateTime checkOut,
        string roomId);
}
