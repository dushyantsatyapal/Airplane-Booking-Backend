
using System.ComponentModel;
using Google.Cloud.Firestore;

namespace AirplaneBooking.Domain.Entities;

public class Booking
{
    public string Id { get; private set; } // Firebase ID
    public string UserId { get; private set; }
    public string FlightId { get; private set; } // Reference to Amadeus Flight Offer ID
    public List<Passenger> Passengers { get; private set; }
    [FirestoreProperty(ConverterType = typeof(DecimalConverter))] // <--- ADD THIS LINE

    public decimal TotalPrice { get; private set; }
    public DateTime BookingDate { get; private set; }
    public string Status { get; private set; } // e.g., "Confirmed", "Pending", "Cancelled"
    public string AmadeusBookingReference { get; private set; } // PNR from Amadeus

    public Booking(string id, string userId, string flightId, List<Passenger> passengers, decimal totalPrice, DateTime bookingDate, string status, string amadeusBookingReference = null)
    {
        Id = id;
        UserId = userId;
        FlightId = flightId;
        Passengers = passengers ?? new List<Passenger>();
        TotalPrice = totalPrice;
        BookingDate = bookingDate;
        Status = status;
        AmadeusBookingReference = amadeusBookingReference;
    }

    public void ConfirmBooking(string amadeusReference)
    {
        Status = "Confirmed";
        AmadeusBookingReference = amadeusReference;
    }

    public void CancelBooking()
    {
        Status = "Cancelled";
        // Add logic for Amadeus cancellation if needed
    }
    public void SetStatus(string newStatus)
    {
        Status = newStatus;
    }

}


