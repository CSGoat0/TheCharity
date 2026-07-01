using Microsoft.Extensions.Logging;
using TheCharityBLL.Events.Abstraction;
using TheCharityBLL.Events.CampaignEvents;
using TheCharityBLL.Services.Abstraction;
using TheCharityBLL.Services.Enums;

namespace TheCharityBLL.Events.EventHandlers.CampaignEventHandlers
{
    public class CampaignDismissedEventHandler : IEventHandler<CampaignDismissedEvent>
    {
        private readonly ICampaignNotificationService _notificationService;
        private readonly ILogger<CampaignDismissedEventHandler> _logger;

        public CampaignDismissedEventHandler(
            ICampaignNotificationService notificationService,
            ILogger<CampaignDismissedEventHandler> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task HandleAsync(CampaignDismissedEvent @event)
        {
            try
            {
                var campaign = @event.Campaign;

                var subject = $"📌 Campaign Dismissed: {campaign.Title}";
                var message =
                    $"The campaign '{campaign.Title}' has been dismissed.\n\n" +
                    $"📊 Final Results:\n" +
                    $"   • Target: ${campaign.Target:F2}\n" +
                    $"   • Raised: ${campaign.Achieved:F2}\n" +
                    $"   • Achievement: {((campaign.Achieved / campaign.Target) * 100):F1}%\n\n" +
                    $"You can create a new campaign whenever you're ready.";

                await _notificationService.SendCampaignNotificationAsync(
                    campaign.Id,
                    subject,
                    message,
                    NotificationType.General);

                _logger.LogInformation("Sent dismissed notification for campaign {CampaignId}", campaign.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle CampaignDismissedEvent for campaign {CampaignId}", @event.Campaign?.Id);
            }
        }
    }
}
