using AutoMapper;
using RentIt.Housing.DataAccess.Entities;
using RentIt.Availabilities.Domain.Contracts.Dto.Availabilities;

namespace RentIt.Housing.Domain.Mappings.Availabilities
{
    public class AvailabilityProfile : Profile
    {
        public AvailabilityProfile()
        {
            CreateMap<AvailabilityDto, Availability>()
                .ForMember(dest => dest.AvailabilityId, opt => opt.MapFrom(_ => Guid.NewGuid()))
                .ForMember(dest => dest.HousingId, opt => opt.MapFrom((src, dest, destMember, context) =>
                    (Guid)context.Items["housingId"]));
        }
    }
}
