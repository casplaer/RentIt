using Microsoft.Extensions.DependencyInjection;
using RentIt.Bookings.Core.Interfaces.Repositories;
using RentIt.Bookings.Infrastructure.Repositories;

namespace RentIt.Bookings.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationRepositories(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
