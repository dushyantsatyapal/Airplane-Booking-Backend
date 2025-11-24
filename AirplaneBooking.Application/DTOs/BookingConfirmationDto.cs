
namespace AirplaneBooking.Application.DTOs;

public class BookingConfirmationDto
{
    public string BookingId { get; set; }
    public string AmadeusBookingReference { get; set; }
    public string Status { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime BookingDate { get; set; }
}
