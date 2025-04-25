using Hangfire;
using RentIt.Housing.Domain.Services.Interfaces;

namespace RentIt.Housing.Domain.Services
{
    public static class HangfireJobsService
    {
        public static void ConfigureRecurringJobs()
        {
            RecurringJob.AddOrUpdate<IHousingService>(
                recurringJobId: "CheckHousingsForSpamAndProfanity",
                methodCall: service => service.CheckUnpublishedHousingsForSpamAsync(new CancellationToken()),
                cronExpression: () => Cron.Daily(),
                options: new RecurringJobOptions { TimeZone = TimeZoneInfo.Local }
                );
        }
    }
}
