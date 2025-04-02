using Microsoft.Extensions.DependencyInjection;
using RentIt.Bookings.Application.Interfaces.UseCases.Bookings;
using RentIt.Bookings.Application.Interfaces.UseCases.Payments;
using RentIt.Bookings.Application.Services.Grpc;
using RentIt.Bookings.Application.UseCases.Bookings;
using RentIt.Bookings.Application.UseCases.Payments;

namespace RentIt.Bookings.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationUseCases(this IServiceCollection services)
        {
            services.AddScoped<IGetBookingUseCase, GetBookingUseCase>();
            services.AddScoped<IDeleteBookingUseCase, DeleteBookingUseCase>();
            services.AddScoped<IAddBookingUseCase, AddBookingUseCase>();
            services.AddScoped<IUpdateBookingUseCase, UpdateBookingUseCase>();
            services.AddScoped<IProcessPaymentUseCase, ProcessPaymentUseCase>();

            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<HousingIntegrationsService>();

            return services;
        }
    }
}
