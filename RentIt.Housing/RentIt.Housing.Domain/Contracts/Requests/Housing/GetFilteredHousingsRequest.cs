using RentIt.Housing.DataAccess.Enums;

namespace RentIt.Housing.Domain.Contracts.Requests.Housing
{
    public record GetFilteredHousingsRequest(
        string? Title,
        string? Address,
        string? City,
        string? Country,
        decimal? PricePerNight,
        int? NumberOfRooms,
        double? Rating,
        HousingStatus? Status,
        DateOnly? StartDate,
        DateOnly? EndDate,
        int Page,
        int PageSize
        );
}
