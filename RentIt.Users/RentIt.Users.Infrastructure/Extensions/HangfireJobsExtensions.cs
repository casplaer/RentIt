using Hangfire;
using RentIt.Users.Application.Interfaces;

namespace RentIt.Users.Infrastructure.Extensions
{
    public static class HangfireJobsExtensions
    {
        public static void ConfigureRecurringJobs()
        {
            RecurringJob.AddOrUpdate<ITokenCleanupService>(
                recurringJobId: "ConfirmationTokenCleanupJob",
                methodCall: service => service.CleanExpiredConfirmationTokensAsync(CancellationToken.None),
                cronExpression: () => Cron.Daily(),
                options: new RecurringJobOptions { TimeZone = TimeZoneInfo.Local }
            );

            RecurringJob.AddOrUpdate<ITokenCleanupService>(
                recurringJobId: "ResetTokenCleanupJob",
                methodCall: service => service.CleanExpiredResetTokensAsync(CancellationToken.None),
                cronExpression: () => Cron.Daily(),
                options: new RecurringJobOptions { TimeZone = TimeZoneInfo.Local }
            );
        }
    }
}