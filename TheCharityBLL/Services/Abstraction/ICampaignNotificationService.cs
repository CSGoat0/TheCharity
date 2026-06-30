using TheCharityBLL.Services.Enums;

namespace TheCharityBLL.Services.Abstraction
{
    public interface ICampaignNotificationService
    {
        /// <summary>
        /// Send notification to all stakeholders of a campaign
        /// </summary>
        Task SendCampaignNotificationAsync(
            int campaignId,
            string subject,
            string message,
            NotificationType type);
        Task SendOrganizationNotificationAsync(int id, string creatorExpiredSubject, string creatorExpiredMessage, NotificationType general, bool includeSubAdmins);

        /// <summary>
        /// Send notification to all SuperAdmins only
        /// </summary>
        Task SendSuperAdminNotificationAsync(
            string subject,
            string message,
            NotificationType type);
    }
}
