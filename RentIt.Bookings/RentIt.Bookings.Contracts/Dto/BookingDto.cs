namespace RentIt.Bookings.Contracts.Dto
{
    public record BookingDto(
        Guid BookingId,
        Guid HousingId,
        Guid UserId,
        DateTime StartDate,
        DateTime EndDate,
        decimal TotalPrice,
        string Status,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );
}
