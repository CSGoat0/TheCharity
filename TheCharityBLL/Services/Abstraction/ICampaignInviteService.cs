using TheCharityBLL.DTOs;
using TheCharityBLL.DTOs.CampaignDTOs;
using TheCharityBLL.DTOs.PaginationDTOs;

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
        /// Get all invites for a shared campaign with pagination
        /// </summary>
        Task<ServiceResponse<PagedResultDto<InviteResponseDto>>> GetInvitesForCampaignAsync(
            PaginationParametersDto parametersDto,
            int sharedCampaignId);

        /// <summary>
        /// Get all pending invites for an organization with pagination
        /// </summary>
        Task<ServiceResponse<PagedResultDto<InviteResponseDto>>> GetPendingInvitesForOrganizationAsync(
            PaginationParametersDto parametersDto,
            int organizationId);

        /// <summary>
        /// Get all invites sent by a user with pagination
        /// </summary>
        Task<ServiceResponse<PagedResultDto<InviteResponseDto>>> GetInvitesSentByUserAsync(
            PaginationParametersDto parametersDto,
            string userId);

        /// <summary>
        /// Get all expired invites with pagination
        /// </summary>
        Task<ServiceResponse<PagedResultDto<InviteResponseDto>>> GetExpiredInvitesAsync(
            PaginationParametersDto parametersDto,
            DateTime cutoffDate);

        /// <summary>
        /// Check if an organization has a pending invite for a campaign
        /// </summary>
        Task<ServiceResponse<bool>> HasPendingInviteAsync(int sharedCampaignId, int organizationId);
    }
}