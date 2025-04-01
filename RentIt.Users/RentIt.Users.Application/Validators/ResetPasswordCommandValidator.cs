using FluentValidation;
using RentIt.Users.Application.Commands.Users.Password;

namespace RentIt.Users.Application.Validators
{
    public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordCommandValidator()
        {
            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("Пароль обязателен.")
                .MinimumLength(8).WithMessage("Пароль должен содержать минимум 8 символов.")
                .Equal(x => x.ConfirmPassword).WithMessage("Пароли не совпадают.")
                .Matches(@"^[a-zA-Z]+$").WithMessage("Пароль должен содержать только латиницу.")
                .Matches(@"(?=.*[a-z])").WithMessage("Пароль должен содержать хотя бы одну строчную латинскую букву.")
                .Matches(@"(?=.*[A-Z])").WithMessage("Пароль должен содержать хотя бы одну заглавную латинскую букву.");
        }
    }
}
