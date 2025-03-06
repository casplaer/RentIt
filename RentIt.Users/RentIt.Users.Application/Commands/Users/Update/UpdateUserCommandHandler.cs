using AutoMapper;
using FluentValidation;
using MediatR;
using RentIt.Users.Application.Exceptions;
using RentIt.Users.Application.Interfaces;
using RentIt.Users.Core.Interfaces.Repositories;

namespace RentIt.Users.Application.Commands.Users.Update
{
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailNormalizer _emailNormalizer;
        private readonly IMapper _mapper;
        private readonly IValidator<UpdateUserCommand> _validator;

        public UpdateUserCommandHandler(
            IUserRepository userRepository,
            IEmailNormalizer emailNormalizer,
            IMapper mapper,
            IValidator<UpdateUserCommand> validator)
        {
            _userRepository = userRepository;
            _emailNormalizer = emailNormalizer;
            _mapper = mapper;
            _validator = validator;
        }

        public async Task<bool> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                throw new NotFoundException("Пользователь не найден.");
            }

            await _validator.ValidateAndThrowAsync(request, cancellationToken);

            _mapper.Map(request, user);

            user.NormalizedEmail = _emailNormalizer.NormalizeEmail(request.Email);
            user.UpdatedAt = DateTime.UtcNow;

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
