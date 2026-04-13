namespace Hotel.Billing.Contracts;

public interface IBillingService
{
    Invoice GetInvoice(string reservationId);
}

public interface IBillingDataSource
{
    BillingReservation? GetReservation(string reservationId);
    BillingRoom? GetRoom(string roomId);
}

public sealed class BillingReservation
{
    public string Id { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public string RoomId { get; set; } = string.Empty;
    public string RoomTypeCode { get; set; } = string.Empty;
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public int GuestCount { get; set; }

    public int Nights => (CheckOut - CheckIn).Days;
}

public sealed class BillingRoom
{
    public string Id { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
}

public sealed class Invoice
{
    public string ReservationId { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public List<InvoiceLine> Lines { get; set; } = new();
    public decimal Total => Lines.Sum(line => line.Amount);

    public void Print()
    {
        Console.WriteLine($"  Invoice for {GuestName} (Reservation: {ReservationId})");
        foreach (var line in Lines)
            Console.WriteLine($"    {line.Description,-40} {line.Amount,10:C}");
        Console.WriteLine($"    {"TOTAL",-40} {Total,10:C}");
    }
}

public sealed class InvoiceLine
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
