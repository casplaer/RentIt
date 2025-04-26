using RentIt.Bookings.Application.Interfaces.UseCases.Bookings;
using RentIt.Bookings.Core.Interfaces.Repositories;
using RentIt.Bookings.Application.Exceptions;
using Serilog;
using RentIt.Bookings.Core.Enums;
using RentIt.Bookings.Application.Interfaces.EventBus;
using RentIt.MessageBroker.Contracts.Events;
using RentIt.Bookings.Application.Interfaces.UseCases.Payments;
using RentIt.Bookings.Application.Interfaces.Services;
using Hangfire;

namespace RentIt.Bookings.Application.UseCases.Bookings
{
    public class AdminCancelBookingUseCase : IAdminCancelBookingUseCase
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventBus _eventBus;
        private readonly IRefundPaymentUseCase _refundPaymentUseCase;
        private readonly IBookingNotificationService _bookingNotificationService;


        public AdminCancelBookingUseCase(
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

            if (bookingToCancel.Payment != null && bookingToCancel.Payment.Status == PaymentStatus.Completed)
            {
                _logger.Information("Возврат денег клиенту, если бронирование уже было оплачено.");

                var payment = await _unitOfWork.Payments.GetByIdAsync(bookingToCancel.Payment.PaymentId, cancellationToken);
                if (payment == null || payment.Status != PaymentStatus.Completed)
                {
                    _logger.Warning("Платеж с ID {PaymentId} не найден или не был завершен.", bookingToCancel.Payment.PaymentId);
                    throw new Exception("Платеж не найден.");
                }

                decimal refundAmount = payment.Amount;
                int finePercent = 0;

                if (isFined)
                {
                    var now = DateTime.UtcNow;
                    var totalDays = (bookingToCancel.EndDate.Date - bookingToCancel.StartDate.Date).TotalDays;
                    var remainingDays = (bookingToCancel.EndDate.Date - now.Date).TotalDays;

                    if (remainingDays <= 0)
                    {
                        finePercent = 50;
                    }
                    else
                    {
                        finePercent = (int)Math.Round(Math.Min((remainingDays / totalDays) * 100, 50));
                    }

                    var fineAmount = payment.Amount * finePercent / 100m;
                    refundAmount -= fineAmount;

                    _logger.Information("Пользователь оштрафован на {FinePercent}%, сумма штрафа {FineAmount}, сумма возврата {RefundAmount}",
                        finePercent, fineAmount, refundAmount);
                }

                payment.Status = PaymentStatus.Refunded;

                _unitOfWork.Payments.Update(payment);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.Information("Платеж с ID {PaymentId} отмечен как возвращенный.", payment.PaymentId);

                BackgroundJob.Enqueue(() =>
                    _bookingNotificationService.NotifyUserAboutRefundAsync(
                        bookingToCancel, payment, isFined, finePercent, refundAmount, CancellationToken.None));
            }

            _logger.Information("Находим следующую цепочку бронирований для обновления информации в объявлении. (Если таковая имеется)");

            var (StartDate, EndDate) = await _unitOfWork.Bookings.GetCurrentBookingChainAsync(
                bookingToCancel,
                cancellationToken);

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
                _bookingNotificationService.NotifyUserAboutBookingCancellationAsync(bookingToCancel, CancellationToken.None));
        }
    }
}