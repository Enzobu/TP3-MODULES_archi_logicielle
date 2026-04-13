using Hotel.Billing.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Hotel.Billing;

public static class ServiceRegistration
{
    public static IServiceCollection AddBillingModule(this IServiceCollection services)
    {
        services.AddSingleton<PricingStrategyFactory>();
        services.AddSingleton<TaxCalculator>();
        services.AddSingleton<InvoiceGenerator>();
        services.AddSingleton<IBillingService, BillingService>();
        return services;
    }
}
