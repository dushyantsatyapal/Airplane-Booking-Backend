
using AirplaneBooking.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AirplaneBooking.Application;

public static class DependencyInjection // This static class holds extension methods
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register your application services here
        services.AddScoped<AmadeusService>();
        services.AddScoped<BookingService>();
        services.AddScoped<DbTestService>();

        return services;
    }
}