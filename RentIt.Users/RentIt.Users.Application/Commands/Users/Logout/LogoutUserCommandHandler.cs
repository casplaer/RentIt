using MediatR;
using Microsoft.AspNetCore.Http;
using RentIt.Users.Application.Interfaces;

namespace RentIt.Users.Application.Commands.Users.Logout
{
    public class LogoutUserCommandHandler : IRequestHandler<LogoutUserCommand, string>
    {
        private readonly IJwtProvider _jwtProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LogoutUserCommandHandler(IJwtProvider jwtProvider, IHttpContextAccessor httpContextAccessor)
        {
            _jwtProvider = jwtProvider ?? throw new ArgumentNullException(nameof(jwtProvider));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task<string> Handle(LogoutUserCommand request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(request.RefreshToken))
            {
                await _jwtProvider.RevokeRefreshTokenAsync(request.RefreshToken, cancellationToken);
            }

            if (!string.IsNullOrEmpty(request.AccessToken))
            {
                await _jwtProvider.RevokeAccessTokenAsync(request.AccessToken, cancellationToken);
            }

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                httpContext.Response.Cookies.Delete("AccessToken");
                httpContext.Response.Cookies.Delete("RefreshToken");
            }

            return "Пользователь успешно вышел.";
        }
    }
}
