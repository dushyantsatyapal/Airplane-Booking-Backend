using AirplaneBooking.Application.DTOs;
using AirplaneBooking.Application.Services;
using AirplaneBooking.Shared.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApplicationException = AirplaneBooking.Shared.Exceptions.ApplicationException;

namespace AirplaneBooking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FlightsController : ControllerBase
{
    private readonly AmadeusService _amadeusService;
    private readonly BookingService _bookingService;

    public FlightsController(AmadeusService amadeusService, BookingService bookingService)
    {
        _amadeusService = amadeusService ?? throw new ArgumentNullException(nameof(amadeusService));
        _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<FlightOfferDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SearchFlights([FromQuery] FlightSearchRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var flightOffers = await _amadeusService.SearchFlightsAsync(request);
            return Ok(flightOffers);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
        }
    }

    [HttpPost("book")]
    [ProducesResponseType(typeof(BookingConfirmationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> BookFlight([FromBody] BookFlightRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var bookingConfirmation = await _bookingService.CreateBookingAsync(request);
            return Ok(bookingConfirmation);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred during booking. Please try again later.");
        }
    }

    [HttpGet("{bookingId}")]
    [ProducesResponseType(typeof(BookingConfirmationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBooking(string bookingId)
    {
        try
        {
            var booking = await _bookingService.GetBookingDetailsAsync(bookingId);
            if (booking == null)
            {
                return NotFound($"Booking with ID {bookingId} not found.");
            }
            return Ok(booking);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
        }
    }

    [HttpDelete("{bookingId}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CancelBooking(string bookingId)
    {
        try
        {
            var cancelled = await _bookingService.CancelBookingAsync(bookingId);
            if (!cancelled)
            {
                return BadRequest("Could not cancel the booking for an unknown reason.");
            }
            return NoContent();
        }
        catch (ApplicationException ex)
        {
            if (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { error = ex.Message });
            }
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred during cancellation. Please try again later.");
        }
    }
}