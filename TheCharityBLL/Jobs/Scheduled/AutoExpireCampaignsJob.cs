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
            const int batchSize = 100;
            int pageNumber = 1;
            int totalExpired = 0;
            bool hasMore = true;

            while (hasMore)
            {
                var (expiredCampaigns, totalCount) = await _campaignRepository
                    .GetExpiredCampaignsAsync(pageNumber, batchSize);

                if (!expiredCampaigns.Any())
                    break;

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
                        totalExpired++;
                    }
                }

                // Check if there are more pages
                hasMore = pageNumber * batchSize < totalCount;
                pageNumber++;
            }

            return JobResult.Success($"Expired {totalExpired} campaigns and fired events");
        }
    }
}
