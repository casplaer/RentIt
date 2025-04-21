using RentIt.Bookings.Application.Interfaces.UseCases.Bookings;
using RentIt.Bookings.Core.Interfaces.Repositories;
using RentIt.Bookings.Application.Exceptions;
using Serilog;
using RentIt.Bookings.Core.Enums;
using RentIt.Bookings.Application.Interfaces.EventBus;
using RentIt.MessageBroker.Contracts.Events;
using RentIt.Bookings.Application.Interfaces.UseCases.Payments;
using RentIt.Bookings.Application.Services;

namespace RentIt.Bookings.Application.UseCases.Bookings
{
    public class AdminCancelBookingUseCase : IAdminCancelBookingUseCase
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventBus _eventBus;
        private readonly IRefundPaymentUseCase _refundPaymentUseCase;
        private readonly BookingNotificationService _bookingNotificationService;


        public AdminCancelBookingUseCase(
            ILogger logger,
            IUnitOfWork unitOfWork,
            IEventBus eventBus,
            IRefundPaymentUseCase refundPaymentUseCase,
            BookingNotificationService bookingNotificationService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _eventBus = eventBus;
            _refundPaymentUseCase = refundPaymentUseCase;
            _bookingNotificationService = bookingNotificationService;
        }

        public async Task ExecuteAsync(
            Guid bookingId, 
            bool isFined,
            CancellationToken cancellationToken)
        {
            _logger.Information("Начало отмены бронирования с ID {BookingId} администратором.", bookingId);

            var bookingToCancel = await _unitOfWork.Bookings.GetByIdAsync(bookingId, cancellationToken);

            if (bookingToCancel == null)
            {
                _logger.Warning("Бронирование с ID {BookingID} не найдено.", bookingId);

                throw new NotFoundException("Бронирование с таким ID не найдено.");
            }

            if (bookingToCancel.Payment != null)
            {
                _logger.Information("Возврат денег клиенту, если бронирование уже было оплачено.");

                await _refundPaymentUseCase.ExecuteAsync(bookingToCancel.Payment.PaymentId, isFined, cancellationToken);
            }

            DateTime? nextEstimatedStartDate = null;
            DateTime? nextEstimatedEndDate = null;

            _logger.Information("Находим следующее бронирование для обновления информации в объявлении. (Если такое имеется)");

            var nextBooking = await _unitOfWork.Bookings.GetNextBookingByEndDate(
                                                            bookingToCancel.HousingId,
                                                            bookingToCancel.EndDate,
                                                            cancellationToken);

            if (nextBooking != null)
            {
                nextEstimatedStartDate = nextBooking.StartDate;
                nextEstimatedEndDate = nextBooking.EndDate;
            }

            bookingToCancel.Status = BookingStatus.Cancelled;

            await _eventBus.PublishAsync(
                new BookingCancelledEvent
                {
                    HousingId = bookingToCancel.HousingId,
                    StartDate = bookingToCancel.StartDate,
                    NextEstimatedStartDate = nextEstimatedStartDate,
                    NextEstimatedEndDate = nextEstimatedEndDate,
                }, cancellationToken);

            _logger.Information("Статус бронирования успешно изменен на Cancelled.");

            _unitOfWork.Bookings.Update(bookingToCancel);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.Information("Изменения успешно сохранены.");

            await _bookingNotificationService.NotifyUserAboutBookingCancellationAsync(bookingToCancel, cancellationToken);
        }
    }
}
