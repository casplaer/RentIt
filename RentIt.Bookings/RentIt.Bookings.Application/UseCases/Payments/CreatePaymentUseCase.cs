using RentIt.Bookings.Application.Exceptions;
using RentIt.Bookings.Application.Interfaces.Services;
using RentIt.Bookings.Application.Interfaces.UseCases.Payments;
using RentIt.Bookings.Application.Services.Grpc;
using RentIt.Bookings.Contracts.Requests.Payments;
using RentIt.Bookings.Core.Entities;
using RentIt.Bookings.Core.Enums;
using RentIt.Bookings.Core.Interfaces.Repositories;
using Serilog;

namespace RentIt.Bookings.Application.UseCases.Payments
{
    public class CreatePaymentUseCase : ICreatePaymentUseCase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;
        private readonly UserIntegrationService _userIntegrationService;
        private readonly IEmailSender _emailSender;

        private readonly string baseUrl = "https://localhost:3000";

        public CreatePaymentUseCase(
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

        public async Task<Payment> ExecuteAsync(ProcessTestPaymentRequest request, CancellationToken cancellationToken)
        {
            _logger.Information("Начало обработки платежа для бронирования с ID: {BookingId}", request.BookingId);

            var booking = await _unitOfWork.Bookings.GetByIdAsync(request.BookingId, cancellationToken);

            if (booking == null)
            {
                _logger.Warning("Бронирование с ID: {BookingId} не найдено, уведомление не отправлено.", request.BookingId);

                throw new NotFoundException("Бронирование не найдено.");
            }

            var payment = new Payment
            {
                PaymentId = Guid.NewGuid(),
                BookingId = request.BookingId,
                Amount = request.Amount,
                PaymentTime = DateTime.UtcNow,
                Status = PaymentStatus.Pending
            };

            _logger.Information("Создан платеж с ID: {PaymentId}, сумма: {Amount}", payment.PaymentId, payment.Amount);

            await _unitOfWork.Payments.AddAsync(payment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.Information("Платеж с ID: {PaymentId} успешно сохранён в базе данных", payment.PaymentId);

            var userInfo = await _userIntegrationService.GetUserInfoAsync(booking.UserId);
            var subject = "Ваше бронирование подтверждено!";

            var paymentUrl = $"{baseUrl}/my-bookings/{booking.BookingId}/checkout";

            var body = $@"<p>Здравствуйте, {userInfo.FirstName} {userInfo.LastName}!</p>
                <p>Ваше бронирование от {booking.StartDate:dd.MM.yyyy} было подтверждено владельцем.</p>
                <p>Общая стоимость бронирования: {payment.Amount:C}.</p>
                <p>Для оплаты перейдите по ссылке: <a href='{paymentUrl}'>Оплатить бронирование</a>.</p>
                <p>С уважением,<br>Команда RentIt</p>";

            await _emailSender.SendEmailAsync(userInfo.Email, subject, body, cancellationToken);
            
            _logger.Information("Отправлено уведомление на почту {Email} о создании бронирования и запросе на оплату, PaymentID: {PaymentId}", userInfo.Email, payment.PaymentId);

            return payment;
        }
    }
}