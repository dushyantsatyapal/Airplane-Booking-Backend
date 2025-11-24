
using AirplaneBooking.Domain.Entities;

namespace AirplaneBooking.Application.Interfaces.Repositories;

public interface IBookingRepository
{
    Task<Booking> GetByIdAsync(string id);
    Task AddAsync(Booking booking);
    Task UpdateAsync(Booking booking);
    Task DeleteAsync(string id);
}
