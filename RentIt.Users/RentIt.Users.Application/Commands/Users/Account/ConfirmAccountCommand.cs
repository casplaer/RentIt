using MediatR;

namespace RentIt.Users.Application.Commands.Users.Account
{
    public record ConfirmAccountCommand(Guid UserId, string Token) : IRequest<bool>;
}
