using AutoMapper;
using RentIt.Housing.DataAccess.Entities;
using RentIt.Housing.Domain.Contracts.Requests.Reviews;

namespace RentIt.Housing.Domain.Mappings.Reviews
{
    public class CreateReviewRequestProfile : Profile
    {
        public CreateReviewRequestProfile()
        {
            CreateMap<CreateReviewRequest, Review>()
                .ForMember(dest => dest.ReviewId, opt => opt.MapFrom(_ => Guid.NewGuid()))
                .ForMember(dest => dest.HousingId, opt => opt.MapFrom((src, dest, destMember, context) =>
                    (Guid)context.Items["housingId"]))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom((src, dest, destMember, context) =>
                    (Guid)context.Items["userId"]))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
        }
    }
}