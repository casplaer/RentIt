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
        DateTime? UserStartDate,
        DateTime? UserEndDate,
        int Page = 1,
        int PageSize = 10
        );
}
