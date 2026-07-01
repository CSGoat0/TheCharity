using TheCharityBLL.Events.Abstraction;
using TheCharityBLL.Events.DonationEvents;
using TheCharityBLL.Services.Abstraction;

namespace TheCharityBLL.Events.EventHandlers.CampaignEventHandlers
{
    public class IncrementCampaignMoneyEventHandler : IEventHandler<CampaignDonationReceivedEvent>
    {
        private readonly ICampaignService _campaignService;

        public IncrementCampaignMoneyEventHandler(ICampaignService campaignService)
        {
            _campaignService = campaignService;
        }

        public async Task HandleAsync(CampaignDonationReceivedEvent @event)
        {
            // Increment the campaign money
            await _campaignService.IncrementCampaignMoneyAsync(@event.CampaignId, @event.Amount);
        }
    }
}
