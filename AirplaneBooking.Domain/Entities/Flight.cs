
using System.ComponentModel;
using Google.Cloud.Firestore;

namespace AirplaneBooking.Domain.Entities;

public class Flight
{
    public string Id { get; private set; } // Amadeus Flight Offer ID
    public string CarrierCode { get; private set; }
    public string FlightNumber { get; private set; }
    public string DepartureAirportCode { get; private set; }
    public string ArrivalAirportCode { get; private set; }
    public DateTime DepartureTime { get; private set; }
    public DateTime ArrivalTime { get; private set; }
    [FirestoreProperty(ConverterType = typeof(DecimalConverter))] // <--- ADD THIS LINE

    public decimal Price { get; private set; }
    public string Currency { get; private set; }
    public int AvailableSeats { get; private set; }

    // Constructor and methods for domain logic
    public Flight(string id, string carrierCode, string flightNumber, string departureAirportCode, string arrivalAirportCode, DateTime departureTime, DateTime arrivalTime, decimal price, string currency, int availableSeats)
    {
        Id = id;
        CarrierCode = carrierCode;
        FlightNumber = flightNumber;
        DepartureAirportCode = departureAirportCode;
        ArrivalAirportCode = arrivalAirportCode;
        DepartureTime = departureTime;
        ArrivalTime = arrivalTime;
        Price = price;
        Currency = currency;
        AvailableSeats = availableSeats;
    }
}
