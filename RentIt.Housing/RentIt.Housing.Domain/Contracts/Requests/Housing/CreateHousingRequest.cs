using Microsoft.AspNetCore.Http;

namespace RentIt.Housing.Domain.Contracts.Requests.Housing
{
    public record CreateHousingRequest(
        string Title,
        string Description,
        string Country,
        string City,
        string Address,
        decimal PricePerNight,
        int NumberOfRooms,
        IEnumerable<string> Amenities,
        IEnumerable<IFormFile>? Images
        );
}
