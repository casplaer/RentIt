using RentIt.Housing.DataAccess.Entities;
using RentIt.Housing.Domain.Contracts.Requests.Housing;
using RentIt.Housing.Domain.Contracts.Responses.Housing;

namespace RentIt.Housing.Domain.Services.Interfaces
{
    public interface IHousingService
    {
        Task<GetHousingByIdResponse> GetByIdAsync(Guid housingId, CancellationToken cancellationToken);
        Task<IEnumerable<HousingEntity>> SearchAsync(GetFilteredHousingsRequest request, CancellationToken cancellationToken);
        Task AddHousingAsync(string ownerId, CreateHousingRequest request, CancellationToken cancellationToken);
        Task UpdateHousingAsync(Guid housingId, string userId, UpdateHousingRequest request, CancellationToken cancellationToken);
        Task UpdateHousingAsync(HousingEntity housing, CancellationToken cancellationToken);
        Task DeleteHousingAsync(Guid housingId, string userId, CancellationToken cancellationToken);
        Task CheckUnpublishedHousingsForSpamAsync(CancellationToken cancellationToken);
    }
}