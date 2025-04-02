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
        private readonly IAddBookingUseCase _addBookingUseCase;
        private readonly IGetBookingUseCase _getBookingUseCase;
        private readonly IUpdateBookingUseCase _updateBookingUseCase;
        private readonly IDeleteBookingUseCase _deleteBookingUseCase;

        public BookingController(
            IAddBookingUseCase addBookingUseCase,
            IGetBookingUseCase getBookingUseCase,
            IUpdateBookingUseCase updateBookingUseCase,
            IDeleteBookingUseCase deleteBookingUseCase)
        {
            _addBookingUseCase = addBookingUseCase;
            _getBookingUseCase = getBookingUseCase;
            _updateBookingUseCase = updateBookingUseCase;
            _deleteBookingUseCase = deleteBookingUseCase;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromQuery] CreateBookingRequest request,
            CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var booking = await _addBookingUseCase.ExecuteAsync(request, userId, cancellationToken);

            return CreatedAtAction(nameof(Get), new { bookingId = booking.BookingId }, booking);
        }

        [HttpGet("{bookingId}")]
        public async Task<IActionResult> Get(
            Guid bookingId,
            CancellationToken cancellationToken)
        {
            var booking = await _getBookingUseCase.ExecuteAsync(bookingId, cancellationToken);

            return Ok(booking);
        }

        [HttpPut("{bookingId}")]
        public async Task<IActionResult> Update(
            [FromRoute] Guid bookingId,
            [FromQuery] UpdateBookingRequest request,
            CancellationToken cancellationToken)
        {
            var booking = await _updateBookingUseCase.ExecuteAsync(bookingId, request, cancellationToken);

            return Ok(booking);
        }

        [HttpDelete("{bookingId}")]
        public async Task<IActionResult> Delete(
            Guid bookingId, 
            CancellationToken cancellationToken)
        {
            await _deleteBookingUseCase.ExecuteAsync(bookingId, cancellationToken);

            return NoContent();
        }
    }
}