using HotelStay.Application.Abstractions;
using HotelStay.Application.Services;
using HotelStay.Domain.Abstractions;
using HotelStay.Infrastructure.Providers.BudgetNests;
using HotelStay.Infrastructure.Providers.PremierStays;
using HotelStay.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace HotelStay.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        // Application services
        services.AddScoped<IHotelSearchService, HotelSearchService>();
        services.AddScoped<IReservationService, ReservationService>();

        // Infrastructure providers
        services.AddSingleton<IHotelProvider, PremierStaysProvider>();
        services.AddSingleton<IHotelProvider, BudgetNestsProvider>();

        // Repository (in-memory singleton)
        services.AddSingleton<IReservationRepository, InMemoryReservationRepository>();

        return services;
    }
}
