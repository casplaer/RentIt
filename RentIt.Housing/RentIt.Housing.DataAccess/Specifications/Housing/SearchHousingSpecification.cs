using RentIt.Housing.DataAccess.Entities;
using RentIt.Housing.DataAccess.Enums;

namespace RentIt.Housing.DataAccess.Specifications.Housing
{
    public class SearchHousingSpecification : Specification<HousingEntity>
    {
        public SearchHousingSpecification(
            string? title,
            string? address,
            string? city,
            string? country,
            decimal? pricePerNight,
            int? numberOfRooms,
            double? rating,
            HousingStatus? status,
            DateOnly? estimatedEndDate,
            int page,
            int pageSize)
            : base(p =>
                (string.IsNullOrEmpty(title) || p.Title.ToLower().Contains(title.ToLower())) &&
                (string.IsNullOrEmpty(address) || p.Address.ToLower().Contains(address.ToLower())) &&
                (string.IsNullOrEmpty(city) || p.City.ToLower().Contains(city.ToLower())) &&
                (string.IsNullOrEmpty(country) || p.Country.ToLower().Contains(country.ToLower())) &&
                (!pricePerNight.HasValue || p.PricePerNight == pricePerNight.Value) &&
                (!numberOfRooms.HasValue || p.NumberOfRooms == numberOfRooms.Value) &&
                (!rating.HasValue || p.Rating >= rating.Value) &&
                (!status.HasValue || p.Status == status.Value) &&
                (
                    (!estimatedEndDate.HasValue || p.EstimatedEndDate.HasValue && p.EstimatedEndDate == estimatedEndDate) ||
                    (estimatedEndDate.HasValue && p.EstimatedEndDate.HasValue && p.EstimatedEndDate <= estimatedEndDate)
                )
            )
        {
            AddInclude(p => p.Images);
            AddInclude(p => p.Reviews);

            SetPagination(page, pageSize);
        }
    }
}
