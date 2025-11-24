
namespace AirplaneBooking.Domain.Entities;

public class Passenger
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public DateTime DateOfBirth { get; private set; }
    public string Email { get; private set; }
    public string PhoneNumber { get; private set; }

    public Passenger(string firstName, string lastName, DateTime dateOfBirth, string email, string phoneNumber)
    {
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        Email = email;
        PhoneNumber = phoneNumber;
    }
}
