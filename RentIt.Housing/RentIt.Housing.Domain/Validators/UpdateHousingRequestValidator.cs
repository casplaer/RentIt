using FluentValidation;
using RentIt.Housing.Domain.Contracts.Requests.Housing;

namespace RentIt.Housing.Domain.Validators
{
    public class UpdateHousingRequestValidator : AbstractValidator<UpdateHousingRequest>
    {
        public UpdateHousingRequestValidator()
        {
            RuleFor(x => x.Title)
                .MaximumLength(100)
                .When(x => !string.IsNullOrEmpty(x.Title))
                .WithMessage("Название не может превышать 100 символов.");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .When(x => !string.IsNullOrEmpty(x.Description))
                .WithMessage("Описание не может превышать 500 символов.");

            RuleFor(x => x.Country)
                .NotEmpty().When(x => x.Country != null)
                .WithMessage("Страна не должна быть пустой.")
                .MaximumLength(100)
                .WithMessage("Страна не может превышать 100 символов.");

            RuleFor(x => x.City)
                .MaximumLength(100)
                .When(x => !string.IsNullOrEmpty(x.City))
                .WithMessage("Город не может превышать 100 символов.");

            RuleFor(x => x.Address)
                .MaximumLength(200)
                .When(x => !string.IsNullOrEmpty(x.Address))
                .WithMessage("Адрес не может превышать 200 символов.");

            RuleFor(x => x.PricePerNight)
                .GreaterThan(0)
                .When(x => x.PricePerNight.HasValue)
                .WithMessage("Цена за ночь должна быть больше 0.");

            RuleFor(x => x.NumberOfRooms)
                .GreaterThan(0)
                .When(x => x.NumberOfRooms.HasValue)
                .WithMessage("Количество комнат должно быть больше 0.");

            RuleFor(x => x.Amenities)
                .NotEmpty()
                .NotNull()
                .WithMessage("Список удобств не может быть пустым.");
        }
    }
}
