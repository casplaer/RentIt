using RentIt.Housing.Domain.Contracts.Requests.Housing;

namespace RentIt.Housing.Domain.Validators
{
    using FluentValidation;
    using System.Text.RegularExpressions;

    public class CreateHousingRequestValidator : AbstractValidator<CreateHousingRequest>
    {
        public CreateHousingRequestValidator()
        {
            RuleFor(x => x)
                .NotNull()
                .WithMessage("Запрос не должен быть пустым.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Название обязательно.")
                .MinimumLength(5).WithMessage("Название должно содержать не менее 5 символов.")
                .MaximumLength(100).WithMessage("Название должно содержать не более 100 символов.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Описание обязательно.")
                .MinimumLength(10).WithMessage("Описание должно содержать не менее 10 символов.")
                .MaximumLength(500).WithMessage("Описание должно содержать не более 500 символов.");

            RuleFor(x => x.Country)
                .NotEmpty().WithMessage("Страна обязательна.")
                .Must(country => !Regex.IsMatch(country, @"\d"))
                    .WithMessage("Название страны не должно содержать цифр.");

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("Город обязателен.");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Адрес обязателен.");

            RuleFor(x => x.PricePerNight)
                .GreaterThan(0).WithMessage("Цена за ночь должна быть больше 0.");

            RuleFor(x => x.NumberOfRooms)
                .GreaterThan(0).WithMessage("Количество комнат должно быть больше 0.");

            RuleFor(x => x.Amenities)
                .NotNull().WithMessage("Список удобств не должен быть пустым.");
        }
    }
}