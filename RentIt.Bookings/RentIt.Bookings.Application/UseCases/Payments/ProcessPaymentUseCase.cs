using RentIt.Bookings.Application.Interfaces.UseCases.Payments;
using RentIt.Bookings.Contracts.Requests.Payments;
using RentIt.Bookings.Core.Entities;
using RentIt.Bookings.Core.Enums;
using RentIt.Bookings.Core.Interfaces.Repositories;

namespace RentIt.Bookings.Application.UseCases.Payments
{
    public class ProcessPaymentUseCase : IProcessPaymentUseCase
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProcessPaymentUseCase(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Payment> ExecuteAsync(ProcessTestPaymentRequest request, CancellationToken cancellationToken)
        {
            var payment = new Payment
            {
                PaymentId = Guid.NewGuid(),
                BookingId = request.BookingId,
                Amount = request.Amount,
                PaymentTime = DateTime.UtcNow,
                Status = PaymentStatus.Pending
            };

            await _unitOfWork.Payments.AddAsync(payment, cancellationToken);
            await _unitOfWork.SaveChangesAsync();

            return payment;
        }
    }
}