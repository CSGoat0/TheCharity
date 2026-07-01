using TheCharityBLL.Jobs.Base;
using TheCharityBLL.Jobs.Context;
using TheCharityBLL.Jobs.Result.Abstraction;
using TheCharityBLL.Jobs.Result.Implementation;
using TheCharityBLL.Services.Abstraction;
using TheCharityBLL.Services.Enums;

namespace TheCharityBLL.Jobs.Emails
{
    public class SendMilestoneEmailJob : BaseJob
    {
        private readonly ICampaignService _campaignService;
        private readonly ICampaignNotificationService _notificationService;

        public SendMilestoneEmailJob(
            ICampaignService campaignService,
            ICampaignNotificationService notificationService)
        {
            _campaignService = campaignService;
            _notificationService = notificationService;
        }

        public override string JobName => "Send milestone celebration email";

        public override async Task<IJobResult> ExecuteAsync(JobContext context)
        {
            var campaignId = context.GetMetadata<int>("CampaignId");
            var milestone = context.GetMetadata<int>("Milestone");

            var campaign = await _campaignService.GetCampaignByIdAsync(campaignId);
            if (!campaign.Success || campaign.Data == null)
                return JobResult.Failure($"Campaign {campaignId} not found");

            var subject = $"🎉 {milestone}% Milestone Reached! - {campaign.Data.Title}";
            var message =
                $"🎊 Congratulations!\n\n" +
                $"Your campaign '{campaign.Data.Title}' has reached {milestone}% of its target!\n\n" +
                $"📊 Current Progress:\n" +
                $"   • Target: ${campaign.Data.Target:F2}\n" +
                $"   • Raised: ${campaign.Data.Achieved:F2}\n" +
                $"   • Achievement: {milestone}%\n\n" +
                $"Keep up the great work!";

            await _notificationService.SendCampaignNotificationAsync(
                campaignId,
                subject,
                message,
                NotificationType.MilestoneReached);

            return JobResult.Success($"Sent {milestone}% milestone email for campaign {campaignId}");
        }
    }
}
