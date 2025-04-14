using AutoMapper;
using RentIt.Bookings.Contracts.Requests.Bookings;
using RentIt.Bookings.Core.Entities;
using RentIt.Bookings.Core.Enums;

namespace RentIt.Bookings.Application.Mappings.Bookings
{
    public class UpdateBookingProfile : Profile
    {
        public UpdateBookingProfile()
        {
            CreateMap<UpdateBookingRequest, Booking>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<BookingStatus>(src.Status)))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom((src, dest, destMember, context) =>
                {
                    return context.Items.ContainsKey("ComputedTotalPrice")
                    ? (decimal)context.Items["ComputedTotalPrice"]
                    : 0m;
                }));
        }
    }
}
