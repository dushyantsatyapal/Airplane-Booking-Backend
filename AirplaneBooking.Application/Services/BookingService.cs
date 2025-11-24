// AirplaneBooking.Application/Services/BookingService.cs

using AirplaneBooking.Application.DTOs;
using AirplaneBooking.Application.Interfaces.Repositories;
using AirplaneBooking.Domain.Entities;
using AirplaneBooking.Shared.Constants;
using AirplaneBooking.Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using ApplicationException = AirplaneBooking.Shared.Exceptions.ApplicationException; // For JsonSerializer, JsonElement

namespace AirplaneBooking.Application.Services
{
    public class BookingService
    {
        private readonly IBookingRepository _firebaseBookingRepository;
        private readonly IMongoDbBookingRepository _mongoDbBookingRepository;
        private readonly IAmadeusService _amadeusApiClient;
        private readonly ILogger<BookingService> _logger;

        public BookingService(
            IBookingRepository firebaseBookingRepository,
            IMongoDbBookingRepository mongoDbBookingRepository,
            IAmadeusService amadeusApiClient,
            ILogger<BookingService> logger)
        {
            _firebaseBookingRepository = firebaseBookingRepository ??
                                         throw new ArgumentNullException(nameof(firebaseBookingRepository));
            _mongoDbBookingRepository = mongoDbBookingRepository ??
                                        throw new ArgumentNullException(nameof(mongoDbBookingRepository));
            _amadeusApiClient = amadeusApiClient ??
                                throw new ArgumentNullException(nameof(amadeusApiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<BookingConfirmationDto> CreateBookingAsync(BookFlightRequestDto request)
        {
            _logger.LogInformation("Attempting to create a new flight booking.");

            // 1. Validate the incoming request DTO
            if (string.IsNullOrWhiteSpace(request.FlightOfferJson))
            {
                _logger.LogWarning("Flight offer JSON is missing in booking request.");
                throw new ApplicationException("Flight offer JSON is required.");
            }
            if (request.Passengers == null || !request.Passengers.Any())
            {
                _logger.LogWarning("No passengers provided in booking request.");
                throw new ApplicationException("At least one passenger is required for booking.");
            }
            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                _logger.LogWarning("User ID is missing in booking request.");
                throw new ApplicationException("User ID is required for booking.");
            }

            // --- MODIFIED LOGIC: BYPASS ALL AMADEUS API CALLS ---
            // 2. Simulate a successful Amadeus booking.
            var flightOfferJsonElement = JsonSerializer.Deserialize<JsonElement>(request.FlightOfferJson);
            var flightId = flightOfferJsonElement.GetProperty("id").GetString();
            var totalPriceString = flightOfferJsonElement.GetProperty("price").GetProperty("grandTotal").GetString();
            var totalPrice = decimal.Parse(totalPriceString);

            var dummyAmadeusReference = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
            var amadeusBookingResult = new BookingConfirmationDto
            {
                BookingId = Guid.NewGuid().ToString(), // A unique ID for the booking
                AmadeusBookingReference = dummyAmadeusReference, // A dummy PNR
                Status = "CONFIRMED",
                TotalPrice = totalPrice,
                BookingDate = DateTime.UtcNow
            };
            _logger.LogInformation($"Successfully simulated Amadeus booking. Reference: {dummyAmadeusReference}");
            // --- END OF MODIFIED LOGIC ---

            // 3. Map DTOs to Domain Entities for internal persistence
            var passengers = request.Passengers.Select(p =>
                new Passenger(p.FirstName, p.LastName, p.DateOfBirth, p.Email, p.PhoneNumber)).ToList();

            var newBooking = new Booking(
                id: amadeusBookingResult.BookingId,
                userId: request.UserId,
                flightId: flightId,
                passengers: passengers,
                totalPrice: amadeusBookingResult.TotalPrice,
                bookingDate: amadeusBookingResult.BookingDate,
                status: amadeusBookingResult.Status,
                amadeusBookingReference: amadeusBookingResult.AmadeusBookingReference
            );

            // 4. Save booking to Firebase FIRST
            try
            {
                _logger.LogInformation($"Attempting to save booking record (ID: {newBooking.Id}) to Firebase.");
                await _firebaseBookingRepository.AddAsync(newBooking);
                _logger.LogInformation($"Booking record (ID: {newBooking.Id}) successfully saved to Firebase.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Critical failure: Failed to save booking record (ID: {newBooking.Id}) to Firebase.");
                throw new ApplicationException("Booking failed to persist in primary database. Manual review may be needed.", ex);
            }

            // 5. Save booking to MongoDB SECOND
            try
            {
                _logger.LogInformation($"Attempting to save booking record (ID: {newBooking.Id}) to MongoDB.");
                await _mongoDbBookingRepository.AddAsync(newBooking);
                _logger.LogInformation($"Booking record (ID: {newBooking.Id}) successfully saved to MongoDB.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Warning: Failed to save booking record (ID: {newBooking.Id}) to MongoDB after successful Firebase save.");
            }

            return amadeusBookingResult;
        }

        public async Task<BookingConfirmationDto> GetBookingDetailsAsync(string bookingId)
        {
            _logger.LogInformation($"Retrieving booking details for ID: {bookingId}.");
            var booking = await _firebaseBookingRepository.GetByIdAsync(bookingId);

            if (booking == null)
            {
                _logger.LogWarning($"Booking with ID: {bookingId} not found in Firebase.");
                return null;
            }

            return new BookingConfirmationDto
            {
                BookingId = booking.Id,
                AmadeusBookingReference = booking.AmadeusBookingReference,
                Status = booking.Status,
                TotalPrice = booking.TotalPrice,
                BookingDate = booking.BookingDate
            };
        }

        public async Task<bool> CancelBookingAsync(string bookingId)
        {
            _logger.LogInformation($"Attempting to cancel booking with ID: {bookingId}.");

            var booking = await _firebaseBookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
            {
                _logger.LogWarning($"Attempted to cancel non-existent booking with ID: {bookingId}.");
                throw new ApplicationException($"Booking with ID {bookingId} not found.");
            }
            if (booking.Status == BookingStatuses.Cancelled)
            {
                _logger.LogInformation($"Booking {bookingId} is already cancelled. No further action needed.");
                throw new ApplicationException("Booking is already cancelled.");
            }
            if (string.IsNullOrWhiteSpace(booking.AmadeusBookingReference))
            {
                _logger.LogError($"Booking {bookingId} cannot be cancelled as it has no Amadeus reference. Manual intervention required.");
                throw new ApplicationException("Cannot cancel booking, no Amadeus reference found.");
            }

            bool amadeusCancelled;
            try
            {
                _logger.LogInformation($"Calling Amadeus API to cancel booking reference: {booking.AmadeusBookingReference} for internal booking ID: {bookingId}.");
                amadeusCancelled = await _amadeusApiClient.CancelBookingAsync(booking.AmadeusBookingReference);
                _logger.LogInformation($"Amadeus cancellation result for {booking.AmadeusBookingReference}: {amadeusCancelled}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to cancel booking {bookingId} with Amadeus API. Aborting local status update.");
                throw new ApplicationException("Failed to cancel booking with Amadeus. Please try again.", ex);
            }

            if (amadeusCancelled)
            {
                booking.CancelBooking();

                try
                {
                    _logger.LogInformation($"Updating booking {booking.Id} status to '{BookingStatuses.Cancelled}' in Firebase.");
                    await _firebaseBookingRepository.UpdateAsync(booking);
                    _logger.LogInformation($"Booking {booking.Id} status updated in Firebase.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to update booking {booking.Id} status in Firebase after Amadeus cancellation. Manual intervention may be required.");
                }

                try
                {
                    _logger.LogInformation($"Updating booking {booking.Id} status to '{BookingStatuses.Cancelled}' in MongoDB.");
                    await _mongoDbBookingRepository.UpdateAsync(booking);
                    _logger.LogInformation($"Booking {booking.Id} status updated in MongoDB.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to update booking {booking.Id} status in MongoDB after Amadeus cancellation. (Secondary DB failure)");
                }

                return true;
            }
            return false;
        }
    }
}