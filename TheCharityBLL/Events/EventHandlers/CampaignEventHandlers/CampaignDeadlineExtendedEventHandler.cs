using TheCharityBLL.Events.Abstraction;
using TheCharityBLL.Events.CampaignEvents;
using TheCharityBLL.Jobs.Emails;
using TheCharityBLL.Services.Abstraction;

namespace TheCharityBLL.Events.EventHandlers.CampaignEventHandlers
{
    public class CampaignDeadlineExtendedEventHandler : IEventHandler<CampaignDeadlineExtendedEvent>
    {
        private readonly IJobSchedulerService _jobScheduler;

        public CampaignDeadlineExtendedEventHandler(IJobSchedulerService jobScheduler)
        {
            _jobScheduler = jobScheduler;
        }

        public async Task HandleAsync(CampaignDeadlineExtendedEvent @event)
        {
            _jobScheduler.EnqueueJob<DeadlineExtensionConfirmationJob>(new { CampaignId = @event.Campaign.Id });

            await Task.CompletedTask;
        }
    }
}
