using System.Security.Claims;

namespace TheCharityBLL.Services.Abstraction
{
    public interface IAuthorizationService
    {
        // ===== SuperAdmin Checks =====
        Task<bool> IsSuperAdminAsync(string userId);
        Task<bool> IsSuperAdminAsync(ClaimsPrincipal user);

        // ===== Organization Permission Checks =====
        Task<bool> CanManageOrganizationAsync(string userId, int organizationId);
        Task<bool> CanUpdatePaymentInfoAsync(string userId, int organizationId);
        Task<bool> CanManageSubAdminsAsync(string userId, int organizationId);
        Task<bool> CanCreateCampaignForOrganizationAsync(string userId, int organizationId);
        Task<bool> IsOrganizationAdminAsync(string userId, int organizationId);
        Task<bool> IsOrganizationSubAdminAsync(string userId, int organizationId);
        Task<bool> IsOrganizationAdminOrSubAdminAsync(string userId, int organizationId);

        // ===== Campaign Permission Checks =====
        Task<bool> CanManageCampaignAsync(string userId, int campaignId);
        Task<bool> CanUpdateCampaignStatusAsync(string userId, int campaignId);
        Task<bool> CanDeleteCampaignAsync(string userId, int campaignId);
        Task<bool> CanViewCampaignDetailsAsync(string userId, int campaignId);

        // ===== Shared Campaign Permission Checks =====
        Task<bool> IsSharedCampaignCreatorAsync(string userId, int campaignId);
        Task<bool> CanSendInviteAsync(string userId, int campaignId);
        Task<bool> CanAcceptInviteAsync(string userId, int inviteId);
        Task<bool> CanRejectInviteAsync(string userId, int inviteId);
        Task<bool> CanAddOrganizationToSharedCampaignAsync(string userId, int campaignId);
        Task<bool> CanRemoveOrganizationFromSharedCampaignAsync(string userId, int campaignId);

        // ===== Donated Item Permission Checks =====
        Task<bool> CanManageDonatedItemAsync(string userId, int donatedItemId);
        Task<bool> CanTransferDonatedItemAsync(string userId, int donatedItemId);

        // ===== User Permission Checks =====
        Task<bool> CanManageUserAsync(string userId, string targetUserId);
        Task<bool> CanManageUserRolesAsync(string userId);
        Task<bool> CanDeleteUserAsync(string userId);
        Task<bool> CanRestoreUserAsync(string userId);

        // ===== Bulk Operations =====
        Task<bool> CanPerformBulkOperationsAsync(string userId);

        // ===== Utility =====
        Task<string?> GetUserIdFromPrincipalAsync(ClaimsPrincipal principal);
        Task<int?> GetOrganizationIdFromCampaignAsync(int campaignId);
    }
}
