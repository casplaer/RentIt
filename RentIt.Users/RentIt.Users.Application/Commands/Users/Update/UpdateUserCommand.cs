using MediatR;

namespace RentIt.Users.Application.Commands.Users.Update
{
    public record UpdateUserCommand(
        Guid UserId,
        string FirstName,
        string LastName,
        string Email,
        string PhoneNumber,
        string Country,
        string City,
        string Address
        ) : IRequest<bool>;
}
