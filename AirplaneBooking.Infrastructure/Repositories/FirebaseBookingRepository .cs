using AirplaneBooking.Application.Interfaces.Repositories;
using AirplaneBooking.Domain.Entities;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;

namespace AirplaneBooking.Infrastructure.Persistence.Repositories;

public class FirebaseBookingRepository : IBookingRepository
{
    private readonly FirestoreDb _firestoreDb;
    private readonly ILogger<FirebaseBookingRepository> _logger;
    private const string CollectionName = "bookings";

    public FirebaseBookingRepository(FirebaseDbContext context, ILogger<FirebaseBookingRepository> logger)
    {
        _firestoreDb = context.FirestoreDb;
        _logger = logger;
    }

    public async Task AddAsync(Booking booking)
    {
        try
        {
            DocumentReference docRef = _firestoreDb.Collection(CollectionName).Document(booking.Id);

            // Manually map Booking properties to a dictionary to handle private setters and custom conversions
            var firestoreBookingData = new Dictionary<string, object>
            {
                { "Id", booking.Id },
                { "UserId", booking.UserId },
                { "FlightId", booking.FlightId },
                { "Passengers", booking.Passengers.Select(p => new Dictionary<string, object> {
                    { "FirstName", p.FirstName },
                    { "LastName", p.LastName },
                    { "DateOfBirth", Timestamp.FromDateTime(p.DateOfBirth.ToUniversalTime()) }, // Ensure DateTime to Timestamp conversion
                    { "Email", p.Email },
                    { "PhoneNumber", p.PhoneNumber }
                }).ToList() },
                // Manually apply the same logic as your DecimalAsLongConverter for storing
                { "TotalPrice", (long)Math.Round(booking.TotalPrice * 100m) }, // Store as cents
                { "BookingDate", Timestamp.FromDateTime(booking.BookingDate.ToUniversalTime()) }, // Ensure DateTime to Timestamp conversion
                { "Status", booking.Status },
                { "AmadeusBookingReference", booking.AmadeusBookingReference }
            };
            await docRef.SetAsync(firestoreBookingData);
            _logger.LogInformation($"Booking {booking.Id} added to Firestore.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error adding booking {booking.Id} to Firestore.");
            throw new Exception("Failed to add booking to database.", ex);
        }
    }

    public async Task<Booking> GetByIdAsync(string id)
    {
        try
        {
            DocumentReference docRef = _firestoreDb.Collection(CollectionName).Document(id);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                var data = snapshot.ToDictionary();
                var passengersData = data.GetValueOrDefault("Passengers") as List<object>;

                return new Booking(
                    id: snapshot.Id,
                    userId: data.GetValueOrDefault("UserId")?.ToString(),
                    flightId: data.GetValueOrDefault("FlightId")?.ToString(),
                    passengers: passengersData?.Select(p => {
                        var passengerData = (Dictionary<string, object>)p;
                        DateTime dob = DateTime.MinValue;
                        if (passengerData.TryGetValue("DateOfBirth", out var dobObj) && dobObj is Timestamp dobTimestamp)
                        {
                            dob = dobTimestamp.ToDateTime().ToLocalTime();
                        }

                        return new Passenger(
                            firstName: passengerData.GetValueOrDefault("FirstName")?.ToString(),
                            lastName: passengerData.GetValueOrDefault("LastName")?.ToString(),
                            dateOfBirth: dob,
                            email: passengerData.GetValueOrDefault("Email")?.ToString(),
                            phoneNumber: passengerData.GetValueOrDefault("PhoneNumber")?.ToString()
                        );
                    }).ToList() ?? new List<Passenger>(),
                    totalPrice: Convert.ToDecimal(data.GetValueOrDefault("TotalPrice")) / 100m,
                    bookingDate: data.TryGetValue("BookingDate", out var bdObj) && bdObj is Timestamp bdTimestamp
                        ? bdTimestamp.ToDateTime().ToLocalTime()
                        : DateTime.MinValue,
                    status: data.GetValueOrDefault("Status")?.ToString(),
                    amadeusBookingReference: data.GetValueOrDefault("AmadeusBookingReference")?.ToString()
                );
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting booking {id} from Firestore.");
            throw new Exception("Failed to retrieve booking from database.", ex);
        }
    }

    public async Task UpdateAsync(Booking booking)
    {
        try
        {
            DocumentReference docRef = _firestoreDb.Collection(CollectionName).Document(booking.Id);
            var updates = new Dictionary<string, object>
            {
                { "Status", booking.Status },
                { "AmadeusBookingReference", booking.AmadeusBookingReference },
                { "TotalPrice", (long)Math.Round(booking.TotalPrice * 100m) } // Update TotalPrice as cents
                // Update other fields if they can change
            };
            await docRef.UpdateAsync(updates);
            _logger.LogInformation($"Booking {booking.Id} updated in Firestore.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating booking {booking.Id} in Firestore.");
            throw new Exception("Failed to update booking in database.", ex);
        }
    }

    public async Task DeleteAsync(string id)
    {
        try
        {
            DocumentReference docRef = _firestoreDb.Collection(CollectionName).Document(id);
            await docRef.DeleteAsync();
            _logger.LogInformation($"Booking {id} deleted from Firestore.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting booking {id} from Firestore.");
            throw new Exception("Failed to delete booking from database.", ex);
        }
    }
}
