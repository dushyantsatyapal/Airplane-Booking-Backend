
namespace AirplaneBooking.Application.DTOs;

public class FlightSearchRequestDto
{
    public string OriginLocationCode { get; set; }
    public string DestinationLocationCode { get; set; }
    public DateTime DepartureDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public int Adults { get; set; }
    public int Children { get; set; }
    public int Infants { get; set; }
    public string TravelClass { get; set; } // e.g., ECONOMY, PREMIUM_ECONOMY, BUSINESS, FIRST
}
