namespace RentIt.Users.Core.Entities
{
    public class UserProfile
    {
        public Guid UserId { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public DateTime CreatedAt { get; set; }
        public User User { get; set; }
    }
}
