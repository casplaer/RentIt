namespace RentIt.Bookings.Contracts.Dto
{
    public record HousingInfoDto(
        Guid HousingId,
        string HousingName,
        decimal PricePerNight
        );
}
