using AutoMapper;
using RentIt.Bookings.Contracts.Requests.Bookings;
using RentIt.Bookings.Core.Entities;
using RentIt.Bookings.Core.Enums;

namespace RentIt.Bookings.Application.Mappings.Bookings
{
    public class CreateBookingProfile : Profile
    {
        public CreateBookingProfile()
        {
            CreateMap<CreateBookingRequest, Booking>()
                .ForMember(dest => dest.BookingId, opt => opt.MapFrom(_ => Guid.NewGuid()))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom((src, dest, destMember, context) =>
                {
                    return context.Items.ContainsKey("UserId")
                        ? context.Items["UserId"] 
                        : null;
                }))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => BookingStatus.Pending))
                .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom((src, dest, destMember, context) =>
                {
                    return context.Items.ContainsKey("ComputedTotalPrice")
                        ? (decimal)context.Items["ComputedTotalPrice"]
                        : 0m;
                }));
        }
    }
}
