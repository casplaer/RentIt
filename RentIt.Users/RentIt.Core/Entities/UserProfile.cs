namespace RentIt.Users.Core.Entities
{
    public class UserProfile
    {
        public Guid UserId { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public User User { get; set; }
    }
}
