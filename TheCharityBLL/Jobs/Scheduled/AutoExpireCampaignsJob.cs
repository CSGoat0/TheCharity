using TheCharityBLL.Events.Abstraction;
using TheCharityBLL.Events.CampaignEvents;
using TheCharityBLL.Jobs.Base;
using TheCharityBLL.Jobs.Context;
using TheCharityBLL.Jobs.Result.Abstraction;
using TheCharityBLL.Jobs.Result.Implementation;
using TheCharityDAL.Enums;
using TheCharityDAL.Repositories.Abstraction;

namespace TheCharityBLL.Jobs.Emails
{
    public class AutoExpireCampaignsJob : BaseJob
    {
        private readonly ICampaignRepository _campaignRepository;
        private readonly IEventDispatcher _eventDispatcher;

        public AutoExpireCampaignsJob(
            ICampaignRepository campaignRepository,
            IEventDispatcher eventDispatcher)
        {
            _campaignRepository = campaignRepository;
            _eventDispatcher = eventDispatcher;
        }

        public override string JobName => "Auto-expire overdue campaigns";

        public override async Task<IJobResult> ExecuteAsync(JobContext context)
        {
            // Use repository directly to get entities
            var expiredCampaigns = await _campaignRepository.GetExpiredCampaignsAsync();

            if (expiredCampaigns == null || !expiredCampaigns.Any())
                return JobResult.Success("No expired campaigns found");

            var expiredCount = 0;

            foreach (var campaign in expiredCampaigns)
            {
                // Only expire active campaigns
                if (campaign.Status == CampaignStatus.Active)
                {
                    // Update status using repository
                    await _campaignRepository.UpdateCampaignStatusAsync(campaign.Id, CampaignStatus.Expired);

                    // Fire event with the actual Campaign entity
                    await _eventDispatcher.DispatchAsync(new CampaignExpiredEvent
                    {
                        Campaign = campaign
                    });

                    expiredCount++;
                }
            }

            return JobResult.Success($"Expired {expiredCount} campaigns and fired events");
        }
    }
}
