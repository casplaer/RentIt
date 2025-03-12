namespace RentIt.Users.Contracts.Requests.Users
{
    public record UpdateUserRequest(
        string FirstName,
        string LastName,
        string Email,
        string PhoneNumber,
        string Country,
        string City,
        string Address
        );
}
