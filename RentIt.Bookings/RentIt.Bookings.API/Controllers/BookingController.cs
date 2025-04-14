using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentIt.Bookings.Application.Interfaces.UseCases.Bookings;
using RentIt.Bookings.Contracts.Requests.Bookings;
using System.Security.Claims;

namespace RentIt.Bookings.API.Controllers
{
    [ApiController]
    [Route("api/bookings")]
    public class BookingController : Controller
    {
        private readonly Serilog.ILogger _logger;

        private readonly IAddBookingUseCase _addBookingUseCase;
        private readonly IGetBookingUseCase _getBookingUseCase;
        private readonly IGetBookingsByUserIdUseCase _getBookingsByUserIdUseCase;
        private readonly IGetBookingsByHousingIdUseCase _getBookingsByHousingIdUseCase;
        private readonly IUpdateBookingUseCase _updateBookingUseCase;
        private readonly IDeleteBookingUseCase _deleteBookingUseCase;
        private readonly IConfirmBookingUseCase _confirmBookingUseCase;
        private readonly IRejectBookingUseCase _rejectBookingUseCase;
        private readonly ICancelBookingUseCase _cancelBookingUseCase;
        private readonly IAdminCancelBookingUseCase _adminCancelBookingUseCase;

        public BookingController(
            Serilog.ILogger logger,
            IAddBookingUseCase addBookingUseCase,
            IGetBookingUseCase getBookingUseCase,
            IGetBookingsByHousingIdUseCase getBookingsByHousingIdUseCase,
            IGetBookingsByUserIdUseCase getBookingsByUserIdUseCase,
            IUpdateBookingUseCase updateBookingUseCase,
            IDeleteBookingUseCase deleteBookingUseCase,
            IConfirmBookingUseCase confirmBookingUseCase,
            IRejectBookingUseCase rejectBookingUseCase,
            ICancelBookingUseCase cancelBookingUseCase,
            IAdminCancelBookingUseCase adminCancelBookingUseCase)
        {
            _logger = logger;
            _addBookingUseCase = addBookingUseCase;
            _getBookingUseCase = getBookingUseCase;
            _getBookingsByUserIdUseCase = getBookingsByUserIdUseCase;
            _getBookingsByHousingIdUseCase = getBookingsByHousingIdUseCase;
            _updateBookingUseCase = updateBookingUseCase;
            _deleteBookingUseCase = deleteBookingUseCase;
            _confirmBookingUseCase = confirmBookingUseCase;
            _rejectBookingUseCase = rejectBookingUseCase;
            _cancelBookingUseCase = cancelBookingUseCase;
            _adminCancelBookingUseCase = adminCancelBookingUseCase;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateBooking(
            [FromQuery] CreateBookingRequest request,
            CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            _logger.Information("Начало создания бронирования для пользователя {UserId}.", userId);

            var booking = await _addBookingUseCase.ExecuteAsync(request, userId, cancellationToken);

            _logger.Information("Бронирование успешно создано. ID: {BookingId} для пользователя {UserId}", booking.BookingId, userId);

            return  CreatedAtAction(nameof(GetBooking), new { bookingId = booking.BookingId }, booking);
        }

        [Authorize]
        [HttpGet("{bookingId}")]
        public async Task<IActionResult> GetBooking(
            Guid bookingId,
            CancellationToken cancellationToken)
        {
            var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var authenticatedUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            _logger.Information("Запрос на получение бронирования с ID: {BookingId}", bookingId);

            var booking = await _getBookingUseCase.ExecuteAsync(
                                                       bookingId, 
                                                       authenticatedUserId!,
                                                       authenticatedUserRole!,
                                                       cancellationToken);

            _logger.Information("Бронирование с ID {BookingId} успешно получено", bookingId);

            return Ok(booking);
        }

        [Authorize]
        [HttpGet("users/{userId}")]
        public async Task<IActionResult> GetBookingByUserId(
            [FromRoute] Guid userId,
            [FromQuery] GetBookingsByPagesRequest request,
            CancellationToken cancellationToken)
        {
            var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var authenticatedUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            _logger.Information("Запрос на получение бронирований для пользователя {UserId}.", userId);

            var bookingDtos = await _getBookingsByUserIdUseCase.ExecuteAsync(
                                                                    userId,
                                                                    authenticatedUserId!,
                                                                    authenticatedUserRole!,
                                                                    request, 
                                                                    cancellationToken);

            _logger.Information("Получено {Count} бронирований для пользователя {UserId}", bookingDtos.Items.Count, userId);

            return Ok(bookingDtos);
        }

        [Authorize]
        [HttpGet("housings/{housingId}")]
        public async Task<IActionResult> GetBookingsByHousingId(
            [FromRoute] Guid housingId,
            GetBookingsByPagesRequest request,
            CancellationToken cancellationToken
            )
        {
            var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var authenticatedUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            _logger.Information("Запрос на получение бронирований по ID собственности {HousingId}.", housingId);

            var bookingDtos = await _getBookingsByHousingIdUseCase.ExecuteAsync(
                                                                       housingId,
                                                                       authenticatedUserId!,
                                                                       authenticatedUserRole!,
                                                                       request,
                                                                       cancellationToken);

            _logger.Information("Получено {Count} бронирований для собственности {HousingId}", housingId);

            return Ok();
        }

        [Authorize]
        [HttpPut("{bookingId}")]
        public async Task<IActionResult> UpdateBooking(
            [FromRoute] Guid bookingId,
            [FromQuery] UpdateBookingRequest request,
            CancellationToken cancellationToken)
        {
            _logger.Information("Начало обновления бронирования с ID: {BookingId} с данными: {@Request}", bookingId, request);

            var booking = await _updateBookingUseCase.ExecuteAsync(bookingId, request, cancellationToken);

            _logger.Information("Бронирование с ID {BookingId} успешно обновлено", bookingId);

            return Ok(booking);
        }

        [Authorize]
        [HttpPut("{bookingId}/confirmation")]
        public async Task<IActionResult> ConfirmBooking(
            [FromRoute] Guid bookingId,
            CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            _logger.Information("Запрос на подтверждение бронирования с ID: {BookingId}.", bookingId);

            await _confirmBookingUseCase.ExecuteAsync(userId!, bookingId, cancellationToken);

            _logger.Information("Бронирование c ID {BookingId} успешно подтверждено.", bookingId);

            return Ok("Бронирование подтверждено.");
        }

        [Authorize]
        [HttpPut("{bookingId}/rejection")]
        public async Task<IActionResult> RejectBooking(
            [FromRoute] Guid bookingId,
            CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            _logger.Information("Запрос на отклонение бронирования с ID: {BookingId}.", bookingId);

            await _rejectBookingUseCase.ExecuteAsync(userId!, bookingId, cancellationToken);

            _logger.Information("Бронирование c ID {BookingId} успешно отклонено.", bookingId);

            return Ok("Бронирование отклонено.");
        }

        [Authorize]
        [HttpPut("{bookingId}/cancellation")]
        public async Task<IActionResult> CancelBooking(
            [FromRoute] Guid bookingId,
            CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            _logger.Information("Запрос на отмену бронирования с ID: {BookingId}.", bookingId);

            await _cancelBookingUseCase.ExecuteAsync(bookingId, userId!, cancellationToken);

            _logger.Information("Бронирование c ID {BookingId} успешно отменено.", bookingId);

            return Ok("Бронирование отменено.");
        }

        [Authorize(Policy = "AdminPolicy")]
        [HttpPut("{bookingId}/admin-cancellation")]
        public async Task<IActionResult> AdminCancelBooking(
            [FromRoute] Guid bookingId,
            CancellationToken cancellationToken)
        {
            _logger.Information("Запрос на отмену бронирования с ID: {BookingId}.", bookingId);

            await _adminCancelBookingUseCase.ExecuteAsync(bookingId, cancellationToken);

            _logger.Information("Бронирование c ID {BookingId} успешно отменено.", bookingId);

            return Ok("Бронирование отменено.");
        }

        [Authorize]
        [HttpDelete("{bookingId}")]
        public async Task<IActionResult> DeleteBooking(
            Guid bookingId,
            CancellationToken cancellationToken)
        {
            _logger.Information("Запрос на удаление бронирования с ID: {BookingId}", bookingId);

            await _deleteBookingUseCase.ExecuteAsync(bookingId, cancellationToken);

            _logger.Information("Бронирование с ID {BookingId} успешно удалено", bookingId);

            return Ok("Бронирование удалено.");
        }
    }
}