
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AirplaneBooking.Application.DTOs;

public class BookFlightRequestDto
{
    //public string FlightOfferId { get; set; }
    //public string UserId { get; set; }
    //public List<PassengerDto> Passengers { get; set; }

    [Required]
    [JsonPropertyName("userId")]
    public string UserId { get; set; }

    [Required]
    [MinLength(1)]
    [JsonPropertyName("passengers")]
    public List<PassengerDto> Passengers { get; set; }

    [Required]
    [JsonPropertyName("flightOfferJson")]
    public string FlightOfferJson { get; set; }

}
