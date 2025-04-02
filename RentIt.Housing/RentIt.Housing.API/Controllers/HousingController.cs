using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentIt.Housing.Domain.Contracts.Requests.Housing;
using RentIt.Housing.Domain.Services;
using Serilog;
using System.Security.Claims;

namespace RentIt.Housing.API.Controllers
{
    [ApiController]
    [Route("api/housings")]
    public class HousingController : Controller
    {
        private readonly HousingService _housingService;

        public HousingController(HousingService housingService)
        {
            _housingService = housingService;
        }

        [HttpGet("{housingId}")]
        public async Task<IActionResult> GetHousingById(
            [FromRoute] Guid housingId, 
            CancellationToken cancellationToken)
        {
            Log.Information("Запрос на получение жилья с ID {HousingId} в {Time}", housingId, DateTime.UtcNow);

            var housing = await _housingService.GetByIdAsync(housingId, cancellationToken);

            Log.Information("Жильё с ID {HousingId} успешно получено", housingId);

            return Ok(housing);
        }

        [HttpGet]
        public async Task<IActionResult> GetHousings(
            [FromQuery] GetFilteredHousingsRequest request,
            CancellationToken cancellationToken)
        {
            Log.Information("Запрос на получение списка жилья с фильтрами: {@Request} в {Time}", request, DateTime.UtcNow);

            var housings = await _housingService.SearchAsync(request, cancellationToken);

            Log.Information("Найдено {Count} объектов недвижимости", housings.Count());

            return Ok(housings);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateHousing(
            [FromForm] CreateHousingRequest request,
            CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Log.Information("Попытка создания новой собственности для пользователя {UserId} в {Time}", userId, DateTime.UtcNow);

            await _housingService.AddHousingAsync(userId, request, cancellationToken);

            Log.Information("Собственность успешно создана для пользователя {UserId}", userId);

            return Ok();
        }

        [HttpPut("{housingId}")]
        [Authorize]
        public async Task<IActionResult> UpdateHousing(
            [FromRoute] Guid housingId, 
            [FromForm] UpdateHousingRequest request, 
            CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            Log.Information("Запрос на обновление собственности с ID {HousingId} в {Time}", housingId, DateTime.UtcNow);

            await _housingService.UpdateHousingAsync(housingId, userId, request, cancellationToken);

            Log.Information("Собственность с ID {HousingId} успешно обновлена", housingId);

            return NoContent();
        }

        [HttpDelete("{housingId}")]
        [Authorize]
        public async Task<IActionResult> DeleteHousing(
            [FromRoute] Guid housingId, 
            CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            Log.Information("Запрос на удаление собственности с ID {HousingId} в {Time}", housingId, DateTime.UtcNow);

            await _housingService.DeleteHousingAsync(housingId, userId, cancellationToken);

            Log.Information("Собственность с ID {HousingId} успешно удалена", housingId);

            return NoContent();
        }
    }
}