using RentIt.Bookings.Application.Exceptions;
using RentIt.Bookings.Application.Interfaces.Services;
using RentIt.Bookings.Application.Interfaces.UseCases.Payments;
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
        private readonly UserIntegrationService _userIntegrationService;
        private readonly IEmailSender _emailSender;

        public ConfirmPaymentUseCase(
            IUnitOfWork unitOfWork,
            ILogger logger,
            UserIntegrationService userIntegrationService,
            IEmailSender emailSender)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _userIntegrationService = userIntegrationService;
            _emailSender = emailSender;
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

            var userInfo = await _userIntegrationService.GetUserInfoAsync(booking.UserId);
            var subject = "Спасибо за оплату.";
            var body = $"Здравствуйте, {userInfo.FirstName} {userInfo.LastName}! Ваш платеж на сумму {payment.Amount} успешно завершен. Приятного отдыха.";
            await _emailSender.SendEmailAsync(userInfo.Email, subject, body, cancellationToken);

            _logger.Information("Уведомление о подтверждении отправлено пользователю {Email}.", userInfo.Email);

            payment.Status = PaymentStatus.Completed;
            payment.PaymentTime = DateTime.UtcNow;

            _unitOfWork.Payments.Update(payment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.Information("Платеж с ID {PaymentId} подтвержден.", paymentId);

            return payment;
        }
    }

}
