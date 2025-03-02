using AutoMapper;
using RentIt.Users.Contracts.DTO.Users;
using RentIt.Users.Core.Entities;

namespace RentIt.Users.Application.Mappings
{
    public class UserDTOProfile : Profile
    {
        public UserDTOProfile()
        {
            CreateMap<User, UserDTO>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.RoleName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Profile.PhoneNumber))
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Profile.Country))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.Profile.City));
        }
    }
}
