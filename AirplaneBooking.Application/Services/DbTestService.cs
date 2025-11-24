using AirplaneBooking.Application.DTOs;
using AirplaneBooking.Application.Interfaces.Repositories;
using AirplaneBooking.Domain.Entities;
using Microsoft.Extensions.Logging;



namespace AirplaneBooking.Application.Services;


    public class DbTestService
    {
        private readonly IBookingRepository _firebaseBookingRepository;
        private readonly IMongoDbBookingRepository _mongoDbBookingRepository; // Use the interface for Clean Architecture
        private readonly ILogger<DbTestService> _logger;

        public DbTestService(
            IBookingRepository firebaseBookingRepository,
            IMongoDbBookingRepository mongoDbBookingRepository, // Injecting the interface
            ILogger<DbTestService> logger)
        {
            _firebaseBookingRepository = firebaseBookingRepository ?? throw new ArgumentNullException(nameof(firebaseBookingRepository));
            _mongoDbBookingRepository = mongoDbBookingRepository ?? throw new ArgumentNullException(nameof(mongoDbBookingRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> TestDbConnectionAndWrite(TestDbEntryDto data)
        {
            _logger.LogInformation($"Attempting to write test data: '{data.Message}' from '{data.Source}' to databases.");

            // Create a dummy Booking object as your repositories work with Booking entities
            var dummyBooking = new Booking(
                id: Guid.NewGuid().ToString(),
                userId: "TEST_USER_DB_CHECK",
                flightId: "TEST_FLIGHT_DB_CHECK",
                passengers: new List<Passenger> { new Passenger("Test", "User", DateTime.UtcNow.AddYears(-30), "test@example.com", "1234567890") },
                totalPrice: 0.00m,
                bookingDate: DateTime.UtcNow,
                status: data.Source, // Using Source as status for testing context
                amadeusBookingReference: data.Message // Using Message as reference for testing context
            );

            // 1. Try saving to Firebase
            try
            {
                _logger.LogInformation($"Attempting to save test data (ID: {dummyBooking.Id}) to Firebase.");
                await _firebaseBookingRepository.AddAsync(dummyBooking);
                _logger.LogInformation($"Test data (ID: {dummyBooking.Id}) successfully saved to Firebase.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save test data to Firebase.");
                return $"Firebase Error: {ex.Message}";
            }

            // 2. Try saving to MongoDB
            try
            {
                _logger.LogInformation($"Attempting to save test data (ID: {dummyBooking.Id}) to MongoDB.");
                await _mongoDbBookingRepository.AddAsync(dummyBooking);
                _logger.LogInformation($"Test data (ID: {dummyBooking.Id}) successfully saved to MongoDB.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save test data to MongoDB.");
                return $"MongoDB Error: {ex.Message}";
            }

            return $"Success: Data saved to both databases. Firebase ID: {dummyBooking.Id}";
        }
    }