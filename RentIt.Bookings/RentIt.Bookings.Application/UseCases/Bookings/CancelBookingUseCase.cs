using Hangfire;
using RentIt.Bookings.Application.Exceptions;
using RentIt.Bookings.Application.Interfaces.EventBus;
using RentIt.Bookings.Application.Interfaces.Services;
using RentIt.Bookings.Application.Interfaces.UseCases.Bookings;
using RentIt.Bookings.Application.Interfaces.UseCases.Payments;
using RentIt.Bookings.Core.Enums;
using RentIt.Bookings.Core.Interfaces.Repositories;
using RentIt.MessageBroker.Contracts.Events;

namespace RentIt.Bookings.Application.UseCases.Bookings
{
    public class CancelBookingUseCase : ICancelBookingUseCase
    {
        private readonly IEnumerable<BookingStatus> _allowedToCancelStatuses;

        private readonly IAppLogger _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventBus _eventBus;
        private readonly IBookingNotificationService _bookingNotificationService;

        public CancelBookingUseCase(
            IAppLogger logger, 
            IUnitOfWork unitOfWork,
            IEventBus eventBus,
            IBookingNotificationService bookingNotificationService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _eventBus = eventBus;
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
            _logger.LogInformation("Начало обработки запроса на отмену бронирования {BookingId}.", bookingId);

            var userIdParseAttempt = Guid.TryParse(userId, out var userGuid);

            if (!userIdParseAttempt)
            {
                _logger.LogWarning("Некорректный формат ID пользователя.");

                throw new ArgumentException("Некорректный формат ID пользователя.");
            }

            var bookingToCancel = await _unitOfWork.Bookings.GetByIdAsync(bookingId, cancellationToken);

            if (bookingToCancel == null)
            {
                _logger.LogWarning("Бронирование с ID {BookingId} не было найдено.", bookingId);

                throw new NotFoundException("Бронирование с таким ID не найдено.");
            }

            if (bookingToCancel.UserId != userGuid)
            {
                _logger.LogWarning("Произошла попытка неавторизованного доступа пользователя {UserId} к бронированию {BookingId}.", userId, bookingId);

                throw new UnauthorizedAccessException("Попытка неавторизованного доступа.");
            }

            if (!_allowedToCancelStatuses.Contains(bookingToCancel.Status))
            {
                _logger.LogWarning($"Пользователь попытался отменить бронирование с некорректным статусом. Текущий статус: {bookingToCancel.Status}");

                throw new ArgumentException($"Можно отменить бронирование только в статусе \"Обрабатывается\", \"Подтверждено\" или \"Оплачено\". Текущий статус: {bookingToCancel.Status}.");
            }

            if ((bookingToCancel.StartDate - DateTime.UtcNow).TotalHours <= 24)
            {
                _logger.LogWarning("Пользователь попытался отменить бронирование, которое начинается ранее чем через 24 часа от текущего момента.");

                throw new ArgumentException("Минимальное время для отмены бронирования состовляет 24 часа до его начала. " +
                    "Для его отмены и возврата средств обратитесь в техническую поддержку.");
            }

            _logger.LogInformation("Находим предущую цепочу бронирований для обновления информации в объявлении. (Если такая имеется)");

            var (StartDate, EndDate) = await _unitOfWork.Bookings.GetCurrentBookingChainAsync(
                                                            bookingToCancel,
                                                            cancellationToken);

            _logger.LogInformation("Возврат денег клиенту, если бронирование уже оплачено.");

            if (bookingToCancel.Status == BookingStatus.Paid)
            {
                var payment = await _unitOfWork.Payments.GetByIdAsync(bookingToCancel.Payment.PaymentId, cancellationToken);
                if (payment == null || payment.Status != PaymentStatus.Completed)
                {
                    _logger.LogWarning("Платеж с ID {PaymentId} не найден или не был завершен.", bookingToCancel.Payment.PaymentId);
                    throw new Exception("Платеж не найден.");
                }

                payment.Status = PaymentStatus.Refunded;

                _unitOfWork.Payments.Update(payment);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Платеж с ID {PaymentId} отмечен как возвращенный.", bookingToCancel.Payment.PaymentId);

                BackgroundJob.Enqueue(() =>
                    _bookingNotificationService.NotifyUserAboutRefundAsync(
                        bookingToCancel,
                        payment,
                        false,
                        0,
                        payment.Amount,
                        CancellationToken.None));
            }


            bookingToCancel.Status = BookingStatus.Cancelled;

            _logger.LogInformation("Статус бронирования успешно изменен на Cancelled.");

            _unitOfWork.Bookings.Update(bookingToCancel);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Изменения успешно сохранены.");

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