using MediatR;

namespace RentIt.Users.Application.Commands.Users.Password
{
    public record ForgotPasswordCommand(
        string Email
        ): IRequest<string>;
}
