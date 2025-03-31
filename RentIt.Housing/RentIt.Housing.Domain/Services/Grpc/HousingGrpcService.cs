using Grpc.Core;
using RentIt.Housing.Domain.Exceptions;
using RentIt.Protos.Housing;
using Serilog;

namespace RentIt.Housing.Domain.Services.Grpc
{
    public class HousingGrpcService : Protos.Housing.HousingService.HousingServiceBase
    {
        private readonly HousingService _housingService;
        private readonly ILogger _logger;

        public HousingGrpcService(
            HousingService housingService,
            ILogger logger)
        {
            _housingService = housingService;
            _logger = logger;
        }

        public override async Task<GetHousingResponse> GetHousing(GetHousingRequest request, ServerCallContext context)
        {
            _logger.Information("Получение собственности с ID {HousingId}", request.HousingId);

            if (!Guid.TryParse(request.HousingId, out var housingGuid))
            {
                _logger.Warning("Неверный формат housing_id. Ожидается GUID.");

                throw new RpcException(new Status(StatusCode.InvalidArgument, "Неверный формат housing_id. Ожидается GUID."));
            }

            var housing = await _housingService.GetByIdAsync(housingGuid, new CancellationToken());

            if ( housing == null )
            {
                _logger.Warning("Собственность с ID {HousingID} не найдена.", request.HousingId);

                throw new NotFoundException("Собственность с таким ID не найдена.");
            }

            return new GetHousingResponse
            {
                HousingId = housing.Housing.HousingId.ToString(),
                HousingName = housing.Housing.Title.ToString(),
                PricePerNight = (double) housing.Housing.PricePerNight
            };
        }
    }
}
