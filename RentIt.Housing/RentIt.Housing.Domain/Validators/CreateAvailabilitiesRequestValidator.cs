using FluentValidation;
using RentIt.Housing.Domain.Contracts.Requests.Availabilities;

namespace RentIt.Housing.Domain.Validators
{
    public class CreateAvailabilitiesRequestValidator : AbstractValidator<CreateAvailabilitiesRequest>
    {
        public CreateAvailabilitiesRequestValidator()
        {
            RuleFor(x => x)
                .NotNull()
                .WithMessage("Запрос не должен быть пустым.");

            RuleFor(x => x.AvailabilityDtos)
                .NotEmpty()
                .WithMessage("Список доступных дней не может быть пустым.");

            RuleForEach(x => x.AvailabilityDtos)
                .ChildRules(avail =>
                {
                    avail.RuleFor(a => a.StartDate)
                        .LessThan(a => a.EndDate)
                        .WithMessage("Дата начала должна быть меньше даты окончания.");

                    avail.RuleFor(a => a.StartDate)
                        .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
                        .WithMessage("Дата начала не может быть раньше текущей даты.");
                });
        }
    }
}
