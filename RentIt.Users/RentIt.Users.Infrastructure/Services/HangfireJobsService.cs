using Hangfire;
using RentIt.Users.Application.Interfaces;

namespace RentIt.Users.Infrastructure.Services
{
    public static class HangfireJobsService
    {
        public static void ConfigureHangfireJobs()
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