using RentIt.Bookings.Contracts.Dto;
using RentIt.Bookings.Contracts.Requests.Bookings;
using RentIt.Bookings.Core.Entities;

namespace RentIt.Bookings.Application.Interfaces.Services
{
    public interface IBookingNotificationService
    {
        Task NotifyOwnerAboutNewBookingAsync(HousingInfoDto housingResponse, CreateBookingRequest request, CancellationToken cancellationToken);
        Task NotifyUserAboutBookingCreationAsync(HousingInfoDto housingResponse, CreateBookingRequest request, Guid userGuid, CancellationToken cancellationToken);
        Task NotifyUserAboutBookingCancellationAsync(Booking bookingToCancel, CancellationToken cancellationToken);
        Task NotifyUserAboutBookingRejectionAsync(Booking bookingToReject, HousingInfoDto housingInfo, CancellationToken cancellationToken);
        Task NotifyUserAboutBookingConfirmationAsync(Booking booking, Payment payment, CancellationToken cancellationToken);
        Task NotifyUserAboutPaymentSuccessAsync(Booking booking, Payment payment, CancellationToken cancellationToken);
        Task NotifyUserAboutRefundAsync(Booking booking, Payment payment, bool isFined, decimal finePercent, decimal refundAmount, CancellationToken cancellationToken);
        Task NotifyUserAboutBookingCompletionAsync(Booking booking, CancellationToken cancellationToken);
    }
}
