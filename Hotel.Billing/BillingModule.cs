using Hotel.Billing.Contracts;

namespace Hotel.Billing;

internal sealed class BillingService : IBillingService
{
    private readonly InvoiceGenerator _invoiceGenerator;
    private readonly IBillingDataSource _dataSource;

    public BillingService(InvoiceGenerator invoiceGenerator, IBillingDataSource dataSource)
    {
        _invoiceGenerator = invoiceGenerator;
        _dataSource = dataSource;
    }

    public Invoice GetInvoice(string reservationId)
    {
        var reservation = _dataSource.GetReservation(reservationId)
            ?? throw new Exception($"Reservation {reservationId} not found");

        return _invoiceGenerator.Generate(reservation);
    }
}

internal sealed class InvoiceGenerator
{
    private readonly PricingStrategyFactory _pricingFactory;
    private readonly TaxCalculator _taxCalculator;
    private readonly IBillingDataSource _dataSource;

    public InvoiceGenerator(
        PricingStrategyFactory pricingFactory,
        TaxCalculator taxCalculator,
        IBillingDataSource dataSource)
    {
        _pricingFactory = pricingFactory;
        _taxCalculator = taxCalculator;
        _dataSource = dataSource;
    }

    public Invoice Generate(BillingReservation reservation)
    {
        var room = _dataSource.GetRoom(reservation.RoomId)
            ?? throw new Exception($"Room {reservation.RoomId} not found");

        var pricingStrategy = _pricingFactory.Create(reservation.RoomTypeCode);
        var nightRate = pricingStrategy.CalculateNightRate(room);
        var subtotal = nightRate * reservation.Nights;
        var tva = _taxCalculator.CalculateTva(subtotal);
        var touristTax = _taxCalculator.CalculateTouristTax(
            reservation.GuestCount,
            reservation.Nights);

        return new Invoice
        {
            ReservationId = reservation.Id,
            GuestName = reservation.GuestName,
            Lines = new List<InvoiceLine>
            {
                new()
                {
                    Description = $"{reservation.Nights} night(s) × {nightRate:C}/night",
                    Amount = subtotal
                },
                new() { Description = "TVA (12%)", Amount = tva },
                new()
                {
                    Description =
                        $"Tourist tax ({reservation.GuestCount} pers. × {reservation.Nights} nights × 1.50€)",
                    Amount = touristTax
                }
            }
        };
    }
}

internal sealed class TaxCalculator
{
    private const decimal AccommodationTvaRate = 0.12m;
    private const decimal TouristTaxPerPersonPerNight = 1.50m;

    public decimal CalculateTva(decimal subtotal) => subtotal * AccommodationTvaRate;

    public decimal CalculateTouristTax(int guestCount, int nights) =>
        guestCount * nights * TouristTaxPerPersonPerNight;
}

internal interface IPricingStrategy
{
    decimal CalculateNightRate(BillingRoom room);
}

internal sealed class StandardPricingStrategy : IPricingStrategy
{
    public decimal CalculateNightRate(BillingRoom room) => room.BasePrice;
}

internal sealed class SuitePricingStrategy : IPricingStrategy
{
    public decimal CalculateNightRate(BillingRoom room) => room.BasePrice * 1.2m;
}

internal sealed class FamilyPricingStrategy : IPricingStrategy
{
    public decimal CalculateNightRate(BillingRoom room) => room.BasePrice * 0.9m;
}

internal sealed class PricingStrategyFactory
{
    public IPricingStrategy Create(string roomTypeCode) => roomTypeCode switch
    {
        "Standard" => new StandardPricingStrategy(),
        "Suite" => new SuitePricingStrategy(),
        "Family" => new FamilyPricingStrategy(),
        _ => new StandardPricingStrategy()
    };
}
