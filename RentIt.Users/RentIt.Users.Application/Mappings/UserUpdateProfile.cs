using AutoMapper;
using RentIt.Users.Application.Commands.Users.Update;
using RentIt.Users.Core.Entities;

namespace RentIt.Users.Application.Mappings
{
    public class UserUpdateProfile : Profile
    {
        public UserUpdateProfile()
        {
            CreateMap<UpdateUserCommand, User>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.Profile, opt => opt.MapFrom(src => new UserProfile
                {
                    PhoneNumber = src.PhoneNumber,
                    Address = src.Address,
                    City = src.City,
                    Country = src.Country,
                }));
        }
    }
}
