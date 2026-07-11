using TheCharityBLL.DTOs;
using TheCharityBLL.DTOs.CampaignDTOs;
using TheCharityDAL.Enums;

namespace TheCharityBLL.Services.Abstraction
{
    public interface ICampaignService
    {
        // ===== Base CRUD Operations =====
        Task<ServiceResponse<CampaignResponseDto>> GetCampaignByIdAsync(int id);
        Task<ServiceResponse<CampaignDetailsResponseDto>> GetCampaignDetailsByIdAsync(int id);
        Task<ServiceResponse<bool>> UpdateCampaignAsync(UpdateCampaignDto updateDto);
        Task<ServiceResponse<bool>> DeleteCampaignAsync(int id);
        Task<ServiceResponse<bool>> RestoreCampaignAsync(int id);

        // ===== Solo Campaign Operations =====
        Task<ServiceResponse<IEnumerable<SoloCampaignResponseDto>>> GetAllSoloCampaignsAsync(bool includeDeleted = false);
        Task<ServiceResponse<SoloCampaignResponseDto>> GetSoloCampaignByIdAsync(int id);
        Task<ServiceResponse<int>> CreateSoloCampaignAsync(CreateSoloCampaignDto createDto);
        Task<ServiceResponse<bool>> UpdateSoloCampaignAsync(UpdateSoloCampaignDto updateDto);
        Task<ServiceResponse<IEnumerable<SoloCampaignResponseDto>>> GetSoloCampaignsByOrganizationIdAsync(int organizationId);
        Task<ServiceResponse<IEnumerable<SoloCampaignResponseDto>>> GetSoloCampaignsByStatusAsync(CampaignStatus status);

        // ===== Shared Campaign Operations =====
        Task<ServiceResponse<IEnumerable<SharedCampaignResponseDto>>> GetAllSharedCampaignsAsync(bool includeDeleted = false);
        Task<ServiceResponse<SharedCampaignResponseDto>> GetSharedCampaignByIdAsync(int id);
        Task<ServiceResponse<int>> CreateSharedCampaignAsync(CreateSharedCampaignDto createDto);
        Task<ServiceResponse<bool>> UpdateSharedCampaignAsync(UpdateSharedCampaignDto updateDto);
        Task<ServiceResponse<IEnumerable<SharedCampaignResponseDto>>> GetSharedCampaignsByOrganizationIdAsync(int organizationId);
        Task<ServiceResponse<IEnumerable<SharedCampaignResponseDto>>> GetSharedCampaignsByStatusAsync(CampaignStatus status);

        // ===== Shared Campaign Organization Management =====
        Task<ServiceResponse<bool>> AddOrganizationToSharedCampaignAsync(int sharedCampaignId, int organizationId);
        Task<ServiceResponse<bool>> RemoveOrganizationFromSharedCampaignAsync(int sharedCampaignId, int organizationId);
        Task<ServiceResponse<int>> GetOrganizationCountForSharedCampaignAsync(int sharedCampaignId);

        // ===== Campaign Progress Operations =====
        Task<ServiceResponse<bool>> UpdateCampaignMoneyAsync(int campaignId, double achievedAmount);
        Task<ServiceResponse<bool>> IncrementCampaignMoneyAsync(int campaignId, double amount);
        Task<ServiceResponse<bool>> UpdateCampaignStatusAsync(int campaignId, CampaignStatus status);

        // ===== Filtering & Querying =====
        Task<ServiceResponse<IEnumerable<CampaignResponseDto>>> GetAllCampaignsAsync(bool includeDeleted = false);
        Task<ServiceResponse<IEnumerable<CampaignResponseDto>>> GetCampaignsByStatusAsync(CampaignStatus status);
        Task<ServiceResponse<IEnumerable<CampaignResponseDto>>> GetCampaignsByTypeAsync(CampaignType type);
        Task<ServiceResponse<IEnumerable<CampaignResponseDto>>> GetActiveCampaignsAsync();
        Task<ServiceResponse<IEnumerable<CampaignResponseDto>>> SearchCampaignsAsync(string searchTerm);
        Task<ServiceResponse<IEnumerable<CampaignResponseDto>>> GetDeletedCampaignsAsync();

        // ===== Advanced Filtering =====
        Task<ServiceResponse<IEnumerable<CampaignResponseDto>>> GetCampaignsByTargetRangeAsync(double minTarget, double maxTarget);
        Task<ServiceResponse<IEnumerable<CampaignResponseDto>>> GetCampaignsByAchievementPercentageAsync(double minPercentage);
        Task<ServiceResponse<IEnumerable<CampaignResponseDto>>> GetCampaignsEndingSoonAsync(double remainingValue = 1000);

        // ===== Statistics & Analytics =====
        Task<ServiceResponse<int>> GetTotalCampaignsCountAsync(bool includeDeleted = false);
        Task<ServiceResponse<int>> GetTotalActiveCampaignsCountAsync();
        Task<ServiceResponse<int>> GetSoloCampaignsCountAsync();
        Task<ServiceResponse<int>> GetSharedCampaignsCountAsync();
        Task<ServiceResponse<double>> GetTotalMoneyRaisedAsync();
        Task<ServiceResponse<double>> GetAverageAchievementPercentageAsync();
        Task<ServiceResponse<Dictionary<CampaignType, int>>> GetCampaignCountByTypeAsync();
        Task<ServiceResponse<Dictionary<CampaignStatus, int>>> GetCampaignCountByStatusAsync();
        Task<ServiceResponse<CampaignStatisticsDto>> GetCampaignStatisticsAsync();

        // ===== Featured & Trending =====
        Task<ServiceResponse<IEnumerable<CampaignResponseDto>>> GetTopCampaignsByAchievementAsync(int limit = 10);
        Task<ServiceResponse<IEnumerable<CampaignResponseDto>>> GetTopCampaignsByDonationsAsync(int limit = 10);
        Task<ServiceResponse<IEnumerable<CampaignResponseDto>>> GetRecentCampaignsAsync(int days = 30);
        Task<ServiceResponse<IEnumerable<CampaignResponseDto>>> GetUrgentCampaignsAsync(double minPercentage = 75);

        // ===== Deadline Operations =====
        Task<ServiceResponse<IEnumerable<CampaignResponseDto>>> GetCampaignsByDeadlineAsync(DateTime deadlineDate, bool includeDeleted = false);
        Task<ServiceResponse<IEnumerable<CampaignResponseDto>>> GetExpiredCampaignsAsync();
        Task<ServiceResponse<IEnumerable<CampaignResponseDto>>> GetCampaignsExpiringSoonAsync(int daysThreshold = 7);
        Task<ServiceResponse<bool>> ExtendCampaignDeadlineAsync(int campaignId, DateTime newDeadline);
        Task<ServiceResponse<bool>> AutoExpireCampaignsAsync();

        // ===== Bulk Operations =====
        Task<ServiceResponse<int>> BulkUpdateCampaignStatusAsync(CampaignStatus oldStatus, CampaignStatus newStatus);
        Task<ServiceResponse<int>> SoftDeleteExpiredCampaignsAsync(int daysAfterCompletion = 30);
    }
}
