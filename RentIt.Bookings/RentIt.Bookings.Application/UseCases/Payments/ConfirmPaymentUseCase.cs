using RentIt.Bookings.Application.Exceptions;
using RentIt.Bookings.Application.Interfaces.Services;
using RentIt.Bookings.Application.Interfaces.UseCases.Payments;
using RentIt.Bookings.Application.Services;
using RentIt.Bookings.Application.Services.Grpc;
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
        private readonly BookingNotificationService _bookingNotificationService;

        public ConfirmPaymentUseCase(
            IUnitOfWork unitOfWork,
            ILogger logger,
            BookingNotificationService bookingNotificationService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _bookingNotificationService = bookingNotificationService; 
        }

        public async Task<Payment> ExecuteAsync(Guid paymentId, CancellationToken cancellationToken)
        {
            _logger.Information("Начало подтверждения платежа. PaymentId: {PaymentId}", paymentId);

            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId, cancellationToken);

            if (payment == null)
            {
                _logger.Warning("Платеж с ID {PaymentId} не найден.", paymentId);

                throw new Exception("Платеж не найден.");
            }

            var booking = await _unitOfWork.Bookings.GetByIdAsync(payment.BookingId, cancellationToken);
            if (booking == null)
            {
                _logger.Warning("Бронирование для платежа {PaymentId} не найдено.", paymentId);

                throw new NotFoundException("Бронирование не найдено.");
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

            _logger.Information("Платеж с ID {PaymentId} подтвержден.", paymentId);

            return payment;
        }
    }
}