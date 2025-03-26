using AutoMapper;
using RentIt.Users.Application.Commands.Users.Password;
using RentIt.Users.Contracts.Requests.Users;

namespace RentIt.Users.Application.Mappings
{
    public class ResetPasswordCommandProfile : Profile
    {
        public ResetPasswordCommandProfile()
        {
            CreateMap<ResetPasswordRequest, ResetPasswordCommand>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Token, opt => opt.MapFrom(src => src.Token))
            .ForMember(dest => dest.NewPassword, opt => opt.MapFrom(src => src.NewPassword))
            .ForMember(dest => dest.ConfirmPassword, opt => opt.MapFrom(src => src.ConfirmPassword));
        }
    }
}
