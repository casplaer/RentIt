namespace RentIt.Housing.Domain.Contracts.Dto.Users
{
    public record UserInfoDto(
        string FirstName, 
        string LastName, 
        string Email,
        string PhoneNumber);
}
