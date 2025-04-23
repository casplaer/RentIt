using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentIt.Bookings.Application.Interfaces.UseCases.Messages;
using System.Security.Claims;

namespace RentIt.Bookings.API.Controllers
{
    [Authorize]
    [Route("api/chat")]
    public class ChatController : Controller
    {
        private readonly IGetMessagesUseCase _getMessagesUseCase;
        private readonly Serilog.ILogger _logger;

        public ChatController(
            IGetMessagesUseCase getMessagesUseCase, 
            Serilog.ILogger logger)
        {
            _getMessagesUseCase = getMessagesUseCase;
            _logger = logger;
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetChatHistory(
            Guid otherUserId, 
            CancellationToken cancellationToken)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            _logger.Information("Запрос на получение сообщений между {UserId} и {OwnerId}.", currentUserId, otherUserId);

            var messages = await _getMessagesUseCase.ExecuteAsync(currentUserId!, otherUserId, cancellationToken);

            _logger.Information("Получено {Count} сообщений.", messages.Count());

            return Ok(messages);
        }
    }
}