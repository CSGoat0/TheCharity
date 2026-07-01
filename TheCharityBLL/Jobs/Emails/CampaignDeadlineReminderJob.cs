using TheCharityBLL.Jobs.Base;
using TheCharityBLL.Jobs.Context;
using TheCharityBLL.Jobs.Result.Abstraction;
using TheCharityBLL.Jobs.Result.Implementation;
using TheCharityBLL.Services.Abstraction;
using TheCharityBLL.Services.Enums;

namespace TheCharityBLL.Jobs.Emails
{
    public class CampaignDeadlineReminderJob : BaseJob
    {
        private readonly ICampaignService _campaignService;
        private readonly ICampaignNotificationService _notificationService;

        public CampaignDeadlineReminderJob(
            ICampaignService campaignService,
            ICampaignNotificationService notificationService)
        {
            _campaignService = campaignService;
            _notificationService = notificationService;
        }

        public override string JobName => "Send campaign deadline reminders";

        public override async Task<IJobResult> ExecuteAsync(JobContext context)
        {
            var expiringCampaigns = await _campaignService.GetCampaignsExpiringSoonAsync(7);

            if (!expiringCampaigns.Success || expiringCampaigns.Data == null)
                return JobResult.Failure("Failed to get expiring campaigns");

            var remindersSent = 0;

            foreach (var campaign in expiringCampaigns.Data)
            {
                var daysLeft = campaign.Deadline.HasValue ? (campaign.Deadline.Value - DateTime.UtcNow).Days : 0;
                var percentage = campaign.Target > 0 ? (campaign.Achieved / campaign.Target) * 100 : 0;

                var subject = $"⚠️ Campaign Ending Soon: {campaign.Title}";
                var message =
                    $"Your campaign '{campaign.Title}' ends in {daysLeft} days.\n" +
                    $"Current progress: {percentage:F1}% (${campaign.Achieved:F2} / ${campaign.Target:F2})\n" +
                    $"Visit your dashboard to extend the deadline if needed.";

                await _notificationService.SendCampaignNotificationAsync(
                    campaign.Id,
                    subject,
                    message,
                    NotificationType.DeadlineReminder);

                remindersSent++;
            }

            return JobResult.Success($"Sent {remindersSent} deadline reminders");
        }
    }
}
