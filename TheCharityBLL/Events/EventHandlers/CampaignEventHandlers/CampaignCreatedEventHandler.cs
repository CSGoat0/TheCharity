using TheCharityBLL.Events.Abstraction;
using TheCharityBLL.Events.CampaignEvents;
using TheCharityBLL.Jobs.Emails;
using TheCharityBLL.Services.Abstraction;

namespace TheCharityBLL.Events.EventHandlers.CampaignEventHandlers
{
    public class CampaignCreatedEventHandler : IEventHandler<CampaignCreatedEvent>
    {
        private readonly IJobSchedulerService _jobScheduler;

        public CampaignCreatedEventHandler(IJobSchedulerService jobScheduler)
        {
            _jobScheduler = jobScheduler;
        }

        public async Task HandleAsync(CampaignCreatedEvent @event)
        {
            _jobScheduler.EnqueueJob<NewCampaignNotificationJob>(new { CampaignId = @event.Campaign.Id });

            await Task.CompletedTask;
        }
    }
}
