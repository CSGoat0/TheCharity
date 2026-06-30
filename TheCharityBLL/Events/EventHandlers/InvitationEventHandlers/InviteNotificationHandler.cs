using Microsoft.Extensions.Logging;
using TheCharityBLL.Events.Abstraction;
using TheCharityBLL.Events.Events.InvitationEvents;
using TheCharityBLL.Services.Abstraction;
using TheCharityBLL.Services.Enums;

namespace TheCharityBLL.Events.EventHandlers.InvitationEventHandlers
{
    public class InviteNotificationHandler :
           IEventHandler<InviteSentEvent>,
           IEventHandler<InviteAcceptedEvent>,
           IEventHandler<InviteRejectedEvent>,
           IEventHandler<InviteExpiredEvent>
    {
        private readonly ICampaignNotificationService _notificationService;
        private readonly ILogger<InviteNotificationHandler> _logger;

        public InviteNotificationHandler(
            ICampaignNotificationService notificationService,
            ILogger<InviteNotificationHandler> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task HandleAsync(InviteSentEvent @event)
        {
            try
            {
                var invite = @event.Invite;
                var campaign = @event.Campaign;
                var invitedOrg = @event.InvitedOrganization;
                var invitedBy = @event.InvitedByUser;

                _logger.LogInformation(
                    "Processing InviteSentEvent: Campaign {CampaignId} → Organization {OrganizationId}",
                    campaign.Id, invitedOrg.Id);

                // 1. Notify the invited organization's Admin and SubAdmins
                var subject = $"📨 You've been invited to join: {campaign.Title}";
                var message =
                    $"Hello,\n\n" +
                    $"You have been invited to join the shared campaign '{campaign.Title}'.\n\n" +
                    $"📌 Campaign Details:\n" +
                    $"   • Title: {campaign.Title}\n" +
                    $"   • Description: {campaign.Description ?? "No description provided"}\n" +
                    $"   • Target: ${campaign.Target:F2}\n" +
                    $"   • Deadline: {campaign.Deadline:yyyy-MM-dd}\n\n" +
                    $"👤 Invited By: {invitedBy.FullName ?? invitedBy.UserName ?? "Unknown User"}\n" +
                    $"📅 Invite Expires: {invite.ExpiresAt:yyyy-MM-dd}\n\n" +
                    $"To accept this invite, please visit your dashboard and go to 'Pending Invites'.\n\n" +
                    $"Thank you!";

                // Send to ALL stakeholders of the invited organization
                await _notificationService.SendOrganizationNotificationAsync(
                    invitedOrg.Id,
                    subject,
                    message,
                    NotificationType.General,
                    includeSubAdmins: true);

                // 2. Notify the campaign creator organization's Admin and SubAdmins (confirmation)
                var creatorOrg = campaign.CreatorOrganization;
                if (creatorOrg != null && creatorOrg.Id != invitedOrg.Id)
                {
                    var confirmSubject = $"📤 Invite sent to {invitedOrg.Name} for: {campaign.Title}";
                    var confirmMessage =
                        $"An invite has been sent to {invitedOrg.Name} to join your shared campaign '{campaign.Title}'.\n\n" +
                        $"📌 Campaign: {campaign.Title}\n" +
                        $"🏢 Organization: {invitedOrg.Name}\n" +
                        $"📅 Expires: {invite.ExpiresAt:yyyy-MM-dd}\n\n" +
                        $"You will be notified when they accept or reject the invite.";

                    await _notificationService.SendOrganizationNotificationAsync(
                        creatorOrg.Id,
                        confirmSubject,
                        confirmMessage,
                        NotificationType.General,
                        includeSubAdmins: true);
                }

                // 3. Notify SuperAdmins
                var superAdminSubject = $"📨 New Campaign Invite: {campaign.Title} → {invitedOrg.Name}";
                var superAdminMessage =
                    $"A new campaign invite has been sent.\n\n" +
                    $"📌 Campaign: {campaign.Title}\n" +
                    $"🏢 Invited Organization: {invitedOrg.Name}\n" +
                    $"👤 Invited By: {invitedBy.FullName ?? invitedBy.UserName ?? "Unknown User"}\n" +
                    $"📅 Expires: {invite.ExpiresAt:yyyy-MM-dd}\n\n" +
                    $"This is for informational purposes only. No action is required.";

                await _notificationService.SendSuperAdminNotificationAsync(
                    superAdminSubject,
                    superAdminMessage,
                    NotificationType.General);

                _logger.LogInformation(
                    "InviteSentEvent processed successfully. Invite {InviteId}",
                    invite.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle InviteSentEvent for invite {InviteId}",
                    @event.Invite?.Id);
            }
        }

        public async Task HandleAsync(InviteAcceptedEvent @event)
        {
            try
            {
                var invite = @event.Invite;
                var campaign = @event.Campaign;
                var invitedOrg = @event.InvitedOrganization;
                var acceptedBy = @event.AcceptedByUser;

                _logger.LogInformation(
                    "Processing InviteAcceptedEvent: Campaign {CampaignId} → Organization {OrganizationId}",
                    campaign.Id, invitedOrg.Id);

                // 1. Notify the organization that accepted (confirmation)
                var confirmSubject = $"✅ You've joined: {campaign.Title}";
                var confirmMessage =
                    $"🎉 Congratulations!\n\n" +
                    $"Your organization '{invitedOrg.Name}' has successfully joined the shared campaign '{campaign.Title}'.\n\n" +
                    $"📌 Campaign Details:\n" +
                    $"   • Title: {campaign.Title}\n" +
                    $"   • Description: {campaign.Description ?? "No description provided"}\n" +
                    $"   • Target: ${campaign.Target:F2}\n" +
                    $"   • Deadline: {campaign.Deadline:yyyy-MM-dd}\n\n" +
                    $"You can now manage this campaign and contribute to its success.\n\n" +
                    $"Visit your dashboard to get started!";

                await _notificationService.SendOrganizationNotificationAsync(
                    invitedOrg.Id,
                    confirmSubject,
                    confirmMessage,
                    NotificationType.General,
                    includeSubAdmins: true);

                // 2. Notify the campaign creator organization
                var creatorOrg = campaign.CreatorOrganization;
                if (creatorOrg != null && creatorOrg.Id != invitedOrg.Id)
                {
                    var creatorSubject = $"✅ {invitedOrg.Name} accepted invite for: {campaign.Title}";
                    var creatorMessage =
                        $"{invitedOrg.Name} has accepted the invite to join your shared campaign '{campaign.Title}'.\n\n" +
                        $"📌 Campaign: {campaign.Title}\n" +
                        $"🏢 Organization: {invitedOrg.Name}\n" +
                        $"👤 Accepted By: {acceptedBy.FullName ?? acceptedBy.UserName ?? "Unknown User"}\n\n" +
                        $"The organization is now a participant in your campaign.";

                    await _notificationService.SendOrganizationNotificationAsync(
                        creatorOrg.Id,
                        creatorSubject,
                        creatorMessage,
                        NotificationType.General,
                        includeSubAdmins: true);
                }

                // 3. Notify SuperAdmins
                var superAdminSubject = $"✅ Invite Accepted: {campaign.Title} → {invitedOrg.Name}";
                var superAdminMessage =
                    $"A campaign invite has been accepted.\n\n" +
                    $"📌 Campaign: {campaign.Title}\n" +
                    $"🏢 Organization: {invitedOrg.Name}\n" +
                    $"👤 Accepted By: {acceptedBy.FullName ?? acceptedBy.UserName ?? "Unknown User"}\n\n" +
                    $"The organization is now a participant in this shared campaign.";

                await _notificationService.SendSuperAdminNotificationAsync(
                    superAdminSubject,
                    superAdminMessage,
                    NotificationType.General);

                _logger.LogInformation(
                    "InviteAcceptedEvent processed successfully. Invite {InviteId}",
                    invite.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle InviteAcceptedEvent for invite {InviteId}",
                    @event.Invite?.Id);
            }
        }

        public async Task HandleAsync(InviteRejectedEvent @event)
        {
            try
            {
                var invite = @event.Invite;
                var campaign = @event.Campaign;
                var invitedOrg = @event.InvitedOrganization;
                var rejectedBy = @event.RejectedByUser;

                _logger.LogInformation(
                    "Processing InviteRejectedEvent: Campaign {CampaignId} → Organization {OrganizationId}",
                    campaign.Id, invitedOrg.Id);

                // 1. Notify the campaign creator organization
                var creatorOrg = campaign.CreatorOrganization;
                if (creatorOrg != null && creatorOrg.Id != invitedOrg.Id)
                {
                    var creatorSubject = $"❌ {invitedOrg.Name} rejected invite for: {campaign.Title}";
                    var creatorMessage =
                        $"{invitedOrg.Name} has rejected the invite to join your shared campaign '{campaign.Title}'.\n\n" +
                        $"📌 Campaign: {campaign.Title}\n" +
                        $"🏢 Organization: {invitedOrg.Name}\n" +
                        $"👤 Rejected By: {rejectedBy.FullName ?? rejectedBy.UserName ?? "Unknown User"}\n\n" +
                        $"You can send a new invite if needed.";

                    await _notificationService.SendOrganizationNotificationAsync(
                        creatorOrg.Id,
                        creatorSubject,
                        creatorMessage,
                        NotificationType.General,
                        includeSubAdmins: true);
                }

                // 2. Notify SuperAdmins (informational)
                var superAdminSubject = $"❌ Invite Rejected: {campaign.Title} → {invitedOrg.Name}";
                var superAdminMessage =
                    $"A campaign invite has been rejected.\n\n" +
                    $"📌 Campaign: {campaign.Title}\n" +
                    $"🏢 Organization: {invitedOrg.Name}\n" +
                    $"👤 Rejected By: {rejectedBy.FullName ?? rejectedBy.UserName ?? "Unknown User"}\n\n" +
                    $"This is for informational purposes only.";

                await _notificationService.SendSuperAdminNotificationAsync(
                    superAdminSubject,
                    superAdminMessage,
                    NotificationType.General);

                _logger.LogInformation(
                    "InviteRejectedEvent processed successfully. Invite {InviteId}",
                    invite.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle InviteRejectedEvent for invite {InviteId}",
                    @event.Invite?.Id);
            }
        }

        public async Task HandleAsync(InviteExpiredEvent @event)
        {
            try
            {
                var invite = @event.Invite;
                var campaign = @event.Campaign;
                var invitedOrg = @event.InvitedOrganization;

                _logger.LogInformation(
                    "Processing InviteExpiredEvent: Campaign {CampaignId} → Organization {OrganizationId}",
                    campaign.Id, invitedOrg.Id);

                // 1. Notify the invited organization
                var expiredSubject = $"⏰ Invite expired for: {campaign.Title}";
                var expiredMessage =
                    $"The invite to join the shared campaign '{campaign.Title}' has expired.\n\n" +
                    $"📌 Campaign: {campaign.Title}\n" +
                    $"🏢 Organization: {invitedOrg.Name}\n" +
                    $"📅 Expired On: {DateTime.UtcNow:yyyy-MM-dd}\n\n" +
                    $"If you still wish to join this campaign, please request a new invite.";

                await _notificationService.SendOrganizationNotificationAsync(
                    invitedOrg.Id,
                    expiredSubject,
                    expiredMessage,
                    NotificationType.General,
                    includeSubAdmins: true);

                // 2. Notify the campaign creator organization
                var creatorOrg = campaign.CreatorOrganization;
                if (creatorOrg != null && creatorOrg.Id != invitedOrg.Id)
                {
                    var creatorExpiredSubject = $"⏰ Invite expired for: {campaign.Title} → {invitedOrg.Name}";
                    var creatorExpiredMessage =
                        $"The invite sent to {invitedOrg.Name} for your shared campaign '{campaign.Title}' has expired.\n\n" +
                        $"📌 Campaign: {campaign.Title}\n" +
                        $"🏢 Organization: {invitedOrg.Name}\n" +
                        $"📅 Expired On: {DateTime.UtcNow:yyyy-MM-dd}\n\n" +
                        $"You can send a new invite if needed.";

                    await _notificationService.SendOrganizationNotificationAsync(
                        creatorOrg.Id,
                        creatorExpiredSubject,
                        creatorExpiredMessage,
                        NotificationType.General,
                        includeSubAdmins: true);
                }

                _logger.LogInformation(
                    "InviteExpiredEvent processed successfully. Invite {InviteId}",
                    invite.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle InviteExpiredEvent for invite {InviteId}",
                    @event.Invite?.Id);
            }
        }
    }
}
