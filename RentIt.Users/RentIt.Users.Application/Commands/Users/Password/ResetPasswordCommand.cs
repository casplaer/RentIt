using MediatR;

namespace RentIt.Users.Application.Commands.Users.Password
{
    public class ResetPasswordCommand : IRequest<string>
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}