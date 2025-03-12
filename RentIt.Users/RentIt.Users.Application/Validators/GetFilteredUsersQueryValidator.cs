using FluentValidation;
using RentIt.Users.Application.Queries.Users;
using System.Text.RegularExpressions;

namespace RentIt.Users.Application.Validators
{
    public class GetFilteredUsersQueryValidator : AbstractValidator<GetFilteredUsersQuery>
    {
        public GetFilteredUsersQueryValidator()
        {
            RuleFor(u => u)
                .NotNull()
                .WithMessage("Отсутствуют данные для фильтрации.");

            RuleFor(u => u.FirstName)
                .Must(name => string.IsNullOrEmpty(name) || !Regex.IsMatch(name, @"\d"))
                .WithMessage("Имя не должно содержать цифр.");

            RuleFor(u => u.LastName)
                .Must(name => string.IsNullOrEmpty(name) || !Regex.IsMatch(name, @"\d"))
                .WithMessage("Фамилия не должна содержать цифр.");

            RuleFor(u => u.City)
                .Must(city => string.IsNullOrEmpty(city) || !Regex.IsMatch(city, @"\d"))
                .WithMessage("Город не должен содержать цифр.");

            RuleFor(u => u.Country)
                .Must(country => string.IsNullOrEmpty(country) || !Regex.IsMatch(country, @"\d"))
                .WithMessage("Страна не должна содержать цифр.");

            RuleFor(u => u.Page)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Номер страницы должен быть больше или равен 1.");

            RuleFor(u => u.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("Размер страницы должен быть между 1 и 100.");

            RuleFor(u => u.PhoneNumber)
                .Must(phone => string.IsNullOrEmpty(phone) || !Regex.IsMatch(phone, "[A-Za-z]"))
                .WithMessage("Номер телефона не должен содержать букв.");

            RuleFor(u => u.Email)
                .EmailAddress()
                .When(u => !string.IsNullOrWhiteSpace(u.Email))
                .WithMessage("Неверный формат электронной почты.");
        }
    }
}
