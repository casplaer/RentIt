using Hangfire;
using RentIt.Bookings.Application.Interfaces.EventBus;
using RentIt.Bookings.Application.Interfaces.Services;
using RentIt.Bookings.Application.Specifications.Bookings;
using RentIt.Bookings.Core.Enums;
using RentIt.Bookings.Core.Interfaces.Repositories;
using RentIt.MessageBroker.Contracts.Events;

namespace RentIt.Bookings.Application.Services
{

    public class BookingStatusService : IBookingStatusService
    {
        private const int hoursToPayBeforeCancellation = 12;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventBus _eventBus;
        private readonly IBookingNotificationService _bookingNotificationService;

        public BookingStatusService(
            IUnitOfWork unitOfWork,
            IEventBus eventBus,
            IBookingNotificationService bookingNotificationService)
        {
            _unitOfWork = unitOfWork;
            _eventBus = eventBus;
            _bookingNotificationService = bookingNotificationService;
        }

        public async Task UpdateActiveBookingsAsync(CancellationToken cancellationToken)
        {
            var specification = new SearchBookingSpecification(endDate: DateTime.UtcNow.Date, status: BookingStatus.Active);

            var bookingsToUpdate = await _unitOfWork.Bookings.GetAllFilteredBookingsAsync(specification, cancellationToken);

            foreach (var booking in bookingsToUpdate)
            {
                booking.Status = BookingStatus.Completed;

                BackgroundJob.Enqueue(() =>
                    _bookingNotificationService.NotifyUserAboutBookingCompletionAsync(booking, cancellationToken));

                _unitOfWork.Bookings.Update(booking);

                var (StartDate, EndDate) = await _unitOfWork.Bookings.GetCurrentBookingChainAsync(
                        booking,
                        cancellationToken);

                var completedEvent = new BookingUpdatedEvent
                {
                    HousingId = booking.HousingId,
                    NewStartDate = StartDate,
                    NewEndDate = EndDate,
                };
                await _eventBus.PublishAsync(completedEvent, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateConfirmedBookingsAsync(CancellationToken cancellationToken)
        {
            var specification = new SearchBookingSpecification(status: BookingStatus.Confirmed);

            var bookingsToUpdate = await _unitOfWork.Bookings.GetAllFilteredBookingsAsync(specification, cancellationToken);

            foreach (var booking in bookingsToUpdate)
            {
                if ((booking.StartDate - DateTime.UtcNow).TotalHours <= hoursToPayBeforeCancellation)
                {
                    booking.Status = BookingStatus.Cancelled;

                    BackgroundJob.Enqueue(() =>
                        _bookingNotificationService.NotifyUserAboutBookingCancellationDueToNonPaymentAsync(booking, cancellationToken));

                    _unitOfWork.Bookings.Update(booking);

                    var (StartDate, EndDate) = await _unitOfWork.Bookings.GetCurrentBookingChainAsync(
                                                booking,
                                                cancellationToken);

                    var cancelledEvent = new BookingUpdatedEvent
                    {
                        HousingId = booking.HousingId,
                        NewStartDate = StartDate,
                        NewEndDate = EndDate,
                    };

                    await _eventBus.PublishAsync(cancelledEvent, cancellationToken);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdatePaidBookingsAsync(CancellationToken cancellationToken)
        {
            var specification = new SearchBookingSpecification(startDate: DateTime.UtcNow.Date, status: BookingStatus.Paid);

            var bookingsToUpdate = await _unitOfWork.Bookings.GetAllFilteredBookingsAsync(specification, cancellationToken);

            foreach (var booking in bookingsToUpdate)
            {
                booking.Status = BookingStatus.Active;

                _unitOfWork.Bookings.Update(booking);

                var activatedEvent = new BookingUpdatedEvent
                {
                    HousingId = booking.HousingId,
                    NewStartDate = booking.StartDate,
                    NewEndDate = booking.EndDate,
                    BookingStatus = booking.Status.ToString()
                };
                await _eventBus.PublishAsync(activatedEvent, cancellationToken);
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

                    BackgroundJob.Enqueue(() =>
                        _bookingNotificationService.NotifyUserAboutBookingCancellationDueToNonConfirmationAsync(booking, isCreatedMoreThan48HoursAgo, cancellationToken));
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}