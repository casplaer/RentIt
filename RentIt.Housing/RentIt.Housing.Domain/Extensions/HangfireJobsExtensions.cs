using Hangfire;
using RentIt.Housing.Domain.Services;

namespace RentIt.Housing.Domain.Extensions
{
    public static class HangfireJobsExtensions
    {
        public static void ConfigureRecurringJobs()
        {
            RecurringJob.AddOrUpdate<HousingService>(
                recurringJobId: "CheckHousingsForSpamAndProfanity",
                methodCall: service => service.CheckUnpublishedHousingsForSpamAsync(new CancellationToken()),
                cronExpression: () => Cron.Daily(),
                options: new RecurringJobOptions { TimeZone = TimeZoneInfo.Local }
                );
        }
    }
}
