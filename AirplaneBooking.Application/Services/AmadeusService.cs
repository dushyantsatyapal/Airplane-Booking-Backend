
using AirplaneBooking.Application.DTOs;
using AirplaneBooking.Application.Interfaces.Repositories;

namespace AirplaneBooking.Application.Services;

public class AmadeusService
{
    private readonly IAmadeusService _amadeusApiClient; // The interface from infrastructure

    /// <summary>
    /// Initializes a new instance of the AmadeusService.
    /// </summary>
    /// <param name="amadeusApiClient">The concrete implementation of IAmadeusService from the Infrastructure layer.</param>
    public AmadeusService(IAmadeusService amadeusApiClient)
    {
        _amadeusApiClient = amadeusApiClient ?? throw new ArgumentNullException(nameof(amadeusApiClient));
    }

    /// <summary>
    /// Searches for available flights based on the provided criteria.
    /// </summary>
    /// <param name="request">The DTO containing flight search parameters.</param>
    /// <returns>A collection of FlightOfferDto representing available flight offers.</returns>
    /// <exception cref="ArgumentException">Thrown if the search request contains invalid parameters.</exception>
    /// <exception cref="Application.Shared.Exceptions.ApplicationException">Thrown if an error occurs during the Amadeus API call.</exception>
    public async Task<IEnumerable<FlightOfferDto>> SearchFlightsAsync(FlightSearchRequestDto request)
    {

        if (string.IsNullOrWhiteSpace(request.OriginLocationCode))
        {
            throw new ArgumentException("Origin location code is required.", nameof(request.OriginLocationCode));
        }
        if (string.IsNullOrWhiteSpace(request.DestinationLocationCode))
        {
            throw new ArgumentException("Destination location code is required.", nameof(request.DestinationLocationCode));
        }
        if (request.DepartureDate < DateTime.Today.Date) // Compare with just date part
        {
            throw new ArgumentException("Departure date cannot be in the past.", nameof(request.DepartureDate));
        }
        if (request.ReturnDate.HasValue && request.ReturnDate.Value.Date < request.DepartureDate.Date) // Compare with just date part
        {
            throw new ArgumentException("Return date cannot be before departure date.", nameof(request.ReturnDate));
        }
        if (request.Adults < 1)
        {
            throw new ArgumentException("At least one adult is required for flight search.", nameof(request.Adults));
        }
        if (request.Adults + request.Children + request.Infants > 9) // Amadeus generally has a limit of 9 travelers
        {
            throw new ArgumentException("Total number of travelers cannot exceed 9 per search.", nameof(request.Adults));
        }

        try
        {
            var flightOffers = await _amadeusApiClient.SearchFlightsAsync(request);

            return flightOffers;
        }
        catch (Exception ex)
        {
            throw new AirplaneBooking.Shared.Exceptions.ApplicationException("Failed to retrieve flight offers from Amadeus.", ex);
        }
    }
}
