namespace RentIt.Users.Application.Interfaces
{
    public interface ITokenCleanupService
    {
        Task CleanExpiredConfirmationTokensAsync(CancellationToken cancellationToken);
        Task CleanExpiredResetTokensAsync(CancellationToken cancellationToken);
    }
}