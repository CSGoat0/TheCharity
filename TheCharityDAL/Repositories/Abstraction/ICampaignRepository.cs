using Azure;
using TheCharityDAL.Entities;
using TheCharityDAL.Enums;

namespace TheCharityDAL.Repositories.Abstraction
{
    public interface ICampaignRepository
    {
        // ===== CRUD Operations for Abstract Campaign =====
        Task<IEnumerable<Campaign>> GetAllCampaignsAsync(bool includeDeleted = false);
        Task<(IEnumerable<Campaign> Data, int TotalCount)> GetAllCampaignsAsync(int pageNumber, int pageSize, bool includeDeleted = false);
        Task<Campaign?> GetCampaignByIdAsync(int id);
        Task<Campaign> AddCampaignAsync(Campaign campaign);
        Task<Campaign> UpdateCampaignAsync(Campaign campaign);
        Task DeleteCampaignAsync(int id);
        Task RestoreCampaignAsync(int id);

        // ===== Type-Specific CRUD Operations =====
        // SoloCampaign
        Task<IEnumerable<SoloCampaign>> GetAllSoloCampaignsAsync();
        Task<(IEnumerable<SoloCampaign> Data, int TotalCount)> GetAllSoloCampaignsAsync(int pageNumber, int pageSize);
        Task<SoloCampaign?> GetSoloCampaignByIdAsync(int id);
        Task<SoloCampaign> AddSoloCampaignAsync(SoloCampaign campaign);
        Task<SoloCampaign> UpdateSoloCampaignAsync(SoloCampaign campaign);

        // SharedCampaign
        Task<IEnumerable<SharedCampaign>> GetAllSharedCampaignsAsync();
        Task<(IEnumerable<SharedCampaign> Data, int TotalCount)> GetAllSharedCampaignsAsync(int pageNumber, int pageSize);
        Task<SharedCampaign?> GetSharedCampaignByIdAsync(int id);
        Task<SharedCampaign> AddSharedCampaignAsync(SharedCampaign campaign);
        Task<SharedCampaign> UpdateSharedCampaignAsync(SharedCampaign campaign);

        // ===== Filtering & Querying =====
        Task<IEnumerable<Campaign>> GetCampaignsByStatusAsync(CampaignStatus status);
        Task<(IEnumerable<Campaign> Data, int TotalCount)> GetCampaignsByStatusAsync(int pageNumber, int pageSize, CampaignStatus status);
        Task<IEnumerable<Campaign>> GetCampaignsByTypeAsync(CampaignType type);
        Task<(IEnumerable<Campaign> Data, int TotalCount)> GetCampaignsByTypeAsync(int pageNumber, int pageSize, CampaignType type);
        Task<IEnumerable<Campaign>> GetActiveCampaignsAsync();
        Task<(IEnumerable<Campaign> Data, int TotalCount)> GetActiveCampaignsAsync(int pageNumber, int pageSize);
        Task<IEnumerable<Campaign>> SearchCampaignsAsync(string searchTerm);
        Task<(IEnumerable<Campaign> Data, int TotalCount)> SearchCampaignsAsync(int pageNumber, int pageSize, string searchTerm);
        Task<IEnumerable<Campaign>> GetDeletedCampaignsAsync();
        Task<(IEnumerable<Campaign> Data, int TotalCount)> GetDeletedCampaignsAsync(int pageNumber, int pageSize);

