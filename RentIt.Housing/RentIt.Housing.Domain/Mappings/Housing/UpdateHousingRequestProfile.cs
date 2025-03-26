using AutoMapper;
using RentIt.Housing.DataAccess.Entities;
using RentIt.Housing.Domain.Contracts.Requests.Housing;

namespace RentIt.Housing.Domain.Mappings.Housing
{
    public class UpdateHousingRequestProfile : Profile
    {
        public UpdateHousingRequestProfile()
        {
            CreateMap<UpdateHousingRequest, HousingEntity>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
