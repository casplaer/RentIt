using Microsoft.AspNetCore.Mvc;
using RentIt.Housing.Domain.Contracts.Requests.Housing;
using RentIt.Housing.Domain.Services;

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
            var housing = await _housingService.GetByIdAsync(housingId, cancellationToken);
            return Ok(housing);
        }

        [HttpGet]
        public async Task<IActionResult> GetHousings(
            [FromQuery] GetFilteredHousingsRequest request,
            CancellationToken cancellationToken)
        {
            var housings = await _housingService.SearchAsync(request, cancellationToken);
            return Ok(housings);
        }

        [HttpPost]
        public async Task<IActionResult> CreateHousing(
            [FromForm] CreateHousingRequest request,
            CancellationToken cancellationToken)
        {
            await _housingService.AddHousingAsync(request, cancellationToken);
            return Ok();
        }

        [HttpPut("{housingId}")]
        public async Task<IActionResult> UpdateHousing(
            [FromRoute] Guid housingId, 
            [FromForm] UpdateHousingRequest request, 
            CancellationToken cancellationToken)
        {
            await _housingService.UpdateHousingAsync(housingId, request, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{housingId}")]
        public async Task<IActionResult> DeleteHousing(
            [FromRoute] Guid housingId, 
            CancellationToken cancellationToken)
        {
            await _housingService.DeleteHousingAsync(housingId, cancellationToken);
            return NoContent();
        }
    }
}