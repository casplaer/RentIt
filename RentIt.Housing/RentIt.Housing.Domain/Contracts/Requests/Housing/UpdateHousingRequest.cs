using Microsoft.AspNetCore.Http;
using RentIt.Housing.DataAccess.Enums;

namespace RentIt.Housing.Domain.Contracts.Requests.Housing
{
    public record UpdateHousingRequest(
        string? Title,
        string? Description,
        string? Country,
        string? City,
        string? Address,
        decimal? PricePerNight,
        int? NumberOfRooms,
        List<string>? Amenities,
        HousingStatus? Status,
        DateTime? EstimatedEndDate,
        List<IFormFile>? AddedImages,
        List<string>? RemovedImages
        );
}