using TheCharityBLL.Events.Abstraction;
using TheCharityBLL.Events.DonationEvents;
using TheCharityBLL.Jobs.Emails;
using TheCharityBLL.Services.Abstraction;
using TheCharityDAL.Repositories.Abstraction;

namespace TheCharityBLL.Events.EventHandlers.DonationEventHandlers
{
    public class CampaignDonationEventHandler : IEventHandler<CampaignDonationReceivedEvent>
    {
        private readonly IJobSchedulerService _jobScheduler;
        private readonly ICampaignRepository _campaignRepository;

        public CampaignDonationEventHandler(IJobSchedulerService jobScheduler, ICampaignRepository campaignRepository)
        {
            _jobScheduler = jobScheduler;
            _campaignRepository = campaignRepository;
        }

        public async Task HandleAsync(CampaignDonationReceivedEvent @event)
        {
            var campaignId = @event.CampaignId;
            var campaign = await _campaignRepository.GetCampaignByIdAsync(campaignId);

            // Calculate percentage
            var percentage = (campaign?.Achieved ?? 0) / (campaign?.Target ?? 1) * 100;

            // Check milestones
            if (percentage >= 25 && percentage < 50)
            {
                _jobScheduler.EnqueueJob<SendMilestoneEmailJob>(new { CampaignId = campaign?.Id, Milestone = 25 });
            }
            else if (percentage >= 50 && percentage < 75)
            {
                _jobScheduler.EnqueueJob<SendMilestoneEmailJob>(new { CampaignId = campaign?.Id, Milestone = 50 });
            }
            else if (percentage >= 75 && percentage < 100)
            {
                _jobScheduler.EnqueueJob<SendMilestoneEmailJob>(new { CampaignId = campaign?.Id, Milestone = 75 });
            }
            else if (percentage >= 100)
            {
                _jobScheduler.EnqueueJob<SendMilestoneEmailJob>(new { CampaignId = campaign?.Id, Milestone = 100 });
            }

            await Task.CompletedTask;
        }
    }
}
