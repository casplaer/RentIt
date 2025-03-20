using RentIt.Availabilities.Domain.Contracts.Dto.Availabilities;

namespace RentIt.Housing.Domain.Contracts.Dto.Housing
{
    public record HousingDto
    {
        public Guid HousingId { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string Address { get; init; } = string.Empty;
        public string City { get; init; } = string.Empty;
        public string Country { get; init; } = string.Empty;
        public decimal PricePerNight { get; init; }
        public int NumberOfRooms { get; init; }
        public List<string> Amenities { get; init; } = new();
        public List<AvailabilityDto> Availabilities { get; init; } = new();
    }
}