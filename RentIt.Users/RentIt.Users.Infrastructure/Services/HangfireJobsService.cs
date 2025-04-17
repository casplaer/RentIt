using Hangfire;
using Hangfire.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RentIt.Users.Application.Interfaces;

namespace RentIt.Users.Infrastructure.Services
{
    public static class HangfireJobsService
    {
        public static void ConfigureHangfireJobs(IApplicationBuilder app)
        {
            var recurringJobManager = app.ApplicationServices.GetService<IRecurringJobManager>();
            if (recurringJobManager == null)
            {
                throw new InvalidOperationException("IRecurringJobManager не зарегистрирован. Убедитесь, что AddHangfire вызван в конфигурации сервисов.");
            }

            recurringJobManager.AddOrUpdate(
                "ConfirmationTokenCleanupJob",
                Job.FromExpression<ITokenCleanupService>(service => service.CleanExpiredConfirmationTokensAsync(CancellationToken.None)),
                Cron.Daily(),
                new RecurringJobOptions { TimeZone = TimeZoneInfo.Local }
            );

            recurringJobManager.AddOrUpdate(
                "ResetTokenCleanupJob",
                Job.FromExpression<ITokenCleanupService>(service => service.CleanExpiredResetTokensAsync(CancellationToken.None)),
                Cron.Daily(),
                new RecurringJobOptions { TimeZone = TimeZoneInfo.Local }
            );
        }
    }
}