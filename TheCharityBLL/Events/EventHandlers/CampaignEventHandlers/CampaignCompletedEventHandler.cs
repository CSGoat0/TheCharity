using Microsoft.Extensions.Logging;
using TheCharityBLL.Events.Abstraction;
using TheCharityBLL.Events.CampaignEvents;
using TheCharityBLL.Services.Abstraction;
using TheCharityBLL.Services.Enums;

namespace TheCharityBLL.Events.EventHandlers.CampaignEventHandlers
{
    public class CampaignCompletedEventHandler : IEventHandler<CampaignCompletedEvent>
    {
        private readonly ICampaignNotificationService _notificationService;
        private readonly ILogger<CampaignCompletedEventHandler> _logger;

        public CampaignCompletedEventHandler(
            ICampaignNotificationService notificationService,
            ILogger<CampaignCompletedEventHandler> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task HandleAsync(CampaignCompletedEvent @event)
        {
            try
            {
                var campaign = @event.Campaign;

                var subject = $"🎉 Campaign Completed: {campaign.Title}";
                var message =
                    $"🎊 Congratulations!\n\n" +
                    $"Your campaign '{campaign.Title}' has been successfully completed!\n\n" +
                    $"📊 Final Results:\n" +
                    $"   • Target: ${campaign.Target:F2}\n" +
                    $"   • Raised: ${campaign.Achieved:F2}\n" +
                    $"   • Achievement: {((campaign.Achieved / campaign.Target) * 100):F1}%\n\n" +
                    $"Thank you for your efforts!";

                await _notificationService.SendCampaignNotificationAsync(
                    campaign.Id,
                    subject,
                    message,
                    NotificationType.CampaignCompleted);

                _logger.LogInformation("Sent campaign completed notification for campaign {CampaignId}", campaign.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle CampaignCompletedEvent for campaign {CampaignId}", @event.Campaign?.Id);
            }
        }
    }
}