        // Type-specific filtering
        Task<IEnumerable<SoloCampaign>> GetSoloCampaignsByOrganizationIdAsync(int organizationId);
        Task<(IEnumerable<SoloCampaign> Data, int TotalCount)> GetSoloCampaignsByOrganizationIdAsync(int pageNumber, int pageSize, int organizationId);
        Task<IEnumerable<SharedCampaign>> GetSharedCampaignsByOrganizationIdAsync(int organizationId);
        Task<(IEnumerable<SharedCampaign> Data, int TotalCount)> GetSharedCampaignsByOrganizationIdAsync(int pageNumber, int pageSize, int organizationId);
        Task<IEnumerable<SoloCampaign>> GetSoloCampaignsByStatusAsync(CampaignStatus status);
        Task<(IEnumerable<SoloCampaign> Data, int TotalCount)> GetSoloCampaignsByStatusAsync(int pageNumber, int pageSize, CampaignStatus status);
        Task<IEnumerable<SharedCampaign>> GetSharedCampaignsByStatusAsync(CampaignStatus status);
        Task<(IEnumerable<SharedCampaign> Data, int TotalCount)> GetSharedCampaignsByStatusAsync(int pageNumber, int pageSize, CampaignStatus status);

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
        Task<(IEnumerable<Campaign> Data, int TotalCount)> GetCampaignsByTargetRangeAsync(int pageNumber, int pageSize, double minTarget, double maxTarget);
        Task<IEnumerable<Campaign>> GetCampaignsByAchievementPercentageAsync(double minPercentage);
        Task<(IEnumerable<Campaign> Data, int TotalCount)> GetCampaignsByAchievementPercentageAsync(int pageNumber, int pageSize, double minPercentage);
        Task<IEnumerable<Campaign>> GetCampaignsNearTargetAsync(int percentageThreshold = 90);
        Task<(IEnumerable<Campaign> Data, int TotalCount)> GetCampaignsNearTargetAsync(int pageNumber, int pageSize, int percentageThreshold = 90);
        Task<IEnumerable<Campaign>> GetCampaignsEndingSoonAsync(double remainingValue = 1000);
        Task<(IEnumerable<Campaign> Data, int TotalCount)> GetCampaignsEndingSoonAsync(int pageNumber, int pageSize, double remainingValue = 1000);

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
        Task<(IEnumerable<Campaign> Data, int TotalCount)> GetTopCampaignsByAchievementAsync(int pageNumber, int pageSize, int limit = 10);
        Task<IEnumerable<Campaign>> GetTopCampaignsByDonationsAsync(int limit = 10);
        Task<(IEnumerable<Campaign> Data, int TotalCount)> GetTopCampaignsByDonationsAsync(int pageNumber, int pageSize, int limit = 10);
        Task<IEnumerable<Campaign>> GetRecentCampaignsAsync(int days = 30);
        Task<(IEnumerable<Campaign> Data, int TotalCount)> GetRecentCampaignsAsync(int pageNumber, int pageSize, int days = 30);
        Task<IEnumerable<Campaign>> GetUrgentCampaignsAsync(double minPercentage = 75);
        Task<(IEnumerable<Campaign> Data, int TotalCount)> GetUrgentCampaignsAsync(int pageNumber, int pageSize, double minPercentage = 75);
        // ===== Deadline Operations =====
        Task<IEnumerable<Campaign>> GetCampaignsByDeadlineAsync(DateTime deadlineDate, bool includeDeleted = false);
        Task<(IEnumerable<Campaign> Data, int TotalCount)> GetCampaignsByDeadlineAsync(int pageNumber, int pageSize, DateTime deadlineDate, bool includeDeleted = false);
        Task<IEnumerable<Campaign>> GetExpiredCampaignsAsync();
        Task<(IEnumerable<Campaign> Data, int TotalCount)> GetExpiredCampaignsAsync(int pageNumber, int pageSize);
        Task<IEnumerable<Campaign>> GetCampaignsExpiringSoonAsync(int daysThreshold = 7);
        Task<(IEnumerable<Campaign> Data, int TotalCount)> GetCampaignsExpiringSoonAsync(int pageNumber, int pageSize, int daysThreshold = 7);
        Task<Campaign?> ExtendCampaignDeadlineAsync(int campaignId, DateTime newDeadline);

        // ===== Bulk Operations =====
        Task<int> BulkUpdateCampaignStatusAsync(CampaignStatus oldStatus, CampaignStatus newStatus);
        Task<int> SoftDeleteExpiredCampaignsAsync(int daysAfterCompletion = 30);

        // ===== Utility Methods =====
        Task<bool> CampaignExistsAsync(int id);
        Task<bool> IsCampaignActiveAsync(int id);
        Task<double> GetCampaignAchievementPercentageAsync(int id);
        Task<CampaignType?> GetCampaignTypeAsync(int id);
    }
}
