using Microsoft.Extensions.DependencyInjection;
using RentIt.Bookings.Application.Interfaces.EventBus;
using RentIt.Bookings.Application.Interfaces.Services;
using RentIt.Bookings.Application.Interfaces.Services.Grpc;
using RentIt.Bookings.Core.Interfaces.Repositories;
using RentIt.Bookings.Infrastructure.MessageBroker;
using RentIt.Bookings.Infrastructure.Repositories;
using RentIt.Bookings.Infrastructure.Services.Grpc;
using RentIt.Users.Infrastructure.Services;

namespace RentIt.Bookings.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationRepositories(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }

        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddTransient<IEventBus, EventBus>();
            services.AddTransient<IHousingIntegrationService, HousingIntegrationService>();
            services.AddTransient<IUserIntegrationService, UserIntegrationService>();
            services.AddTransient<IEmailSender, EmailSender>();

            return services;
        }
    }
}
