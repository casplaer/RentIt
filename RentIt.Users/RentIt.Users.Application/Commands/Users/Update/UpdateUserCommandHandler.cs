using AutoMapper;
using MediatR;
using RentIt.Users.Application.Exceptions;
using RentIt.Users.Application.Interfaces;
using RentIt.Users.Core.Interfaces.Repositories;
using Serilog;

namespace RentIt.Users.Application.Commands.Users.Update
{
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailNormalizer _emailNormalizer;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public UpdateUserCommandHandler(
            IUserRepository userRepository,
            IEmailNormalizer emailNormalizer,
            IMapper mapper,
            ILogger logger)
        {
            _userRepository = userRepository;
            _emailNormalizer = emailNormalizer;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<bool> Handle(
            UpdateUserCommand request,
            CancellationToken cancellationToken)
        {
            _logger.Information("Запрос на обновление пользователя с Id: {UserId}", request.UserId);

            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                _logger.Warning("Пользователь с Id {UserId} не найден", request.UserId);

                throw new NotFoundException("Пользователь не найден.");
            }

            _mapper.Map(request, user);

            user.NormalizedEmail = _emailNormalizer.NormalizeEmail(request.Email);
            user.UpdatedAt = DateTime.UtcNow;

            _logger.Information("Обновляем данные пользователя с Id: {UserId}", request.UserId);

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync(cancellationToken);

            _logger.Information("Пользователь с Id: {UserId} успешно обновлен", request.UserId);

            return true;
        }
    }
}
