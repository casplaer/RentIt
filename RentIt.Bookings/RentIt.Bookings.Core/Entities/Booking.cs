using RentIt.Bookings.Core.Enums;

namespace RentIt.Bookings.Core.Entities
{
    public class Booking
    {
        public Guid BookingId { get; set; }            
        public Guid HousingId { get; set; }    
        public Guid UserId { get; set; }  
        public DateTime StartDate { get; set; }   
        public DateTime EndDate { get; set; }      
        public decimal TotalPrice { get; set; }       
        public BookingStatus Status { get; set; }    
        public DateTime CreatedAt { get; set; }     
        public DateTime UpdatedAt { get; set; }      
        public Payment Payment { get; set; }
    }
}
