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
    public class RefundPaymentUseCase : IRefundPaymentUseCase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;
        private readonly BookingNotificationService _bookingNotificationService;

        public RefundPaymentUseCase(
            IUnitOfWork unitOfWork,
            ILogger logger,
            BookingNotificationService bookingNotificationService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _bookingNotificationService = bookingNotificationService;
        }

        public async Task<Payment> ExecuteAsync(
            Guid paymentId,
            bool isFined,
            CancellationToken cancellationToken)
        {
            _logger.Information("Начало возврата средств для платежа. PaymentId: {PaymentId}", paymentId);

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

            decimal refundAmount = payment.Amount;
            int finePercent = 0;

            if (isFined)
            {
                var now = DateTime.UtcNow;

                var totalDays = (booking.EndDate.Date - booking.StartDate.Date).TotalDays;
                var remainingDays = (booking.EndDate.Date - now.Date).TotalDays;

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

            _logger.Information("Платеж с ID {PaymentId} отмечен как возвращенный.", paymentId);

            await _bookingNotificationService.NotifyUserAboutRefundAsync(booking, payment, isFined, finePercent, refundAmount, cancellationToken);

            return payment;
        }
    }
}