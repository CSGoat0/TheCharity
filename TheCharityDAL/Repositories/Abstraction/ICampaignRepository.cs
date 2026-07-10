using TheCharityDAL.Entities;
using TheCharityDAL.Enums;

namespace TheCharityDAL.Repositories.Abstraction
{
    public interface ICampaignRepository
    {
        // ===== CRUD Operations for Abstract Campaign =====
        Task<IEnumerable<Campaign>> GetAllCampaignsAsync(bool includeDeleted = false);
        Task<Campaign?> GetCampaignByIdAsync(int id);
        Task<Campaign> AddCampaignAsync(Campaign campaign);
        Task<Campaign> UpdateCampaignAsync(Campaign campaign);
        Task DeleteCampaignAsync(int id);
        Task RestoreCampaignAsync(int id);

        // ===== Type-Specific CRUD Operations =====
        // SoloCampaign
        Task<IEnumerable<SoloCampaign>> GetAllSoloCampaignsAsync();
        Task<SoloCampaign?> GetSoloCampaignByIdAsync(int id);
        Task<SoloCampaign> AddSoloCampaignAsync(SoloCampaign campaign);
        Task<SoloCampaign> UpdateSoloCampaignAsync(SoloCampaign campaign);

        // SharedCampaign
        Task<IEnumerable<SharedCampaign>> GetAllSharedCampaignsAsync();
        Task<SharedCampaign?> GetSharedCampaignByIdAsync(int id);
        Task<SharedCampaign> AddSharedCampaignAsync(SharedCampaign campaign);
        Task<SharedCampaign> UpdateSharedCampaignAsync(SharedCampaign campaign);

        // ===== Filtering & Querying =====
        Task<IEnumerable<Campaign>> GetCampaignsByStatusAsync(CampaignStatus status);
        Task<IEnumerable<Campaign>> GetCampaignsByTypeAsync(CampaignType type);
        Task<IEnumerable<Campaign>> GetActiveCampaignsAsync();
        Task<IEnumerable<Campaign>> SearchCampaignsAsync(string searchTerm);
        Task<IEnumerable<Campaign>> GetDeletedCampaignsAsync();

        // Type-specific filtering
        Task<IEnumerable<SoloCampaign>> GetSoloCampaignsByOrganizationIdAsync(int organizationId);
        Task<IEnumerable<SharedCampaign>> GetSharedCampaignsByOrganizationIdAsync(int organizationId);
        Task<IEnumerable<SoloCampaign>> GetSoloCampaignsByStatusAsync(CampaignStatus status);
        Task<IEnumerable<SharedCampaign>> GetSharedCampaignsByStatusAsync(CampaignStatus status);

        // ===== SharedCampaign Specific Operations =====
        Task AddOrganizationToSharedCampaignAsync(int sharedCampaignId, Organization organization);
        Task RemoveOrganizationFromSharedCampaignAsync(int sharedCampaignId, Organization organization);
        Task<int> GetOrganizationCountForSharedCampaignAsync(int sharedCampaignId);

        // ===== Campaign Progress Operations =====
        Task<Campaign?> UpdateCampaignMoneyAsync(int campaignId, double achievedAmount);
        Task<Campaign?> IncrementCampaignMoneyAsync(int campaignId, double amount);
        Task<Campaign?> UpdateCampaignStatusAsync(int campaignId, CampaignStatus status);

        // ===== Advanced Filtering =====
        Task<IEnumerable<Campaign>> GetCampaignsByTargetRangeAsync(double minTarget, double maxTarget);
        Task<IEnumerable<Campaign>> GetCampaignsByAchievementPercentageAsync(double minPercentage);
        Task<IEnumerable<Campaign>> GetCampaignsNearTargetAsync(int percentageThreshold = 90);
        Task<IEnumerable<Campaign>> GetCampaignsEndingSoonAsync(double remainingValue = 1000);

        // ===== Statistics & Analytics =====
        Task<int> GetTotalCampaignsCountAsync(bool includeDeleted = false);
        Task<int> GetTotalActiveCampaignsCountAsync();
        Task<int> GetSoloCampaignsCountAsync();
        Task<int> GetSharedCampaignsCountAsync();
        Task<double> GetTotalMoneyRaisedAsync();
        Task<double> GetTotalMoneyRaisedBySoloCampaignsAsync();
        Task<double> GetTotalMoneyRaisedBySharedCampaignsAsync();
        Task<double> GetAverageAchievementPercentageAsync();
        Task<Dictionary<CampaignType, int>> GetCampaignCountByTypeAsync();
        Task<Dictionary<CampaignStatus, int>> GetCampaignCountByStatusAsync();
        Task<Dictionary<int, int>> GetCampaignCountByOrganizationAsync();

        // ===== Featured & Trending =====
        Task<IEnumerable<Campaign>> GetTopCampaignsByAchievementAsync(int limit = 10);
        Task<IEnumerable<Campaign>> GetTopCampaignsByDonationsAsync(int limit = 10);
        Task<IEnumerable<Campaign>> GetRecentCampaignsAsync(int days = 30);
        Task<IEnumerable<Campaign>> GetUrgentCampaignsAsync(double minPercentage = 75);
        // ===== Deadline Operations =====
        Task<IEnumerable<Campaign>> GetCampaignsByDeadlineAsync(DateTime deadlineDate, bool includeDeleted = false);
        Task<IEnumerable<Campaign>> GetExpiredCampaignsAsync();
        Task<IEnumerable<Campaign>> GetCampaignsExpiringSoonAsync(int daysThreshold = 7);
        Task<Campaign?> ExtendCampaignDeadlineAsync(int campaignId, DateTime newDeadline);

        // ===== Bulk Operations =====
        Task<int> BulkUpdateCampaignStatusAsync(CampaignStatus oldStatus, CampaignStatus newStatus);
        Task<int> SoftDeleteExpiredCampaignsAsync(int daysAfterCompletion = 30);

        // ===== Utility Methods =====
        Task<bool> CampaignExistsAsync(int id);
        Task<bool> IsCampaignActiveAsync(int id);
        Task<double> GetCampaignAchievementPercentageAsync(int id);
        Task<CampaignType?> GetCampaignTypeAsync(int id);

        // Campaign Ownership
        Task<bool> IsCampaignOwnedByOrganizationAsync(int campaignId, int organizationId);
        Task<int?> GetCampaignCreatorOrganizationIdAsync(int campaignId);

        // Shared Campaign Invites
        Task<SharedCampaignInvite> CreateInviteAsync(SharedCampaignInvite invite);
        Task<IEnumerable<SharedCampaignInvite>> GetExpiredInvitesAsync(DateTime cutoffDate);
        Task<SharedCampaignInvite?> GetInviteByIdAsync(int inviteId);
        Task<IEnumerable<SharedCampaignInvite>> GetInvitesForSharedCampaignAsync(int sharedCampaignId);
        Task<IEnumerable<SharedCampaignInvite>> GetPendingInvitesForOrganizationAsync(int organizationId);
        Task<IEnumerable<SharedCampaignInvite>> GetInvitesSentByUserAsync(string userId);
        Task<SharedCampaignInvite> UpdateInviteStatusAsync(int inviteId, InviteStatus status);
        Task<bool> HasPendingInviteAsync(int sharedCampaignId, int organizationId);
        Task<int> ExpireOldInvitesAsync(DateTime cutoffDate);
    }
}
