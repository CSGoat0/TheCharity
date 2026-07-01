using TheCharityBLL.Jobs.Base;
using TheCharityBLL.Jobs.Context;
using TheCharityBLL.Jobs.Result.Abstraction;
using TheCharityBLL.Jobs.Result.Implementation;
using TheCharityBLL.Services.Abstraction;
using TheCharityBLL.Services.Enums;

namespace TheCharityBLL.Jobs.Emails
{
    public class DeadlineExtensionConfirmationJob : BaseJob
    {
        private readonly ICampaignService _campaignService;
        private readonly ICampaignNotificationService _notificationService;

        public DeadlineExtensionConfirmationJob(
            ICampaignService campaignService,
            ICampaignNotificationService notificationService)
        {
            _campaignService = campaignService;
            _notificationService = notificationService;
        }

        public override string JobName => "Send deadline extension confirmation";

        public override async Task<IJobResult> ExecuteAsync(JobContext context)
        {
            var campaignId = context.GetMetadata<int>("CampaignId");

            var campaign = await _campaignService.GetCampaignByIdAsync(campaignId);
            if (!campaign.Success || campaign.Data == null)
                return JobResult.Failure($"Campaign {campaignId} not found");

            var subject = $"📅 Campaign Deadline Extended: {campaign.Data.Title}";
            var message =
                $"The deadline for '{campaign.Data.Title}' has been extended.\n\n" +
                $"📅 New Deadline: {campaign.Data.Deadline:yyyy-MM-dd}\n" +
                $"💰 Current Progress: ${campaign.Data.Achieved:F2} / ${campaign.Data.Target:F2}\n\n" +
                $"You now have more time to reach your target!";

            await _notificationService.SendCampaignNotificationAsync(
                campaignId,
                subject,
                message,
                NotificationType.DeadlineExtended);

            return JobResult.Success($"Sent deadline extension confirmation for campaign {campaignId}");
        }
    }
}
