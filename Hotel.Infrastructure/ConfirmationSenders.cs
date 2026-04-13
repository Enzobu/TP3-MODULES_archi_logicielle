using Hotel.Booking.Contracts;

namespace Hotel.Infrastructure;

internal sealed class PushConfirmationSender
{
    public void SendBookingConfirmation(
        string guestName,
        string reservationId,
        DateTime checkIn,
        DateTime checkOut,
        string roomId)
    {
        Console.WriteLine($"  [PUSH] Guest app notified for {guestName}");
        Console.WriteLine($"    Reservation: {reservationId} | Room: {roomId}");
        Console.WriteLine($"    {checkIn:d} → {checkOut:d}");
    }
}

internal sealed class MultiChannelBookingConfirmationSender : IBookingConfirmationSender
{
    private readonly EmailConfirmationSender _emailSender;
    private readonly PushConfirmationSender _pushSender;

    public MultiChannelBookingConfirmationSender(
        EmailConfirmationSender emailSender,
        PushConfirmationSender pushSender)
    {
        _emailSender = emailSender;
        _pushSender = pushSender;
    }

    public void SendBookingConfirmation(
        string email,
        string guestName,
        string reservationId,
        DateTime checkIn,
        DateTime checkOut,
        string roomId)
    {
        _emailSender.SendBookingConfirmation(
            email,
            guestName,
            reservationId,
            checkIn,
            checkOut,
            roomId);

        _pushSender.SendBookingConfirmation(
            guestName,
            reservationId,
            checkIn,
            checkOut,
            roomId);
    }
}
