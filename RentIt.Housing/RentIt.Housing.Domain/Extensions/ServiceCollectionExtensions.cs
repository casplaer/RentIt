using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using RentIt.Housing.Domain.Contracts.Requests.Housing;
using RentIt.Housing.Domain.Contracts.Requests.Reviews;
using RentIt.Housing.Domain.Mappings.Housing;
using RentIt.Housing.Domain.Mappings.Reviews;
using RentIt.Housing.Domain.Services;
using RentIt.Housing.Domain.Services.Grpc;
using RentIt.Housing.Domain.Services.Interfaces;
using RentIt.Housing.Domain.Services.MessageBroker;
using RentIt.Housing.Domain.Validators;

namespace RentIt.Housing.Domain.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMappingProfiles(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(HousingProfile));
            services.AddAutoMapper(typeof(UpdateHousingRequestProfile));
            services.AddAutoMapper(typeof(CreateReviewRequestProfile));
            services.AddAutoMapper(typeof(UpdateReviewRequestProfile));

            return services;
        }

        public static IServiceCollection AddValidators(this IServiceCollection services)
        {
            services.AddScoped<IValidator<CreateHousingRequest>, CreateHousingRequestValidator>();
            services.AddScoped<IValidator<GetFilteredHousingsRequest>, GetFilteredHousingsRequestValidator>();
            services.AddScoped<IValidator<UpdateHousingRequest>, UpdateHousingRequestValidator>();
            services.AddScoped<IValidator<CreateReviewRequest>, CreateReviewRequestValidator>();
            services.AddScoped<IValidator<UpdateReviewRequest>, UpdateReviewRequestValidator>();

            return services;
        }

        public static IServiceCollection AddDomainServices(this IServiceCollection services)
        {
            services.AddScoped<IHousingService, HousingService>();
            services.AddScoped<IHousingImageService, HousingImageService>();
            services.AddScoped<IFileStorageService, FileStorageService>();
            services.AddScoped<IReviewsService, ReviewsService>();
            services.AddScoped<IUserIntegrationService, UserIntegrationService>();
            services.AddScoped<IBookingIntegrationService, BookingIntegrationService>();
            services.AddScoped<HousingGrpcService>();
            services.AddScoped<ISpamProfanityFilterService, SpamProfanityFilterService>();
            services.AddTransient<IEventBus, EventBus>();

            return services;
        }
    }
}