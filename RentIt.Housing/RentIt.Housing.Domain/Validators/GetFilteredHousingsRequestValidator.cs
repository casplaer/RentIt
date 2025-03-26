using FluentValidation;
using RentIt.Housing.Domain.Contracts.Requests.Housing;

namespace RentIt.Housing.Domain.Validators
{
    public class GetFilteredHousingsRequestValidator : AbstractValidator<GetFilteredHousingsRequest>
    {
        public GetFilteredHousingsRequestValidator()
        {
            RuleFor(x => x)
                .NotNull()
                .WithMessage("Запрос не может быть пустым.");

            RuleFor(x => x.Page)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Номер страницы должен быть больше или равен 1.");

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .WithMessage("Размер страницы должен быть больше 0.")
                .LessThanOrEqualTo(100)
                .WithMessage("Размер страницы не может превышать 100.");

            RuleFor(x => x.Title)
                .MaximumLength(100)
                .When(x => !string.IsNullOrEmpty(x.Title))
                .WithMessage("Название не может превышать 100 символов.");

            RuleFor(x => x.Address)
                .MaximumLength(200)
                .When(x => !string.IsNullOrEmpty(x.Address))
                .WithMessage("Адрес не может превышать 200 символов.");

            RuleFor(x => x.City)
                .MaximumLength(100)
                .When(x => !string.IsNullOrEmpty(x.City))
                .WithMessage("Город не может превышать 100 символов.");

            RuleFor(x => x.Country)
                .MaximumLength(100)
                .When(x => !string.IsNullOrEmpty(x.Country))
                .WithMessage("Страна не может превышать 100 символов.");                

            RuleFor(x => x.PricePerNight)
                .GreaterThan(0).When(x => x.PricePerNight.HasValue)
                .WithMessage("Цена за ночь должна быть больше 0.");

            RuleFor(x => x.NumberOfRooms)
                .GreaterThan(0).When(x => x.NumberOfRooms.HasValue)
                .WithMessage("Количество комнат должно быть больше 0.");

            RuleFor(x => x.Rating)
                .InclusiveBetween(0.0, 5.0).When(x => x.Rating.HasValue)
                .WithMessage("Рейтинг должен быть в диапазоне от 0 до 5.");
        }
    }
}
