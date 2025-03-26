using Microsoft.EntityFrameworkCore;
using RentIt.Users.Core.Entities;
using RentIt.Users.Core.Enums;
using RentIt.Users.Core.Interfaces.Repositories;
using RentIt.Users.Infrastructure.Data;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;

namespace RentIt.Users.Infrastructure.Repositories
{
    public class AccountTokenRepository : Repository<AccountToken>, IAccountTokenRepository
    {
        public AccountTokenRepository(RentItDbContext context)
            : base(context) { }

        public async Task<IEnumerable<AccountToken>> GetExpiredConfirmationTokensAsync(CancellationToken cancellationToken)
        {
            return await _context.AccountTokens
                .Where(token => token.TokenType == TokenType.Confirmation && token.Expiration < DateTime.UtcNow)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<AccountToken>> GetExpiredResetTokensAsync(CancellationToken cancellationToken)
        {
            return await _context.AccountTokens
                .Where(token => token.TokenType == TokenType.PasswordReset && token.Expiration < DateTime.UtcNow)
                .ToListAsync(cancellationToken);
        }

        public async Task<AccountToken> GetTokenAsync(Guid userId, string token, TokenType tokenType, CancellationToken cancellationToken)
        {
            return await _context.AccountTokens
                .Where(t => t.UserId == userId &&
                            t.Token == token &&
                            t.TokenType == tokenType &&
                            t.Expiration > DateTime.UtcNow)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
