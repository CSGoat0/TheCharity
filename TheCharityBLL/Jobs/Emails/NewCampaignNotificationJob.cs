using TheCharityBLL.Jobs.Base;
using TheCharityBLL.Jobs.Context;
using TheCharityBLL.Jobs.Result.Abstraction;
using TheCharityBLL.Jobs.Result.Implementation;
using TheCharityBLL.Services.Abstraction;
using TheCharityBLL.Services.Enums;

namespace TheCharityBLL.Jobs.Emails
{
    public class NewCampaignNotificationJob : BaseJob
    {
        private readonly ICampaignService _campaignService;
        private readonly ICampaignNotificationService _notificationService;

        public NewCampaignNotificationJob(
            ICampaignService campaignService,
            ICampaignNotificationService notificationService)
        {
            _campaignService = campaignService;
            _notificationService = notificationService;
        }

        public override string JobName => "Send new campaign notification";

        public override async Task<IJobResult> ExecuteAsync(JobContext context)
        {
            var campaignId = context.GetMetadata<int>("CampaignId");

            var campaign = await _campaignService.GetCampaignByIdAsync(campaignId);
            if (!campaign.Success || campaign.Data == null)
                return JobResult.Failure($"Campaign {campaignId} not found");

            var subject = $"🚀 New Campaign Created: {campaign.Data.Title}";
            var message =
                $"A new campaign has been created!\n\n" +
                $"📌 Title: {campaign.Data.Title}\n" +
                $"📝 Description: {campaign.Data.Description ?? "No description provided"}\n" +
                $"🎯 Target: ${campaign.Data.Target:F2}\n" +
                $"💰 Current Progress: ${campaign.Data.Achieved:F2}\n" +
                $"📅 Deadline: {campaign.Data.Deadline:yyyy-MM-dd}\n\n" +
                $"Visit the dashboard to manage this campaign.";

            await _notificationService.SendCampaignNotificationAsync(
                campaignId,
                subject,
                message,
                NotificationType.CampaignCreated);

            return JobResult.Success($"Sent new campaign notification for campaign {campaignId}");
        }
    }
}
