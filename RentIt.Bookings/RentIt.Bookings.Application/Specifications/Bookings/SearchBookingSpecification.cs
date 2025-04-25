using RentIt.Bookings.Core.Entities;
using RentIt.Bookings.Core.Enums;
using System.Linq.Expressions;

namespace RentIt.Bookings.Application.Specifications.Bookings
{
    public class SearchBookingSpecification : Specification<Booking>
    {
        public SearchBookingSpecification(
            Guid? housingId = null,
            Guid? userId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            BookingStatus? status = null,
            int? page = null,
            int? pageSize = null)
            : base(BuildCriteria(housingId, userId, startDate, endDate, status))
        {
            SetPagination(page, pageSize);
        }

        private static Expression<Func<Booking, bool>> BuildCriteria(
            Guid? housingId,
            Guid? userId,
            DateTime? startDate,
            DateTime? endDate,
            BookingStatus? status)
        {
            return booking =>
                (!housingId.HasValue || booking.HousingId == housingId.Value) &&
                (!userId.HasValue || booking.UserId == userId.Value) &&
                (!startDate.HasValue || booking.StartDate >= startDate.Value) &&
                (!endDate.HasValue || booking.EndDate <= endDate.Value) &&
                (!status.HasValue || booking.Status == status.Value);
        }
    }
}
