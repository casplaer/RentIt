using AutoMapper;
using FluentValidation;
using Hangfire;
using RentIt.Bookings.Application.Interfaces.Services;
using RentIt.Bookings.Application.Interfaces.Services.Grpc;
using RentIt.Bookings.Application.Interfaces.UseCases.Bookings;
using RentIt.Bookings.Contracts.Requests.Bookings;
using RentIt.Bookings.Core.Entities;
using RentIt.Bookings.Core.Interfaces.Repositories;

namespace RentIt.Bookings.Application.UseCases.Bookings
{
    public class AddBookingUseCase : IAddBookingUseCase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHousingIntegrationService _housingService;
        private readonly IAppLogger _logger;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateBookingRequest> _validator;
        private readonly IBookingNotificationService _bookingNotificationService;

        public AddBookingUseCase(
            IUnitOfWork unitOfWork,
            IHousingIntegrationService housingService,
            IAppLogger logger,
            IMapper mapper,
            IValidator<CreateBookingRequest> validator,
            IBookingNotificationService bookingNotificationService)
        {
            _unitOfWork = unitOfWork;
            _housingService = housingService;
            _logger = logger;
            _mapper = mapper;
            _validator = validator;
            _bookingNotificationService = bookingNotificationService;
        }

        public async Task<Booking> ExecuteAsync(
            CreateBookingRequest request,
            string userId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Начало создания бронирования. Запрос: {@Request}, UserId: {UserId}", request, userId);

            var userIdParseAttempt = Guid.TryParse(userId, out var userGuid);

            if (!userIdParseAttempt)
            {
                _logger.LogWarning("Некорректный формат UserId: {UserId}", userId);

                throw new ArgumentException("Некорректный формат ID.");
            }

            await _validator.ValidateAndThrowAsync(request, cancellationToken);

            var anyOverlapping = await _unitOfWork.Bookings.AnyOverlappingBookingAsync(request.HousingId, request.StartDate, request.EndDate, cancellationToken);

            if (anyOverlapping)
            {
                throw new ArgumentException("На выбранные даты уже существует бронь.");
            }

            _logger.LogInformation("Получение информации о жилье для HousingId: {HousingId}", request.HousingId);

            var housingResponse = await _housingService.GetHousingInfoAsync(request.HousingId);

            _logger.LogInformation("Информация о жилье получена. Цена за ночь: {PricePerNight}", housingResponse.PricePerNight);

            if (userGuid == housingResponse.OwnerId)
            {
                _logger.LogWarning("Пользователь попытался забронировать свое же жилье.");

                throw new ArgumentException("Извините, но забронировать свою же собственность невозможно.");
            }

            var nights = (request.EndDate.Date - request.StartDate.Date).Days;

            _logger.LogInformation("Количество ночей: {Nights}", nights);

            var computedTotalPrice = housingResponse.PricePerNight * nights;

            _logger.LogInformation("Общая стоимость бронирования: {TotalPrice}", computedTotalPrice);

            var booking = _mapper.Map<Booking>(request, opt => {
                opt.Items["ComputedTotalPrice"] = computedTotalPrice;
                opt.Items["UserId"] = userGuid;
            });

            _logger.LogInformation("Создан объект бронирования: {@Booking}", booking);

            await _unitOfWork.Bookings.AddAsync(booking, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Бронирование успешно сохранено в базе. Id: {BookingId}", booking.BookingId);

            BackgroundJob.Enqueue(() =>
                _bookingNotificationService.NotifyOwnerAboutNewBookingAsync(housingResponse, request, CancellationToken.None));

            BackgroundJob.Enqueue(() =>
                _bookingNotificationService.NotifyUserAboutBookingCreationAsync(housingResponse, request, userGuid, CancellationToken.None));

            return booking;
        }
    }
}
