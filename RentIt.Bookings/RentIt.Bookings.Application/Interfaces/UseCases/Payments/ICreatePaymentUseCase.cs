using RentIt.Bookings.Contracts.Requests.Payments;
using RentIt.Bookings.Core.Entities;

namespace RentIt.Bookings.Application.Interfaces.UseCases.Payments
{
    public interface ICreatePaymentUseCase
    {
        Task<Payment> ExecuteAsync(ProcessTestPaymentRequest request, CancellationToken cancellationToken);
    }
}