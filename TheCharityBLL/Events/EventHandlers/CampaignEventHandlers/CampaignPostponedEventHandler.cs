using Microsoft.Extensions.Logging;
using TheCharityBLL.Events.Abstraction;
using TheCharityBLL.Events.CampaignEvents;
using TheCharityBLL.Services.Abstraction;
using TheCharityBLL.Services.Enums;

namespace TheCharityBLL.Events.EventHandlers.CampaignEventHandlers
{
    public class CampaignPostponedEventHandler : IEventHandler<CampaignPostponedEvent>
    {
        private readonly ICampaignNotificationService _notificationService;
        private readonly ILogger<CampaignPostponedEventHandler> _logger;

        public CampaignPostponedEventHandler(
            ICampaignNotificationService notificationService,
            ILogger<CampaignPostponedEventHandler> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task HandleAsync(CampaignPostponedEvent @event)
        {
            try
            {
                var campaign = @event.Campaign;

                var subject = $"⏸️ Campaign Postponed: {campaign.Title}";
                var message =
                    $"The campaign '{campaign.Title}' has been postponed.\n\n" +
                    $"💰 Current Progress: ${campaign.Achieved:F2} / ${campaign.Target:F2}\n" +
                    $"The campaign will resume at a later date.";

                await _notificationService.SendCampaignNotificationAsync(
                    campaign.Id,
                    subject,
                    message,
                    NotificationType.General);

                _logger.LogInformation("Sent postponed notification for campaign {CampaignId}", campaign.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle CampaignPostponedEvent for campaign {CampaignId}", @event.Campaign?.Id);
            }
        }
    }
}
