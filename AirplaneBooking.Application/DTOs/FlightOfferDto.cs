
namespace AirplaneBooking.Application.DTOs;

public class FlightOfferDto
{
    public string Id { get; set; }
    public string CarrierCode { get; set; }
    public string FlightNumber { get; set; }
    public string DepartureAirportCode { get; set; }
    public string ArrivalAirportCode { get; set; }
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; }
    public int AvailableSeats { get; set; }
    public string RawJsonOffer { get; set; }

}
