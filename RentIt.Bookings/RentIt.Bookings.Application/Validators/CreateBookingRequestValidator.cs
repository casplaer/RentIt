using FluentValidation;
using RentIt.Bookings.Contracts.Requests.Bookings;
using RentIt.Bookings.Core.Interfaces.Repositories;

namespace RentIt.Bookings.Application.Validators
{
    public class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
    {
        public CreateBookingRequestValidator(IUnitOfWork unitOfWork)
        {
            RuleFor(x => x.StartDate)
                .GreaterThan(DateTime.UtcNow)
                .WithMessage("Начальная дата не может раньше текущего дня.");

            RuleFor(x => x.StartDate)
                .LessThan(x => x.EndDate)
                .WithMessage("Начальная дата должна быть раньше конечной даты.");

            RuleFor(x => x.StartDate)
                .GreaterThan(DateTime.UtcNow + TimeSpan.FromHours(24))
                .WithMessage("Нельзя забронировать собственность менее чем за 24 часа до начала.");
        }
    }
}