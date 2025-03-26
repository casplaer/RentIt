using AutoMapper;
using RentIt.Housing.DataAccess.Entities;
using RentIt.Housing.Domain.Contracts.Requests.Reviews;

namespace RentIt.Housing.Domain.Mappings.Reviews
{
    public class UpdateReviewRequestProfile : Profile
    {
        public UpdateReviewRequestProfile()
        {
            CreateMap<UpdateReviewRequest, Review>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
        }
    }
}
