namespace RentIt.Bookings.Contracts.Dto
{
    public record HousingInfoDto(
        Guid OwnerId,
        string HousingName,
        decimal PricePerNight
        );
}
