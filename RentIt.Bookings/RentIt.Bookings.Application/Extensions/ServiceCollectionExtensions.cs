using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using RentIt.Bookings.Application.Interfaces.UseCases.Bookings;
using RentIt.Bookings.Application.Interfaces.UseCases.Payments;
using RentIt.Bookings.Application.Mappings.Bookings;
using RentIt.Bookings.Application.Services.Grpc;
using RentIt.Bookings.Application.UseCases.Bookings;
using RentIt.Bookings.Application.UseCases.Payments;
using RentIt.Bookings.Application.Validators;
using RentIt.Bookings.Contracts.Requests.Bookings;

namespace RentIt.Bookings.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddValidators(this IServiceCollection services)
        {
            services.AddScoped<IValidator<CreateBookingRequest>, CreateBookingRequestValidator>();
            services.AddScoped<IValidator<UpdateBookingRequest>, UpdateBookingRequestValidator>();

            return services;
        }

        public static IServiceCollection AddApplicationUseCases(this IServiceCollection services)
        {
            services.AddScoped<IGetBookingUseCase, GetBookingUseCase>();
            services.AddScoped<IGetBookingsByUserIdUseCase, GetBookingsByUserIdUseCase>();
            services.AddScoped<IGetBookingsByHousingIdUseCase, GetBookingsByHousingIdUseCase>();
            services.AddScoped<IGetConfirmedBookingsByHousingIdUseCase, GetConfirmedBookingsByHousingIdUseCase>();
            services.AddScoped<IConfirmBookingUseCase, ConfirmBookingUseCase>();
            services.AddScoped<IRejectBookingUseCase, RejectBookingUseCase>();
            services.AddScoped<ICancelBookingUseCase, CancelBookingUseCase>();
            services.AddScoped<IAdminCancelBookingUseCase, AdminCancelBookingUseCase>();
            services.AddScoped<IDeleteBookingUseCase, DeleteBookingUseCase>();
            services.AddScoped<IAddBookingUseCase, AddBookingUseCase>();
            services.AddScoped<IUpdateBookingUseCase, UpdateBookingUseCase>();
            services.AddScoped<ICreatePaymentUseCase, CreatePaymentUseCase>();
            services.AddScoped<IConfirmPaymentUseCase, ConfirmPaymentUseCase>();
            services.AddScoped<IRefundPaymentUseCase, RefundPaymentUseCase>();


            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<HousingIntegrationsService>();
            services.AddScoped<UserIntegrationService>();

            return services;
        }

        public static IServiceCollection MapAllProfiles(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(BookingDtoProfile));
            services.AddAutoMapper(typeof(CreateBookingProfile));
            services.AddAutoMapper(typeof(UpdateBookingProfile));

            return services;
        }
    }
}
