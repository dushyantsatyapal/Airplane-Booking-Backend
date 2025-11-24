using global::AirplaneBooking.Domain.Entities;
using System.Threading.Tasks;


namespace AirplaneBooking.Application.Interfaces.Repositories;


    // This interface defines the specific operations BookingService needs from the MongoDB repository.
    // It's in the Application layer, so Application depends on it (an abstraction).
    public interface IMongoDbBookingRepository
    {
        Task AddAsync(Booking booking);
        Task UpdateAsync(Booking booking);
        // Add other methods if BookingService needs to read/delete from Mongo specifically
    }