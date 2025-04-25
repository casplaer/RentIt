using AutoMapper;
using RentIt.Bookings.Contracts.Dto;
using RentIt.Bookings.Core.Entities;
using RentIt.Bookings.Core.Interfaces.Repositories;

namespace RentIt.Bookings.Application.Mappings.Bookings
{
    public class BookingDtoProfile : Profile
    {
        public BookingDtoProfile()
        {
            CreateMap<Booking, BookingDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<PaginatedResult<Booking>, PaginatedResult<BookingDto>>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));
        }
    }
}
