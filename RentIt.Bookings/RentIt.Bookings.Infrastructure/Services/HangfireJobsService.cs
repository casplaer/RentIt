using Hangfire;
using RentIt.Bookings.Application.Interfaces.Services;

namespace RentIt.Bookings.Infrastructure.Services
{
    public static class HangfireJobsService
    {
        public static void ConfigureHangfireJobs()
        {
            RecurringJob.AddOrUpdate<IBookingStatusService>(
                recurringJobId: "UpdatePendingBookingsJob",
                methodCall: service => service.UpdatePendingBookingsAsync(CancellationToken.None),
                cronExpression: () => Cron.Hourly(),
                options: new RecurringJobOptions { TimeZone = TimeZoneInfo.Local }
            );

            RecurringJob.AddOrUpdate<IBookingStatusService>(
                recurringJobId: "UpdateActiveBookingsJob",
                methodCall: service => service.UpdateActiveBookingsAsync(CancellationToken.None),
                cronExpression: () => Cron.Hourly(),
                options: new RecurringJobOptions { TimeZone = TimeZoneInfo.Local }
            );

            RecurringJob.AddOrUpdate<IBookingStatusService>(
                recurringJobId: "UpdatePaidBookingsJob",
                methodCall: service => service.UpdatePaidBookingsAsync(CancellationToken.None),
                cronExpression: () => Cron.Hourly(),
                options: new RecurringJobOptions { TimeZone = TimeZoneInfo.Local }
            );

            RecurringJob.AddOrUpdate<IBookingStatusService>(
                recurringJobId: "UpdateConfirmedBookingsJob",
                methodCall: service => service.UpdateConfirmedBookingsAsync(CancellationToken.None),
                cronExpression: () => Cron.Hourly(),
                options: new RecurringJobOptions { TimeZone = TimeZoneInfo.Local }
            );
        }
    }
}
