using Microsoft.Extensions.Logging;
using TheCharityBLL.Events.Abstraction;
using TheCharityBLL.Events.CampaignEvents;
using TheCharityBLL.Services.Abstraction;
using TheCharityBLL.Services.Enums;

namespace TheCharityBLL.Events.EventHandlers.CampaignEventHandlers
{
    public class CampaignExpiredEventHandler : IEventHandler<CampaignExpiredEvent>
    {
        private readonly ICampaignNotificationService _notificationService;
        private readonly ILogger<CampaignExpiredEventHandler> _logger;

        public CampaignExpiredEventHandler(
            ICampaignNotificationService notificationService,
            ILogger<CampaignExpiredEventHandler> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task HandleAsync(CampaignExpiredEvent @event)
        {
            try
            {
                var campaign = @event.Campaign;

                var subject = $"⚠️ Campaign Expired: {campaign.Title}";
                var message =
                    $"Your campaign '{campaign.Title}' has expired.\n\n" +
                    $"📊 Final Results:\n" +
                    $"   • Target: ${campaign.Target:F2}\n" +
                    $"   • Raised: ${campaign.Achieved:F2}\n" +
                    $"   • Achievement: {((campaign.Achieved / campaign.Target) * 100):F1}%\n\n" +
                    $"You can extend the deadline or start a new campaign.";

                await _notificationService.SendCampaignNotificationAsync(
                    campaign.Id,
                    subject,
                    message,
                    NotificationType.CampaignExpired);

                _logger.LogInformation("Sent expired notification for campaign {CampaignId}", campaign.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle CampaignExpiredEvent for campaign {CampaignId}", @event.Campaign?.Id);
            }
        }
    }
}
