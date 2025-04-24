using AutoMapper;
using FluentValidation;
using Moq;
using RentIt.Housing.DataAccess.Entities;
using RentIt.Housing.DataAccess.Interfaces.Repositories;
using RentIt.Housing.Domain.Contracts.Requests.Housing;
using RentIt.Housing.Domain.Services.Interfaces;
using Serilog;
using System.Reflection;

namespace RentIt.Housing.Tests.HousingService
{
    public class CheckForUnathorizedAccessTests
    {
        private readonly Domain.Services.HousingService _service;
        private readonly Mock<ILogger> _logger;

        public CheckForUnathorizedAccessTests()
        {
            _logger = new Mock<ILogger>();

            _service = new Domain.Services.HousingService(
                new Mock<IHousingRepository>().Object,
                new Mock<IMapper>().Object,
                new Mock<IValidator<CreateHousingRequest>>().Object,
                new Mock<IValidator<GetFilteredHousingsRequest>>().Object,
                new Mock<IValidator<UpdateHousingRequest>>().Object,
                new Mock<IHousingImageService>().Object,
                new Mock<IUserIntegrationService>().Object,
                new Mock<IBookingIntegrationService>().Object,
                new Mock<ISpamProfanityFilterService>().Object,
                _logger.Object,
                new Mock<IEventBus>().Object
            );
        }

        [Fact]
        public void ThrowsArgumentException_WhenUserIdIsInvalidGuid()
        {
            var housing = new HousingEntity { OwnerId = Guid.NewGuid() };

            var ex = Assert.Throws<TargetInvocationException>(() =>
                InvokeCheckForUnauthorizedAccess(housing, "invalid-guid"));

            Assert.IsType<ArgumentException>(ex.InnerException);
            Assert.Equal("Некорректный формат ID владельца.", ex.InnerException!.Message);
        }

        [Fact]
        public void ThrowsUnauthorizedAccessException_WhenUserIsNotOwner()
        {
            var housing = new HousingEntity { OwnerId = Guid.NewGuid() };
            var otherUserId = Guid.NewGuid().ToString();

            var ex = Assert.Throws<TargetInvocationException>(() =>
                InvokeCheckForUnauthorizedAccess(housing, otherUserId));

            Assert.IsType<UnauthorizedAccessException>(ex.InnerException);
            Assert.Equal("Попытка неавторизованного доступа к собственности.", ex.InnerException!.Message);
        }

        [Fact]
        public void DoesNotThrow_WhenUserIsOwner()
        {
            var userId = Guid.NewGuid();
            var housing = new HousingEntity { OwnerId = userId };

            var exception = Record.Exception(() =>
                InvokeCheckForUnauthorizedAccess(housing, userId.ToString()));

            Assert.Null(exception);
        }

        private void InvokeCheckForUnauthorizedAccess(HousingEntity housing, string userId)
        {
            typeof(Domain.Services.HousingService)
                .GetMethod("CheckForUnathorizedAccess", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(_service, new object[] { housing, userId });
        }
    }

}
