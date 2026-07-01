using System.Text;
using TheCharityBLL.Jobs.Base;
using TheCharityBLL.Jobs.Context;
using TheCharityBLL.Jobs.Result.Abstraction;
using TheCharityBLL.Jobs.Result.Implementation;
using TheCharityBLL.Services.Abstraction;
using TheCharityBLL.Services.Enums;

namespace TheCharityBLL.Jobs.Emails
{
    public class WeeklyCampaignDigestJob : BaseJob
    {
        private readonly ICampaignService _campaignService;
        private readonly ICampaignNotificationService _notificationService;

        public WeeklyCampaignDigestJob(
            ICampaignService campaignService,
            ICampaignNotificationService notificationService)
        {
            _campaignService = campaignService;
            _notificationService = notificationService;
        }

        public override string JobName => "Send weekly campaign digest";

        public override async Task<IJobResult> ExecuteAsync(JobContext context)
        {
            var statistics = await _campaignService.GetCampaignStatisticsAsync();
            if (!statistics.Success)
                return JobResult.Failure("Failed to get campaign statistics");

            var activeCount = await _campaignService.GetTotalActiveCampaignsCountAsync();
            var totalRaised = await _campaignService.GetTotalMoneyRaisedAsync();
            var expiringSoon = await _campaignService.GetCampaignsExpiringSoonAsync(7);
            var topCampaigns = await _campaignService.GetTopCampaignsByDonationsAsync(5);

            var digestMessage = new StringBuilder();
            digestMessage.AppendLine("📊 Weekly Campaign Digest");
            digestMessage.AppendLine(new string('=', 30));
            digestMessage.AppendLine();
            digestMessage.AppendLine($"📌 Total Campaigns: {statistics.Data?.TotalCampaigns ?? 0}");
            digestMessage.AppendLine($"📌 Active Campaigns: {activeCount.Data}");
            digestMessage.AppendLine($"📌 Completed Campaigns: {statistics.Data?.CompletedCampaigns ?? 0}");
            digestMessage.AppendLine($"📌 Total Money Raised: ${totalRaised.Data:F2}");
            digestMessage.AppendLine($"📌 Average Achievement: {statistics.Data?.AverageAchievementPercentage:F1}%");
            digestMessage.AppendLine();
            digestMessage.AppendLine($"⚠️ Campaigns expiring within 7 days: {expiringSoon.Data?.Count() ?? 0}");
            digestMessage.AppendLine();
            digestMessage.AppendLine("🏆 Top 5 Campaigns by Donations:");

            if (topCampaigns.Data != null)
            {
                var rank = 1;
                foreach (var campaign in topCampaigns.Data.Take(5))
                {
                    var percentage = campaign.Target > 0 ? (campaign.Achieved / campaign.Target) * 100 : 0;
                    digestMessage.AppendLine($"  {rank}. {campaign.Title}");
                    digestMessage.AppendLine($"     ${campaign.Achieved:F2} / ${campaign.Target:F2} ({percentage:F1}%)");
                    rank++;
                }
            }

            // Send to ALL SuperAdmins
            await _notificationService.SendSuperAdminNotificationAsync(
                "📊 Weekly Campaign Digest",
                digestMessage.ToString(),
                NotificationType.WeeklyDigest);

            return JobResult.Success("Weekly campaign digest sent to SuperAdmins");
        }
    }
}
