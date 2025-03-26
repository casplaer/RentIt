using AutoMapper;
using RentIt.Housing.DataAccess.Entities;
using RentIt.Housing.DataAccess.Enums;
using RentIt.Housing.Domain.Contracts.Requests.Housing;

namespace RentIt.Housing.Domain.Mappings.Housing
{
    public class HousingProfile : Profile
    {
        public HousingProfile()
        {
            CreateMap<CreateHousingRequest, HousingEntity>()
                .ForMember(dest => dest.HousingId, opt => opt.MapFrom(_ => Guid.NewGuid()))
                .ForMember(dest => dest.OwnerId, opt => opt.MapFrom((src, dest, destMember, context) =>
                    (Guid)context.Items["ownerId"]))
                .ForMember(dest => dest.Rating, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => HousingStatus.Unpublished))
                .ForMember(dest => dest.Images, opt => opt.Ignore())
                .ForMember(dest => dest.Reviews, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
        }
    }
}