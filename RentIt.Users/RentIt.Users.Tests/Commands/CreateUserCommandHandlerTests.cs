using Moq;
using RentIt.Users.Application.Commands.Users.Create;
using RentIt.Users.Application.Exceptions;
using RentIt.Users.Application.Interfaces;
using RentIt.Users.Core.Entities;
using RentIt.Users.Core.Interfaces.Repositories;
using AutoMapper;
using Hangfire;
using Hangfire.MemoryStorage;

namespace RentIt.Users.Tests.Commands
{
    public class CreateUserCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IRoleRepository> _mockRoleRepository;
        private readonly Mock<IPasswordHasher> _mockPasswordHasher;
        private readonly Mock<IEmailNormalizer> _mockEmailNormalizer;
        private readonly Mock<IAccountTokenRepository> _mockAccountTokenRepository;
        private readonly Mock<IEmailSender> _mockEmailSender;
        private readonly Mock<IAccountTokenGenerator> _mockAccountTokenGenerator;
        private readonly Mock<ILinkGenerator> _mockLinkGenerator;
        private readonly Mock<IMapper> _mockMapper;

        private readonly CreateUserCommandHandler _handler;

        public CreateUserCommandHandlerTests()
        {
            GlobalConfiguration.Configuration.UseMemoryStorage();

            _mockUserRepository = new Mock<IUserRepository>();
            _mockRoleRepository = new Mock<IRoleRepository>();
            _mockPasswordHasher = new Mock<IPasswordHasher>();
            _mockEmailNormalizer = new Mock<IEmailNormalizer>();
            _mockAccountTokenRepository = new Mock<IAccountTokenRepository>();
            _mockEmailSender = new Mock<IEmailSender>();
            _mockAccountTokenGenerator = new Mock<IAccountTokenGenerator>();
            _mockLinkGenerator = new Mock<ILinkGenerator>();
            _mockMapper = new Mock<IMapper>();

            _handler = new CreateUserCommandHandler(
                _mockUserRepository.Object,
                _mockRoleRepository.Object,
                _mockPasswordHasher.Object,
                _mockEmailNormalizer.Object,
                _mockAccountTokenRepository.Object,
                _mockMapper.Object,
                _mockEmailSender.Object,
                _mockAccountTokenGenerator.Object,
                _mockLinkGenerator.Object
            );
        }

        [Fact]
        public async Task Handle_ShouldCreateUser_WhenEmailIsNotTaken()
        {
            var command = new CreateUserCommand("John", 
                                                "Doe", 
                                                "john.doe@example.com", 
                                                "password123", 
                                                "password123");

            var normalizedEmail = "john.doe@example.com".ToLower();

            var role = new Role 
            {
                RoleId = Guid.NewGuid(), 
                RoleName = "User" 
            };

            _mockEmailNormalizer.Setup(x => x.NormalizeEmail(command.Email)).Returns(normalizedEmail);
            _mockRoleRepository.Setup(x => x.GetRoleByNameAsync("User", It.IsAny<CancellationToken>())).ReturnsAsync(role);
            _mockUserRepository.Setup(x => x.GetUserByNormalizedEmailAsync(normalizedEmail, It.IsAny<CancellationToken>())).ReturnsAsync((User)null);

            _mockPasswordHasher.Setup(x => x.Hash(command.Password)).Returns("hashedPassword");
            _mockAccountTokenGenerator.Setup(x => x.GenerateToken(It.IsAny<int>())).Returns("confirmationToken");

            await _handler.Handle(command, CancellationToken.None);

            _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockAccountTokenRepository.Verify(x => x.AddAsync(It.IsAny<AccountToken>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowUserAlreadyExistsException_WhenEmailIsTaken()
        {
            var command = new CreateUserCommand("John", 
                                                "Doe", 
                                                "john.doe@example.com", 
                                                "password123", 
                                                "password123");

            var normalizedEmail = "john.doe@example.com".ToLower();

            var existingUser = new User { Email = command.Email, NormalizedEmail = normalizedEmail };

            _mockEmailNormalizer.Setup(x => x.NormalizeEmail(command.Email)).Returns(normalizedEmail);
            _mockUserRepository.Setup(x => x.GetUserByNormalizedEmailAsync(normalizedEmail, It.IsAny<CancellationToken>())).ReturnsAsync(existingUser);

            await Assert.ThrowsAsync<UserAlreadyExistsException>(() => _handler.Handle(command, CancellationToken.None));
        }
    }
}
