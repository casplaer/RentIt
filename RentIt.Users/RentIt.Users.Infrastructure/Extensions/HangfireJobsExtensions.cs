using Hangfire;
using RentIt.Users.Infrastructure.Services;

namespace RentIt.Users.Infrastructure.Extensions
{
    public static class HangfireJobsExtensions
    {
        public static void ConfigureRecurringJobs()
        {
            RecurringJob.AddOrUpdate<TokenCleanupService>(
                recurringJobId: "ConfirmationTokenCleanupJob",
                methodCall: service => service.CleanExpiredConfirmationTokensAsync(CancellationToken.None),
                cronExpression: () => Cron.Daily(),
                options: new RecurringJobOptions { TimeZone = TimeZoneInfo.Local }
            );

            RecurringJob.AddOrUpdate<TokenCleanupService>(
                recurringJobId: "ResetTokenCleanupJob",
                methodCall: service => service.CleanExpiredResetTokensAsync(CancellationToken.None),
                cronExpression: () => Cron.Daily(),
                options: new RecurringJobOptions { TimeZone = TimeZoneInfo.Local }
            );
        }
    }
}
