namespace RentIt.Users.Contracts.Requests.Users
{
    public record GetUsersRequest(
        string? FirstName,
        string? LastName,
        string? Email,
        string? Country,
        string? City,
        string? PhoneNumber,
        int Page,
        int PageSize
        );
}
