using FluentValidation;
using RentIt.Housing.Domain.Contracts.Requests.Availabilities;

namespace RentIt.Housing.Domain.Validators
{
    public class UpdateAvailabilitiesRequestValidator : AbstractValidator<UpdateAvailabilitiesRequest>
    {
        public UpdateAvailabilitiesRequestValidator()
        {
            RuleFor(x => x)
                .NotNull()
                .WithMessage("Запрос не должен быть пустым.");

            RuleForEach(x => x.AvailabilityDtos)
                .ChildRules(avail =>
                {
                    avail.RuleFor(a => a.StartDate)
                        .GreaterThan(DateOnly.FromDateTime(DateTime.UtcNow))
                        .WithMessage("Начальная дата должна быть больше текущей даты.");

                    avail.RuleFor(a => a.StartDate)
                        .LessThan(a => a.EndDate)
                        .WithMessage("Начальная дата должна быть меньше конечной даты.");
                });
        }
    }
}
