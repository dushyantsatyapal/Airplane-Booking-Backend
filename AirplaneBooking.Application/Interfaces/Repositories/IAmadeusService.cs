
using AirplaneBooking.Application.DTOs;

namespace AirplaneBooking.Application.Interfaces.Repositories;

public interface IAmadeusService
{
    Task<IEnumerable<FlightOfferDto>> SearchFlightsAsync(FlightSearchRequestDto request);
    //Task<BookingConfirmationDto> BookFlightAsync(string flightOfferId, List<PassengerDto> passengers);
    Task<BookingConfirmationDto> BookFlightAsync(string rawFlightOfferJson, List<PassengerDto> passengers);

    Task<bool> CancelBookingAsync(string amadeusBookingReference);
}
