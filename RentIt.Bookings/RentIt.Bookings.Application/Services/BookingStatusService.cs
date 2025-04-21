using RentIt.Bookings.Application.Interfaces.EventBus;
using RentIt.Bookings.Application.Interfaces.Services;
using RentIt.Bookings.Application.Specifications.Bookings;
using RentIt.Bookings.Core.Enums;
using RentIt.Bookings.Core.Interfaces.Repositories;
using RentIt.MessageBroker.Contracts.Events;
using Serilog;

namespace RentIt.Bookings.Application.Services
{

    public class BookingStatusService : IBookingStatusService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventBus _eventBus;
        private readonly BookingNotificationService _bookingNotificationService;

        public BookingStatusService(
            IUnitOfWork unitOfWork, 
            IEventBus eventBus,
            BookingNotificationService bookingNotificationService)
        {
            _unitOfWork = unitOfWork;
            _eventBus = eventBus;
            _bookingNotificationService = bookingNotificationService;
        }

        public async Task UpdateActiveBookingsAsync(CancellationToken cancellationToken)
        {
            var specification = new SearchBookingSpecification(status: BookingStatus.Active);

            var bookingsToUpdate = await _unitOfWork.Bookings.GetAllFilteredBookingsAsync(specification, cancellationToken);

            foreach (var booking in bookingsToUpdate)
            {
                if (booking.EndDate <= DateTime.UtcNow)
                {
                    booking.Status = BookingStatus.Completed;

                    await _bookingNotificationService.NotifyUserAboutBookingCompletionAsync(booking, cancellationToken);

                    _unitOfWork.Bookings.Update(booking);

                    var completedEvent = new BookingCompletedEvent
                    {
                        HousingId = booking.HousingId,
                        StartDate = booking.StartDate,
                        EndDate = booking.EndDate,
                        NextEstimatedStartDate = null,
                        NextEstimatedEndDate = null 
                    };
                    await _eventBus.PublishAsync(completedEvent, cancellationToken);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateConfirmedBookingsAsync(CancellationToken cancellationToken)
        {
            var specification = new SearchBookingSpecification(status: BookingStatus.Confirmed);

            var bookingsToUpdate = await _unitOfWork.Bookings.GetAllFilteredBookingsAsync(specification, cancellationToken);

            foreach (var booking in bookingsToUpdate)
            {
                if ((booking.StartDate - DateTime.UtcNow).TotalHours <= 12)
                {
                    booking.Status = BookingStatus.Cancelled;

                    await _bookingNotificationService.NotifyUserAboutBookingCancellationDueToNonPaymentAsync(booking, cancellationToken);

                    _unitOfWork.Bookings.Update(booking);

                    var cancelledEvent = new BookingCancelledEvent
                    {
                        HousingId = booking.HousingId,
                        StartDate = booking.StartDate,
                        NextEstimatedStartDate = null,
                        NextEstimatedEndDate = null
                    };
                    await _eventBus.PublishAsync(cancelledEvent, cancellationToken);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdatePaidBookingsAsync(CancellationToken cancellationToken)
        {
            var specification = new SearchBookingSpecification(status: BookingStatus.Paid);

            var bookingsToUpdate = await _unitOfWork.Bookings.GetAllFilteredBookingsAsync(specification, cancellationToken);

            foreach (var booking in bookingsToUpdate)
            {
                if (booking.StartDate.Date == DateTime.UtcNow.Date)
                {
                    booking.Status = BookingStatus.Active;

                    _unitOfWork.Bookings.Update(booking);

                    var activatedEvent = new BookingActivatedEvent
                    {
                        HousingId = booking.HousingId,
                        StartDate = booking.StartDate,
                        EndDate = booking.EndDate
                    };
                    await _eventBus.PublishAsync(activatedEvent, cancellationToken);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdatePendingBookingsAsync(CancellationToken cancellationToken)
        {
            var specification = new SearchBookingSpecification(status: BookingStatus.Pending);

            var bookingsToUpdate = await _unitOfWork.Bookings.GetAllFilteredBookingsAsync(specification, cancellationToken);

            foreach (var booking in bookingsToUpdate)
            {
                var isCreatedMoreThan48HoursAgo = (DateTime.UtcNow - booking.CreatedAt).TotalHours > 48;

                var isLessThan24HoursToStart = (booking.StartDate - DateTime.UtcNow).TotalHours < 24;

                if (isCreatedMoreThan48HoursAgo || isLessThan24HoursToStart)
                {
                    booking.Status = BookingStatus.Cancelled;

                    _unitOfWork.Bookings.Update(booking);

                    await _bookingNotificationService.NotifyUserAboutBookingCancellationDueToNonConfirmationAsync(booking, isCreatedMoreThan48HoursAgo, cancellationToken);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}