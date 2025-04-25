using FluentValidation;
using RentIt.Bookings.Contracts.Requests.Bookings;
using RentIt.Bookings.Core.Enums;

namespace RentIt.Bookings.Application.Validators
{
    public class UpdateBookingRequestValidator : AbstractValidator<UpdateBookingRequest>
    {
        public UpdateBookingRequestValidator()
        {
            RuleFor(x => x.HousingId).NotEmpty().WithMessage("HousingId не может быть пустым.");

            RuleFor(x => x.StartDate)
                .LessThan(x => x.EndDate)
                .WithMessage("Начальная дата должна быть раньше конечной даты.");
            
            RuleFor(x => x.Status)
                .Must(status => Enum.TryParse<BookingStatus>(status, true, out _))
                .WithMessage("Неверный статус бронирования.");
        }
    }
}
