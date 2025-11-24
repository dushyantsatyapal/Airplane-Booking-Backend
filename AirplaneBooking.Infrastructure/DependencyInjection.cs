using AirplaneBooking.Application.Interfaces.Repositories;
using AirplaneBooking.Infrastructure.ExternalServices;
using AirplaneBooking.Infrastructure.Persistence;
using AirplaneBooking.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using AirplaneBooking.Infrastructure.Persistence.Repositories;

namespace AirplaneBooking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLogging(builder => builder.AddConsole().AddDebug().SetMinimumLevel(LogLevel.Information));

        services.AddSingleton<FirebaseDbContext>();
        services.AddScoped<IBookingRepository, FirebaseBookingRepository>(); // Primary booking repository  

        services.AddSingleton<MongoDbContext>();
        services.AddScoped<IMongoDbBookingRepository, MongoDbBookingRepository>(); // <-- REGISTER NEW INTERFACE  

        services.AddHttpClient<IAmadeusService, AmadeusApiClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["Amadeus:BaseUrl"] ?? "https://test.api.amadeus.com");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        })
        .SetHandlerLifetime(TimeSpan.FromMinutes(5))
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13
        });

        return services;
    }
}
