using Microsoft.Extensions.Logging;
using TheCharityBLL.Events.Abstraction;
using TheCharityBLL.Events.CampaignEvents;
using TheCharityDAL.Enums;

namespace TheCharityBLL.Events.EventHandlers.CampaignEventHandlers
{
    /// <summary>
    /// Handles the generic CampaignStatusChangedEvent and dispatches specific events
    /// based on the new status. This allows other handlers to react to specific
    /// status changes (e.g., CampaignCompletedEvent, CampaignExpiredEvent).
    /// </summary>
    public class CampaignStatusChangedEventHandler : IEventHandler<CampaignStatusChangedEvent>
    {
        private readonly IEventDispatcher _eventDispatcher;
        private readonly ILogger<CampaignStatusChangedEventHandler> _logger;

        public CampaignStatusChangedEventHandler(
            IEventDispatcher eventDispatcher,
            ILogger<CampaignStatusChangedEventHandler> logger)
        {
            _eventDispatcher = eventDispatcher;
            _logger = logger;
        }

        public async Task HandleAsync(CampaignStatusChangedEvent @event)
        {
            try
            {
                var campaign = @event.Campaign;
                var oldStatus = @event.OldStatus;
                var newStatus = @event.NewStatus;

                _logger.LogInformation(
                    "Campaign {CampaignId} ({CampaignTitle}) status changed from {OldStatus} to {NewStatus}",
                    campaign.Id,
                    campaign.Title,
                    oldStatus,
                    newStatus);

                // Dispatch specific event based on the new status
                switch (newStatus)
                {
                    case CampaignStatus.Completed:
                        _logger.LogInformation("Campaign {CampaignId} completed - dispatching CampaignCompletedEvent", campaign.Id);
                        await _eventDispatcher.DispatchAsync(new CampaignCompletedEvent
                        {
                            Campaign = campaign
                        });
                        break;

                    case CampaignStatus.Expired:
                        _logger.LogInformation("Campaign {CampaignId} expired - dispatching CampaignExpiredEvent", campaign.Id);
                        await _eventDispatcher.DispatchAsync(new CampaignExpiredEvent
                        {
                            Campaign = campaign
                        });
                        break;

                    case CampaignStatus.Dismissed:
                        _logger.LogInformation("Campaign {CampaignId} dismissed - dispatching CampaignDismissedEvent", campaign.Id);
                        await _eventDispatcher.DispatchAsync(new CampaignDismissedEvent
                        {
                            Campaign = campaign
                        });
                        break;

                    case CampaignStatus.Postponed:
                        _logger.LogInformation("Campaign {CampaignId} postponed - dispatching CampaignPostponedEvent", campaign.Id);
                        await _eventDispatcher.DispatchAsync(new CampaignPostponedEvent
                        {
                            Campaign = campaign
                        });
                        break;

                    case CampaignStatus.Active:
                        _logger.LogInformation("Campaign {CampaignId} is now active - no specific event dispatched", campaign.Id);
                        // Optionally dispatch an event when campaign becomes active
                        // await _eventDispatcher.DispatchAsync(new CampaignActivatedEvent { Campaign = campaign });
                        break;

                    case CampaignStatus.Preparing:
                        _logger.LogInformation("Campaign {CampaignId} is in preparing state - no specific event dispatched", campaign.Id);
                        break;

                    default:
                        _logger.LogInformation("No specific event mapped for status {NewStatus} on campaign {CampaignId}",
                            newStatus, campaign.Id);
                        break;
                }

                _logger.LogInformation("Finished processing status change for campaign {CampaignId}", campaign.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle CampaignStatusChangedEvent for campaign {CampaignId}",
                    @event.Campaign?.Id);
                throw;
            }
        }
    }
}
