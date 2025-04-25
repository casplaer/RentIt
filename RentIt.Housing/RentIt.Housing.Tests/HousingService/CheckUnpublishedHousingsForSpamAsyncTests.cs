using AutoMapper;
using FluentValidation;
using Moq;
using RentIt.Housing.DataAccess.Entities;
using RentIt.Housing.DataAccess.Enums;
using RentIt.Housing.DataAccess.Interfaces.Repositories;
using RentIt.Housing.Domain.Contracts.Requests.Housing;
using RentIt.Housing.Domain.Services.Interfaces;
using Serilog;

namespace RentIt.Housing.Tests.HousingService
{
    public class CheckUnpublishedHousingsForSpamAsyncTests
    {
        private readonly Mock<IHousingRepository> _housingRepository;
        private readonly Mock<ISpamProfanityFilterService> _spamService;
        private readonly Mock<ILogger> _logger;

        private readonly Domain.Services.HousingService _service;

        public CheckUnpublishedHousingsForSpamAsyncTests()
        {
            _housingRepository = new Mock<IHousingRepository>();
            _spamService = new Mock<ISpamProfanityFilterService>();
            _logger = new Mock<ILogger>();

            _service = new Domain.Services.HousingService(
                _housingRepository.Object,
                new Mock<IMapper>().Object,
                new Mock<IValidator<CreateHousingRequest>>().Object,
                new Mock<IValidator<GetFilteredHousingsRequest>>().Object,
                new Mock<IValidator<UpdateHousingRequest>>().Object,
                new Mock<IHousingImageService>().Object,
                new Mock<IUserIntegrationService>().Object,
                new Mock<IBookingIntegrationService>().Object,
                _spamService.Object,
                _logger.Object,
                new Mock<IEventBus>().Object
            );
        }

        [Fact]
        public async Task ProcessesAllUnpublishedHousings()
        {
            var housings = new List<HousingEntity>
            {
                new() 
                { 
                    HousingId = Guid.NewGuid(), 
                    Title = "Test1" 
                },
                new()
                { 
                    HousingId = Guid.NewGuid(), 
                    Title = "Test2" 
                }
            };

            _housingRepository.Setup(r => r.GetAllUnpublishedAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(housings);

            _spamService.Setup(s => s.ContainsSpamOrProfanity(It.IsAny<string>()))
                .Returns(false);

            _housingRepository.Setup(r => r.UpdateAsync(It.IsAny<HousingEntity>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await _service.CheckUnpublishedHousingsForSpamAsync(CancellationToken.None);

            _housingRepository.Verify(r => r.UpdateAsync(It.IsAny<HousingEntity>(), It.IsAny<CancellationToken>()), Times.Exactly(housings.Count));
        }

        [Fact]
        public async Task DoesNothing_WhenNoUnpublishedHousings()
        {
            _housingRepository.Setup(r => r.GetAllUnpublishedAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<HousingEntity>());

            await _service.CheckUnpublishedHousingsForSpamAsync(CancellationToken.None);

            _housingRepository.Verify(r => r.UpdateAsync(It.IsAny<HousingEntity>(), It.IsAny<CancellationToken>()), Times.Never);
            _spamService.Verify(s => s.ContainsSpamOrProfanity(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task MarksHousingAsSpam_WhenSpamDetected()
        {
            var housing = new HousingEntity
            {
                HousingId = Guid.NewGuid(),
                Title = "Nice place but fucking expensive",
                Description = "Best damn view in the city",
                Status = HousingStatus.Unpublished
            };

            _housingRepository.Setup(r => r.GetAllUnpublishedAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync([housing]);

            _spamService.Setup(s => s.ContainsSpamOrProfanity(housing.Title))
                .Returns(true);

            _housingRepository.Setup(r => r.UpdateAsync(It.IsAny<HousingEntity>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await _service.CheckUnpublishedHousingsForSpamAsync(CancellationToken.None);

            _housingRepository.Verify(r => r.UpdateAsync(It.Is<HousingEntity>(h =>
                h.HousingId == housing.HousingId &&
                h.Status == HousingStatus.Rejected), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}