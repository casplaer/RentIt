using AutoMapper;
using RentIt.Users.Contracts.Dto.Users;
using RentIt.Users.Core.Entities;

namespace RentIt.Users.Application.Mappings
{
    public class UserDtoProfile : Profile
    {
        public UserDtoProfile()
        {
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.RoleName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Profile.PhoneNumber))
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Profile.Country))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.Profile.City));
        }
    }
}
