using AutoMapper;
using FluentValidation;
using RentIt.Bookings.Application.Interfaces.UseCases.Bookings;
using RentIt.Bookings.Application.Services.Grpc;
using RentIt.Bookings.Contracts.Requests.Bookings;
using RentIt.Bookings.Core.Entities;
using RentIt.Bookings.Core.Interfaces.Repositories;
using RentIt.Protos.Housing;
using Serilog;

namespace RentIt.Bookings.Application.UseCases.Bookings
{
    public class UpdateBookingUseCase : IUpdateBookingUseCase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<UpdateBookingRequest> _validator;
        private readonly ILogger _logger;
        private readonly HousingIntegrationsService _housingService;

        public UpdateBookingUseCase(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<UpdateBookingRequest> validator,
            ILogger logger,
            HousingIntegrationsService housingService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _validator = validator;
            _logger = logger;
            _housingService = housingService;
        }

        public async Task<Booking> ExecuteAsync(
            Guid bookingId, 
            UpdateBookingRequest request, 
            CancellationToken cancellationToken)
        {
            _logger.Information("Начало обновления бронирования с ID: {BookingId}", bookingId);

            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId, cancellationToken);
            if (booking == null)
            {
                _logger.Warning("Бронирование с ID {BookingId} не найдено.", bookingId);

                throw new Exception("Бронирование не найдено");
            }

            await _validator.ValidateAndThrowAsync(request);

            var housingResponse = await _housingService.GetHousingInfoAsync(request.HousingId);

            int nights = (request.EndDate.Date - request.StartDate.Date).Days;

            _logger.Information("Количество ночей: {Nights}", nights);

            decimal computedTotalPrice = housingResponse.PricePerNight * nights;

            _logger.Information("Общая стоимость бронирования: {TotalPrice}", computedTotalPrice);

            _mapper.Map(request, booking, opt => {
                opt.Items["ComputedTotalPrice"] = computedTotalPrice;
            });

            _unitOfWork.Bookings.Update(booking);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.Information("Бронирование успешно обновлено.");

            return booking;
        }
    }
}