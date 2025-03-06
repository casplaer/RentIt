using FluentValidation;
using RentIt.Users.Application.Commands.Users.Update;
using System.Text.RegularExpressions;

namespace RentIt.Users.Application.Validators
{
    public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserCommandValidator()
        {
            RuleFor(u => u)
                .NotNull()
                .WithMessage("Отсутствуют данные для обновления.");

            RuleFor(x => x.FirstName)
           .NotEmpty().WithMessage("Имя обязательно.")
           .Must(name => string.IsNullOrEmpty(name) || !Regex.IsMatch(name, @"\d"))
               .WithMessage("Имя не должно содержать цифр.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Фамилия обязательна.")
                .Must(lastName => string.IsNullOrEmpty(lastName) || !Regex.IsMatch(lastName, @"\d"))
                    .WithMessage("Фамилия не должна содержать цифр.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email обязателен.")
                .EmailAddress().WithMessage("Неверный формат email.");

            RuleFor(x => x.PhoneNumber)
                .Must(phone => string.IsNullOrEmpty(phone) || !Regex.IsMatch(phone, "[A-Za-z]"))
                .WithMessage("Номер телефона не должен содержать букв.");

            RuleFor(x => x.Country)
                .NotEmpty().WithMessage("Страна обязательна.")
                .Must(country => string.IsNullOrEmpty(country) || !Regex.IsMatch(country, @"\d"))
                .WithMessage("Страна не должна содержать цифр.");

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("Город обязателен.")
                .Must(city => string.IsNullOrEmpty(city) || !Regex.IsMatch(city, @"\d"))
                .WithMessage("Город не должен содержать цифр.");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Адрес обязателен.");
        }
    }
}
