using Hangfire;
using Microsoft.Extensions.Logging;
using TheCharityBLL.Jobs.Emails;
using TheCharityBLL.Jobs.Registry.Abstraction;
using TheCharityBLL.Services.Abstraction;

namespace TheCharityBLL.Jobs.Registry.Implementation
{
    public class JobRegistry : IJobRegistry
    {
        private readonly IJobSchedulerService _jobScheduler;
        private readonly ILogger<JobRegistry> _logger;

        public JobRegistry(IJobSchedulerService jobScheduler, ILogger<JobRegistry> logger)
        {
            _jobScheduler = jobScheduler;
            _logger = logger;
        }

        public void RegisterAllRecurringJobs()
        {
            _logger.LogInformation("Registering all recurring jobs...");

            // Daily at 9 AM - deadline reminders
            _jobScheduler.AddOrUpdateRecurringJob<CampaignDeadlineReminderJob>(
                "campaign-deadline-reminders",
                Cron.Daily(9)
            );

            // Hourly - auto-expire campaigns
            _jobScheduler.AddOrUpdateRecurringJob<AutoExpireCampaignsJob>(
                "auto-expire-campaigns",
                Cron.Hourly()
            );

            // Weekly on Monday at 7 AM - digest
            _jobScheduler.AddOrUpdateRecurringJob<WeeklyCampaignDigestJob>(
                "weekly-campaign-digest",
                Cron.Weekly(DayOfWeek.Monday, 7)
            );

            // Add more recurring jobs here as you create them

            _logger.LogInformation("All recurring jobs registered successfully");
        }
    }
}
