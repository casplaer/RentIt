using RentIt.Bookings.Application.Exceptions;
using RentIt.Bookings.Application.Interfaces.Services;
using RentIt.Bookings.Application.Interfaces.UseCases.Payments;
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
        private readonly IBookingNotificationService _bookingNotificationService;

        private readonly string baseUrl = "https://localhost:3000";

        public CreatePaymentUseCase(
            IUnitOfWork unitOfWork,
            ILogger logger,
            IBookingNotificationService bookingNotificationService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _bookingNotificationService = bookingNotificationService;
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

            await _bookingNotificationService.NotifyUserAboutBookingConfirmationAsync(booking, payment, cancellationToken);

            return payment;
        }
    }
}