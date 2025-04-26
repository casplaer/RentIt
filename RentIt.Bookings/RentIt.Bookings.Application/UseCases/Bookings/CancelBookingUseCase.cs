using Hangfire;
using RentIt.Bookings.Application.Exceptions;
using RentIt.Bookings.Application.Interfaces.EventBus;
using RentIt.Bookings.Application.Interfaces.Services;
using RentIt.Bookings.Application.Interfaces.UseCases.Bookings;
using RentIt.Bookings.Application.Interfaces.UseCases.Payments;
using RentIt.Bookings.Core.Enums;
using RentIt.Bookings.Core.Interfaces.Repositories;
using RentIt.MessageBroker.Contracts.Events;
using Serilog;

namespace RentIt.Bookings.Application.UseCases.Bookings
{
    public class CancelBookingUseCase : ICancelBookingUseCase
    {
        private readonly IEnumerable<BookingStatus> _allowedToCancelStatuses;

        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventBus _eventBus;
        private readonly IRefundPaymentUseCase _refundPaymentUseCase;
        private readonly IBookingNotificationService _bookingNotificationService;

        public CancelBookingUseCase(
            ILogger logger, 
            IUnitOfWork unitOfWork,
            IEventBus eventBus,
            IRefundPaymentUseCase refundPaymentUseCase,
            IBookingNotificationService bookingNotificationService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _eventBus = eventBus;
            _refundPaymentUseCase = refundPaymentUseCase;
            _bookingNotificationService = bookingNotificationService;

            _allowedToCancelStatuses =
            [
                BookingStatus.Pending,
                BookingStatus.Confirmed,
                BookingStatus.Paid
            ];
        }

        public async Task ExecuteAsync(
            Guid bookingId, 
            string userId, 
            CancellationToken cancellationToken)
        {
            _logger.Information("Начало обработки запроса на отмену бронирования {BookingId}.", bookingId);

            var userIdParseAttempt = Guid.TryParse(userId, out var userGuid);

            if (!userIdParseAttempt)
            {
                _logger.Warning("Некорректный формат ID пользователя.");

                throw new ArgumentException("Некорректный формат ID пользователя.");
            }

            var bookingToCancel = await _unitOfWork.Bookings.GetByIdAsync(bookingId, cancellationToken);

            if (bookingToCancel == null)
            {
                _logger.Warning("Бронирование с ID {BookingId} не было найдено.", bookingId);

                throw new NotFoundException("Бронирование с таким ID не найдено.");
            }

            if (bookingToCancel.UserId != userGuid)
            {
                _logger.Warning("Произошла попытка неавторизованного доступа пользователя {UserId} к бронированию {BookingId}.", userId, bookingId);

                throw new UnauthorizedAccessException("Попытка неавторизованного доступа.");
            }

            if (!_allowedToCancelStatuses.Contains(bookingToCancel.Status))
            {
                _logger.Warning($"Пользователь попытался отменить бронирование с некорректным статусом. Текущий статус: {bookingToCancel.Status}");

                throw new ArgumentException($"Можно отменить бронирование только в статусе \"Обрабатывается\", \"Подтверждено\" или \"Оплачено\". Текущий статус: {bookingToCancel.Status}.");
            }

            if ((bookingToCancel.StartDate - DateTime.UtcNow).TotalHours <= 24)
            {
                _logger.Warning("Пользователь попытался отменить бронирование, которое начинается ранее чем через 24 часа от текущего момента.");

                throw new ArgumentException("Минимальное время для отмены бронирования состовляет 24 часа до его начала. " +
                    "Для его отмены и возврата средств обратитесь в техническую поддержку.");
            }

            _logger.Information("Находим предущую цепочу бронирований для обновления информации в объявлении. (Если такая имеется)");

            var (StartDate, EndDate) = await _unitOfWork.Bookings.GetCurrentBookingChainAsync(
                                                            bookingToCancel,
                                                            cancellationToken);

            _logger.Information("Возврат денег клиенту, если бронирование уже оплачено.");

            if (bookingToCancel.Status == BookingStatus.Paid)
            {
                await _refundPaymentUseCase.ExecuteAsync(bookingToCancel.Payment.PaymentId, false, cancellationToken);
            }

            bookingToCancel.Status = BookingStatus.Cancelled;

            _logger.Information("Статус бронирования успешно изменен на Cancelled.");

            _unitOfWork.Bookings.Update(bookingToCancel);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.Information("Изменения успешно сохранены.");

            await _eventBus.PublishAsync(
                new BookingUpdatedEvent
                {
                    HousingId = bookingToCancel.HousingId,
                    NewStartDate = StartDate,
                    NewEndDate = EndDate,
                }, cancellationToken);

            BackgroundJob.Enqueue(() => 
                _bookingNotificationService.NotifyUserAboutBookingCancellationAsync(bookingToCancel, cancellationToken));
        }
    }
}