using TheCharityBLL.DTOs;
using TheCharityBLL.DTOs.CampaignDTOs;

namespace TheCharityBLL.Services.Abstraction
{
    public interface ICampaignInviteService
    {
        /// <summary>
        /// Send an invite to an organization to join a shared campaign
        /// </summary>
        Task<ServiceResponse<InviteResponseDto>> SendInviteAsync(
            int sharedCampaignId,
            int organizationId,
            string invitedByUserId,
            int expiresInDays = 7);

        /// <summary>
        /// Accept an invite
        /// </summary>
        Task<ServiceResponse<bool>> AcceptInviteAsync(int inviteId, string userId);

        /// <summary>
        /// Reject an invite
        /// </summary>
        Task<ServiceResponse<bool>> RejectInviteAsync(int inviteId, string userId);

        /// <summary>
        /// Get all invites for a shared campaign
        /// </summary>
        Task<ServiceResponse<IEnumerable<InviteResponseDto>>> GetInvitesForCampaignAsync(int sharedCampaignId);

        /// <summary>
        /// Get all pending invites for an organization
        /// </summary>
        Task<ServiceResponse<IEnumerable<InviteResponseDto>>> GetPendingInvitesForOrganizationAsync(int organizationId);

        /// <summary>
        /// Get all invites sent by a user
        /// </summary>
        Task<ServiceResponse<IEnumerable<InviteResponseDto>>> GetInvitesSentByUserAsync(string userId);

        /// <summary>
        /// Check if an organization has a pending invite for a campaign
        /// </summary>
        Task<ServiceResponse<bool>> HasPendingInviteAsync(int sharedCampaignId, int organizationId);
    }
}
