using Microsoft.AspNetCore.Mvc;
using RentIt.Housing.Domain.Contracts.Requests.Availabilities;
using RentIt.Housing.Domain.Services;

namespace RentIt.Housing.API.Controllers
{
    [ApiController]
    [Route("api/availabilities")]
    public class AvailabilitiesController : Controller
    {
        private readonly AvailabilityService _availabilityService;

        public AvailabilitiesController(AvailabilityService availabilityService)
        {
            _availabilityService = availabilityService;
        }

        [HttpGet("{housingId}")]
        public async Task<IActionResult> GetAvailabilities(
            [FromRoute] Guid housingId,
            CancellationToken cancellationToken
            )
        {
            var availabilities = await _availabilityService.GetAvailabilitiesByHousingIdAsync(housingId, cancellationToken);

            return Ok(availabilities);
        }

        [HttpPost("{housingId}")]
        public async Task<IActionResult> CreateAvailabilities(
            [FromRoute] Guid housingId,
            [FromBody] CreateAvailabilitiesRequest request,
            CancellationToken cancellationToken)
        {
            await _availabilityService.AddAvailabilitiesAsync(housingId, request, cancellationToken);
            return Ok();
        }

        [HttpPut("{housingId}")]
        public async Task<IActionResult> UpdateAvailabilities(
            [FromRoute] Guid housingId,
            [FromBody] UpdateAvailabilitiesRequest request,
            CancellationToken cancellationToken)
        {
            await _availabilityService.UpdateAvailabilitiesAsync(housingId, request, cancellationToken);
            return Ok();
        }


        [HttpDelete("housing/{housingId}")]
        public async Task<IActionResult> DeleteAllAvailabilities(
            [FromRoute] Guid housingId,
            CancellationToken cancellationToken)
        {
            await _availabilityService.DeleteAllAvailabilities(housingId, cancellationToken);
            
            return NoContent();
        }

        [HttpDelete("{availabilityId}")]
        public async Task<IActionResult> DeleteAvailability(
            [FromRoute] Guid availabilityId,
            CancellationToken cancellationToken)
        {
            await _availabilityService.DeleteAvailabilityAsync(availabilityId, cancellationToken);

            return NoContent();
        }
    }
}
