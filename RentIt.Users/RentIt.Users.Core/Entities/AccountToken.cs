using RentIt.Users.Core.Enums;

namespace RentIt.Users.Core.Entities
{
    public class AccountToken
    {
        public Guid TokenId { get; set; }
        public Guid UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public TokenType TokenType { get; set; }
    }
}
