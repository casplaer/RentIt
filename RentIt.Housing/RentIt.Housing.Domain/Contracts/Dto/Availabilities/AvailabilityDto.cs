namespace RentIt.Availabilities.Domain.Contracts.Dto.Availabilities
{
    public record AvailabilityDto(
        DateOnly StartDate,
        DateOnly EndDate
        );
}
