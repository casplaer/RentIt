using AutoMapper;
using RentIt.Housing.DataAccess.Entities;
using RentIt.Housing.Domain.Contracts.Requests.Housing;

namespace RentIt.Housing.Domain.Mappings.Housing
{
    public class HousingProfile : Profile
    {
        public HousingProfile()
        {
            CreateMap<CreateHousingRequest, HousingEntity>()
                .ForMember(dest => dest.HousingId, opt => opt.Ignore())
                .ForMember(dest => dest.Rating, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.Images, opt => opt.Ignore())
                .ForMember(dest => dest.Reviews, opt => opt.Ignore())
                .ForMember(dest => dest.Availabilities, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        }
    }
}
