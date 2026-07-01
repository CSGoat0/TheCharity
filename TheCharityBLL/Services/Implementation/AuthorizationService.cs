using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using TheCharityBLL.Services.Abstraction;
using TheCharityDAL.Entities;
using TheCharityDAL.Repositories.Abstraction;

namespace TheCharityBLL.Services.Implementation
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICampaignRepository _campaignRepository;
        private readonly IDonatedItemsRepository _donatedItemsRepository;

        public AuthorizationService(
            IUserRepository userRepository,
            ICampaignRepository campaignRepository,
            IDonatedItemsRepository donatedItemsRepository)
        {
            _userRepository = userRepository;
            _campaignRepository = campaignRepository;
            _donatedItemsRepository = donatedItemsRepository;
        }

        // ===== Helper Methods =====

        public async Task<string?> GetUserIdFromPrincipalAsync(ClaimsPrincipal principal)
        {
            if (principal == null) return null;
            return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public async Task<int?> GetOrganizationIdFromCampaignAsync(int campaignId)
        {
            return await _campaignRepository.GetCampaignCreatorOrganizationIdAsync(campaignId);
        }

        // ===== SuperAdmin Checks =====

        public async Task<bool> IsSuperAdminAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return false;
            return await _userRepository.IsSuperAdminAsync(userId);
        }

        public async Task<bool> IsSuperAdminAsync(ClaimsPrincipal user)
        {
            var userId = await GetUserIdFromPrincipalAsync(user);
            return await IsSuperAdminAsync(userId);
        }

        // ===== Organization Permission Checks =====

        public async Task<bool> CanManageOrganizationAsync(string userId, int organizationId)
        {
            if (string.IsNullOrEmpty(userId)) return false;

            // SuperAdmin can manage ANY organization
            if (await IsSuperAdminAsync(userId)) return true;

            // Organization Admin can manage their own organization
            return await _userRepository.IsOrganizationAdminAsync(userId, organizationId);
        }

        public async Task<bool> CanUpdatePaymentInfoAsync(string userId, int organizationId)
        {
            if (string.IsNullOrEmpty(userId)) return false;

            // SuperAdmin can update ANY payment info
            if (await IsSuperAdminAsync(userId)) return true;

            // ONLY Organization Admin (NOT SubAdmin) can update payment info
            return await _userRepository.IsOrganizationAdminAsync(userId, organizationId);
        }

        public async Task<bool> CanManageSubAdminsAsync(string userId, int organizationId)
        {
            if (string.IsNullOrEmpty(userId)) return false;

            // SuperAdmin can manage sub-admins in ANY organization
            if (await IsSuperAdminAsync(userId)) return true;

            // ONLY Organization Admin (NOT SubAdmin) can manage sub-admins
            return await _userRepository.IsOrganizationAdminAsync(userId, organizationId);
        }

        public async Task<bool> CanCreateCampaignForOrganizationAsync(string userId, int organizationId)
        {
            if (string.IsNullOrEmpty(userId)) return false;

            // SuperAdmin can create campaigns for any organization
            if (await IsSuperAdminAsync(userId)) return true;

            // Organization Admin or SubAdmin can create campaigns
            return await _userRepository.IsOrganizationAdminOrSubAdminAsync(userId, organizationId);
        }

        public async Task<bool> IsOrganizationAdminAsync(string userId, int organizationId)
        {
            if (string.IsNullOrEmpty(userId)) return false;
            return await _userRepository.IsOrganizationAdminAsync(userId, organizationId);
        }

        public async Task<bool> IsOrganizationSubAdminAsync(string userId, int organizationId)
        {
            if (string.IsNullOrEmpty(userId)) return false;
            return await _userRepository.IsOrganizationSubAdminAsync(userId, organizationId);
        }

        public async Task<bool> IsOrganizationAdminOrSubAdminAsync(string userId, int organizationId)
        {
            if (string.IsNullOrEmpty(userId)) return false;
            return await _userRepository.IsOrganizationAdminOrSubAdminAsync(userId, organizationId);
        }

        // ===== Campaign Permission Checks =====

        public async Task<bool> CanManageCampaignAsync(string userId, int campaignId)
        {
            if (string.IsNullOrEmpty(userId)) return false;

            // SuperAdmin can manage ANY campaign
            if (await IsSuperAdminAsync(userId)) return true;

            // Get the campaign's organization
            var organizationId = await _campaignRepository.GetCampaignCreatorOrganizationIdAsync(campaignId);
            if (!organizationId.HasValue) return false;

            // Check if user is Admin or SubAdmin of the organization
            return await _userRepository.IsOrganizationAdminOrSubAdminAsync(userId, organizationId.Value);
        }

        public async Task<bool> CanUpdateCampaignStatusAsync(string userId, int campaignId)
        {
            // Same as CanManageCampaign - Admins and SubAdmins can update status
            return await CanManageCampaignAsync(userId, campaignId);
        }

        public async Task<bool> CanDeleteCampaignAsync(string userId, int campaignId)
        {
            if (string.IsNullOrEmpty(userId)) return false;

            // ONLY SuperAdmin can delete campaigns
            return await IsSuperAdminAsync(userId);
        }

        public async Task<bool> CanViewCampaignDetailsAsync(string userId, int campaignId)
        {
            // Anyone can view campaign details (public)
            // But if we want to restrict, we could check if campaign is deleted
            return true;
        }

        // ===== Shared Campaign Permission Checks =====

        public async Task<bool> IsSharedCampaignCreatorAsync(string userId, int campaignId)
        {
            if (string.IsNullOrEmpty(userId)) return false;

            // SuperAdmin is considered the creator for management purposes
            if (await IsSuperAdminAsync(userId)) return true;

            var campaign = await _campaignRepository.GetCampaignByIdAsync(campaignId);
            if (campaign is not SharedCampaign shared) return false;

            // Check if user is Admin/SubAdmin of the creator organization
            return await _userRepository.IsOrganizationAdminOrSubAdminAsync(userId, shared.CreatorOrganizationId);
        }

        public async Task<bool> CanSendInviteAsync(string userId, int campaignId)
        {
            if (string.IsNullOrEmpty(userId)) return false;

            // SuperAdmin can send invites for ANY shared campaign
            if (await IsSuperAdminAsync(userId)) return true;

            var campaign = await _campaignRepository.GetCampaignByIdAsync(campaignId);
            if (campaign is not SharedCampaign shared) return false;

            // Only the creator organization's Admin/SubAdmin can send invites
            return await _userRepository.IsOrganizationAdminOrSubAdminAsync(userId, shared.CreatorOrganizationId);
        }

        public async Task<bool> CanAcceptInviteAsync(string userId, int inviteId)
        {
            if (string.IsNullOrEmpty(userId)) return false;

            var invite = await _campaignRepository.GetInviteByIdAsync(inviteId);
            if (invite == null) return false;

            // SuperAdmin can accept any invite
            if (await IsSuperAdminAsync(userId)) return true;

            // Check if user is Admin/SubAdmin of the invited organization
            return await _userRepository.IsOrganizationAdminOrSubAdminAsync(userId, invite.OrganizationId);
        }

        public async Task<bool> CanRejectInviteAsync(string userId, int inviteId)
        {
            // Same as CanAcceptInvite
            return await CanAcceptInviteAsync(userId, inviteId);
        }

        public async Task<bool> CanAddOrganizationToSharedCampaignAsync(string userId, int campaignId)
        {
            // Same as CanSendInvite - only creator organization can add
            return await CanSendInviteAsync(userId, campaignId);
        }

        public async Task<bool> CanRemoveOrganizationFromSharedCampaignAsync(string userId, int campaignId)
        {
            // Same as CanSendInvite - only creator organization can remove
            return await CanSendInviteAsync(userId, campaignId);
        }

        // ===== Donated Item Permission Checks =====

        public async Task<bool> CanManageDonatedItemAsync(string userId, int donatedItemId)
        {
            if (string.IsNullOrEmpty(userId)) return false;

            // SuperAdmin can manage ANY donated item
            if (await IsSuperAdminAsync(userId)) return true;

            // Get the donated item
            var donatedItem = await _donatedItemsRepository.GetDonatedItemByIdAsync(donatedItemId);
            if (donatedItem == null) return false;

            // Check if user is Admin/SubAdmin of the organization that owns the item
            return await _userRepository.IsOrganizationAdminOrSubAdminAsync(userId, donatedItem.OrganizationId);
        }

        public async Task<bool> CanTransferDonatedItemAsync(string userId, int donatedItemId)
        {
            if (string.IsNullOrEmpty(userId)) return false;

            // SuperAdmin can transfer ANY donated item
            if (await IsSuperAdminAsync(userId)) return true;

            // Get the donated item
            var donatedItem = await _donatedItemsRepository.GetDonatedItemByIdAsync(donatedItemId);
            if (donatedItem == null) return false;

            // ONLY Organization Admin (NOT SubAdmin) can transfer items
            return await _userRepository.IsOrganizationAdminAsync(userId, donatedItem.OrganizationId);
        }

        // ===== User Permission Checks =====

        public async Task<bool> CanManageUserAsync(string userId, string targetUserId)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(targetUserId)) return false;

            // Users can manage their own profile
            if (userId == targetUserId) return true;

            // SuperAdmin can manage ANY user
            if (await IsSuperAdminAsync(userId)) return true;

            return false;
        }

        public async Task<bool> CanManageUserRolesAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return false;

            // ONLY SuperAdmin can manage user roles
            return await IsSuperAdminAsync(userId);
        }

        public async Task<bool> CanDeleteUserAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return false;

            // ONLY SuperAdmin can delete users
            return await IsSuperAdminAsync(userId);
        }

        public async Task<bool> CanRestoreUserAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return false;

            // ONLY SuperAdmin can restore users
            return await IsSuperAdminAsync(userId);
        }

        // ===== Bulk Operations =====

        public async Task<bool> CanPerformBulkOperationsAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return false;

            // ONLY SuperAdmin can perform bulk operations
            return await IsSuperAdminAsync(userId);
        }
    }
}
