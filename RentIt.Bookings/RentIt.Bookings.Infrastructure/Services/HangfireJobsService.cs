using Hangfire;
using Hangfire.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RentIt.Bookings.Application.Interfaces.Services;

namespace RentIt.Bookings.Infrastructure.Services
{
    public static class HangfireJobsService
    {
        public static void ConfigureHangfireJobs(IApplicationBuilder app)
        {
            var recurringJobManager = app.ApplicationServices.GetService<IRecurringJobManager>();
            if (recurringJobManager == null)
            {
                throw new InvalidOperationException("IRecurringJobManager is not registered. Ensure AddHangfire is called in ConfigureServices.");
            }

            recurringJobManager.AddOrUpdate(
                "UpdatePendingBookingsJob",
                Job.FromExpression<IBookingStatusService>(service => service.UpdatePendingBookingsAsync(CancellationToken.None)),
                Cron.Hourly(),
                new RecurringJobOptions { TimeZone = TimeZoneInfo.Local }
            );

            recurringJobManager.AddOrUpdate(
                "UpdateActiveBookingsJob",
                Job.FromExpression<IBookingStatusService>(service => service.UpdateActiveBookingsAsync(CancellationToken.None)),
                Cron.Hourly(),
                new RecurringJobOptions { TimeZone = TimeZoneInfo.Local }
            );

            recurringJobManager.AddOrUpdate(
                "UpdatePaidBookingsJob",
                Job.FromExpression<IBookingStatusService>(service => service.UpdatePaidBookingsAsync(CancellationToken.None)),
                Cron.Hourly(),
                new RecurringJobOptions { TimeZone = TimeZoneInfo.Local }
            );

            recurringJobManager.AddOrUpdate(
                "UpdateConfirmedBookingsJob",
                Job.FromExpression<IBookingStatusService>(service => service.UpdateConfirmedBookingsAsync(CancellationToken.None)),
                Cron.Hourly(),
                new RecurringJobOptions { TimeZone = TimeZoneInfo.Local }
            );
        }
    }
}
