using AutoMapper;
using FluentValidation;
using RentIt.Bookings.Application.Interfaces.Services;
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
        private readonly IEmailSender _emailSender;
        private readonly UserIntegrationService _userIntegrationService;

        public AddBookingUseCase(
            IUnitOfWork unitOfWork,
            HousingIntegrationsService housingService,
            ILogger logger,
            IMapper mapper,
            IValidator<CreateBookingRequest> validator,
            IEmailSender emailSender,
            UserIntegrationService userIntegrationService)
        {
            _unitOfWork = unitOfWork;
            _housingService = housingService;
            _logger = logger;
            _mapper = mapper;
            _validator = validator;
            _emailSender = emailSender;
            _userIntegrationService = userIntegrationService;
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

            _logger.Information("Создан объект бронирования: {@Booking}", booking);

            await _unitOfWork.Bookings.AddAsync(booking, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.Information("Бронирование успешно сохранено в базе. Id: {BookingId}", booking.BookingId);

            var ownerInfo = await _userIntegrationService.GetUserInfoAsync(housingResponse.OwnerId);
            if (ownerInfo == null)
            {
                _logger.Warning("Не удалось получить информацию о владельце с ID {OwnerId}", housingResponse.OwnerId);
            }
            else
            {
                var ownerEmail = ownerInfo.Email;
                var ownerMessage = $"На Ваше объявление {housingResponse.HousingName} была создана заявка с {request.StartDate:dd.MM.yyyy} по {request.EndDate:dd.MM.yyyy}." +
                    $"Просмотреть эту и остальные заявки Вы можете <a href='https://localhost:3000/my-housings/bookings'>здесь</a>.\n\n" +
                    "С уважением,\nКоманда RentIt.";

                await _emailSender.SendEmailAsync(ownerEmail, "У Вас новая заявка!", ownerMessage, cancellationToken);
                _logger.Information("Письмо отправлено владельцу жилья на почту: {OwnerEmail}", ownerEmail);
            }

            var userInfo = await _userIntegrationService.GetUserInfoAsync(userGuid);
            if (userInfo == null)
            {
                _logger.Warning("Не удалось получить информацию о пользователе с ID {UserId}", userGuid);
            }
            else
            {
                var userEmail = userInfo.Email;
                var userMessage = $"Поздравляем с созданием заявки на {housingResponse.HousingName} с {request.StartDate:dd.MM.yyyy} по {request.EndDate:dd.MM.yyyy}." +
                    $"Свяжитесь с хозяином объявления для его подтверждения и дальнейшей организации Вашего отдыха.\n\n" +
                    $"С уважением,\nКоманда RentIt.";

                await _emailSender.SendEmailAsync(userEmail, "Заявка успешно создана!", userMessage, cancellationToken);
                _logger.Information("Письмо отправлено пользователю на почту: {UserEmail}", userEmail);
            }

            return booking;
        }
    }
}
