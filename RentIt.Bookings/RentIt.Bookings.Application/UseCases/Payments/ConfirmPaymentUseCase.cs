using RentIt.Bookings.Application.Exceptions;
using RentIt.Bookings.Application.Interfaces.Services;
using RentIt.Bookings.Application.Interfaces.UseCases.Payments;
using RentIt.Bookings.Core.Entities;
using RentIt.Bookings.Core.Enums;
using RentIt.Bookings.Core.Interfaces.Repositories;
using Serilog;

namespace RentIt.Bookings.Application.UseCases.Payments
{
    public class ConfirmPaymentUseCase : IConfirmPaymentUseCase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;
        private readonly IBookingNotificationService _bookingNotificationService;

        public ConfirmPaymentUseCase(
            IUnitOfWork unitOfWork,
            ILogger logger,
            IBookingNotificationService bookingNotificationService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _bookingNotificationService = bookingNotificationService; 
        }

        public async Task<Payment> ExecuteAsync(
            Guid bookingId,
            string userId,
            CancellationToken cancellationToken)
        {
            _logger.Information("Начало подтверждения платежа для бронирования с BookingId {BookingId}.", bookingId);

            var userIdParseAttempt = Guid.TryParse(userId, out var userGuid);

            if (!userIdParseAttempt)
            {
                _logger.Warning("Некорректный формат ID пользователя.");

                throw new ArgumentException("Некорректный формат ID пользователя.");
            }

            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId, cancellationToken);

            if (booking == null)
            {
                _logger.Warning("Бронирование с ID {BookingId} не найдено.", bookingId);

                throw new NotFoundException("Бронирование не найдено.");
            }

            if (booking.UserId != userGuid)
            {
                _logger.Warning("Пользователь {IncrctUserId} попытался оплатить бронирование пользователя {CrctUserId}.", userGuid, booking.UserId);

                throw new ArgumentException("Нельзя оплатить бронирование другого человека.");
            }

            var payment = await _unitOfWork.Payments.GetByIdAsync(booking.Payment.PaymentId, cancellationToken);

            if (payment == null)
            {
                _logger.Warning("Платеж с ID {PaymentId} не найден.", booking.Payment.PaymentId);

                throw new Exception("Платеж не найден.");
            }

            if (booking.Status == BookingStatus.Cancelled)
            {
                _logger.Warning("Пользователь пытается оплатить отмененное бронирование.");

                throw new ArgumentException("Извините, но это бронирование отменено. Вероятно, вы не оплатили его вовремя. Проверьте свой электронный ящик.");
            }

            payment.Status = PaymentStatus.Completed;
            payment.PaymentTime = DateTime.UtcNow;

            booking.Status = BookingStatus.Paid;

            _unitOfWork.Payments.Update(payment);
            _unitOfWork.Bookings.Update(booking);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _bookingNotificationService.NotifyUserAboutPaymentSuccessAsync(booking, payment, cancellationToken);

            _logger.Information("Платеж с ID {PaymentId} подтвержден.", booking.Payment.PaymentId);

            return payment;
        }
    }
}