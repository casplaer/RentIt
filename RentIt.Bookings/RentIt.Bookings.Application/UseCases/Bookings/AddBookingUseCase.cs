using AutoMapper;
using FluentValidation;
using RentIt.Bookings.Application.Interfaces.UseCases.Bookings;
using RentIt.Bookings.Application.Services.Grpc;
using RentIt.Bookings.Contracts.Requests.Bookings;
using RentIt.Bookings.Core.Entities;
using RentIt.Bookings.Core.Interfaces.Repositories;
using Serilog;

namespace RentIt.Bookings.Application.UseCases.Bookings
{
    public class AddBookingUseCase : IAddBookingUseCase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly HousingIntegrationsService _housingService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateBookingRequest> _validator;

        public AddBookingUseCase(
            IUnitOfWork unitOfWork,
            HousingIntegrationsService housingService,
            ILogger logger,
            IMapper mapper,
            IValidator<CreateBookingRequest> validator
            )
        {
            _unitOfWork = unitOfWork;
            _housingService = housingService;
            _logger = logger;
            _mapper = mapper;
            _validator = validator;
        }

        public async Task<Booking> ExecuteAsync(
            CreateBookingRequest request,
            string userId,
            CancellationToken cancellationToken)
        {
            _logger.Information("Начало создания бронирования. Запрос: {@Request}, UserId: {UserId}", request, userId);

            var userIdParseAttempt = Guid.TryParse(userId, out var userGuid);

            if (!userIdParseAttempt)
            {
                _logger.Warning("Некорректный формат UserId: {UserId}", userId);

                throw new ArgumentException("Некорректный формат ID.");
            }

            await _validator.ValidateAndThrowAsync(request);

            _logger.Information("Получение информации о жилье для HousingId: {HousingId}", request.HousingId);

            var housingResponse = await _housingService.GetHousingInfoAsync(request.HousingId);

            _logger.Information("Информация о жилье получена. Цена за ночь: {PricePerNight}", housingResponse.PricePerNight);

            if (userGuid == housingResponse.OwnerId)
            {
                _logger.Warning("Пользователь попытался забронировать свое же жилье.");

                throw new ArgumentException("Извините, но забронировать свою же собственность невозможно.");
            }

            var nights = (request.EndDate.Date - request.StartDate.Date).Days;

            _logger.Information("Количество ночей: {Nights}", nights);

            var computedTotalPrice = housingResponse.PricePerNight * nights;

            _logger.Information("Общая стоимость бронирования: {TotalPrice}", computedTotalPrice);

            var booking = _mapper.Map<Booking>(request, opt => {
                opt.Items["ComputedTotalPrice"] = computedTotalPrice;
                opt.Items["UserId"] = userGuid;
            });

            //TODO: Отправить сообщение собственнику жилья о создании заявки по его объявлению.

            _logger.Information("Создан объект бронирования: {@Booking}", booking);

            await _unitOfWork.Bookings.AddAsync(booking, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.Information("Бронирование успешно сохранено в базе. Id: {BookingId}", booking.BookingId);

            return booking;
        }
    }
}
