using Microsoft.AspNetCore.Mvc;
using RentIt.Bookings.Application.Interfaces.UseCases.Payments;
using System.Security.Claims;
using RentIt.Bookings.Application.Interfaces.Services;

namespace RentIt.Bookings.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class PaymentController : Controller
    {
        private readonly IConfirmPaymentUseCase _confirmPaymentUseCase;
        private readonly IAppLogger _logger;
        
        public PaymentController(
            IConfirmPaymentUseCase confirmPaymentUseCase,
            IAppLogger logger)
        {
            _confirmPaymentUseCase = confirmPaymentUseCase;
            _logger = logger;
        }

        [HttpPost("payment/confirmation")]
        public async Task<IActionResult> Confirm(
            [FromQuery] Guid bookingId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Обработка запроса на подтверждение платежа для бронирования {BookingId}.", bookingId);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            await _confirmPaymentUseCase.ExecuteAsync(bookingId, userId!, cancellationToken);

            _logger.LogInformation("Платеж успешно подтвержден.");

            return Ok("Платеж завершён!");
        }
    }
}