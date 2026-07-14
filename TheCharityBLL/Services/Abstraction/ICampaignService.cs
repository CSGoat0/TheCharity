using TheCharityBLL.DTOs;
using TheCharityBLL.DTOs.CampaignDTOs;
using TheCharityBLL.DTOs.PaginationDTOs;
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
        Task<ServiceResponse<PagedResultDto<SoloCampaignResponseDto>>> GetAllSoloCampaignsAsync(PaginationParametersDto parametersDto,bool includeDeleted = false);
        Task<ServiceResponse<SoloCampaignResponseDto>> GetSoloCampaignByIdAsync(int id);
        Task<ServiceResponse<int>> CreateSoloCampaignAsync(CreateSoloCampaignDto createDto);
        Task<ServiceResponse<bool>> UpdateSoloCampaignAsync(UpdateSoloCampaignDto updateDto);
        Task<ServiceResponse<PagedResultDto<SoloCampaignResponseDto>>> GetSoloCampaignsByOrganizationIdAsync(PaginationParametersDto parametersDto,int organizationId);
        Task<ServiceResponse<PagedResultDto<SoloCampaignResponseDto>>> GetSoloCampaignsByStatusAsync(PaginationParametersDto parametersDto, CampaignStatus status);

        // ===== Shared Campaign Operations =====
        Task<ServiceResponse<PagedResultDto<SharedCampaignResponseDto>>> GetAllSharedCampaignsAsync(PaginationParametersDto parametersDto, bool includeDeleted = false);
        Task<ServiceResponse<SharedCampaignResponseDto>> GetSharedCampaignByIdAsync(int id);
        Task<ServiceResponse<int>> CreateSharedCampaignAsync(CreateSharedCampaignDto createDto);
        Task<ServiceResponse<bool>> UpdateSharedCampaignAsync(UpdateSharedCampaignDto updateDto);
        Task<ServiceResponse<PagedResultDto<SharedCampaignResponseDto>>> GetSharedCampaignsByOrganizationIdAsync(PaginationParametersDto parametersDto, int organizationId);
        Task<ServiceResponse<PagedResultDto<SharedCampaignResponseDto>>> GetSharedCampaignsByStatusAsync(PaginationParametersDto parametersDto, CampaignStatus status);

        // ===== Shared Campaign Organization Management =====
        Task<ServiceResponse<bool>> AddOrganizationToSharedCampaignAsync(int sharedCampaignId, int organizationId);
        Task<ServiceResponse<bool>> RemoveOrganizationFromSharedCampaignAsync(int sharedCampaignId, int organizationId);
        Task<ServiceResponse<int>> GetOrganizationCountForSharedCampaignAsync(int sharedCampaignId);

        // ===== Campaign Progress Operations =====
        Task<ServiceResponse<bool>> UpdateCampaignMoneyAsync(int campaignId, double achievedAmount);
        Task<ServiceResponse<bool>> IncrementCampaignMoneyAsync(int campaignId, double amount);
        Task<ServiceResponse<bool>> UpdateCampaignStatusAsync(int campaignId, CampaignStatus status);

        // ===== Filtering & Querying =====
        Task<ServiceResponse<PagedResultDto<CampaignResponseDto>>> GetAllCampaignsAsync(PaginationParametersDto parametersDto, bool includeDeleted = false);
        Task<ServiceResponse<PagedResultDto<CampaignResponseDto>>> GetCampaignsByStatusAsync(PaginationParametersDto parametersDto, CampaignStatus status);
        Task<ServiceResponse<PagedResultDto<CampaignResponseDto>>> GetCampaignsByTypeAsync(PaginationParametersDto parametersDto, CampaignType type);
        Task<ServiceResponse<PagedResultDto<CampaignResponseDto>>> GetActiveCampaignsAsync(PaginationParametersDto parametersDto);
        Task<ServiceResponse<PagedResultDto<CampaignResponseDto>>> SearchCampaignsAsync(PaginationParametersDto parametersDto, string searchTerm);
        Task<ServiceResponse<PagedResultDto<CampaignResponseDto>>> GetDeletedCampaignsAsync(PaginationParametersDto parametersDto);

        // ===== Advanced Filtering =====
        Task<ServiceResponse<PagedResultDto<CampaignResponseDto>>> GetCampaignsByTargetRangeAsync(PaginationParametersDto parametersDto, double minTarget, double maxTarget);
        Task<ServiceResponse<PagedResultDto<CampaignResponseDto>>> GetCampaignsByAchievementPercentageAsync(PaginationParametersDto parametersDto, double minPercentage);
        Task<ServiceResponse<PagedResultDto<CampaignResponseDto>>> GetCampaignsEndingSoonAsync(PaginationParametersDto parametersDto, double remainingValue = 1000);

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
        Task<ServiceResponse<PagedResultDto<CampaignResponseDto>>> GetTopCampaignsByAchievementAsync(PaginationParametersDto parametersDto, int limit = 10);
        Task<ServiceResponse<PagedResultDto<CampaignResponseDto>>> GetTopCampaignsByDonationsAsync(PaginationParametersDto parametersDto, int limit = 10);
        Task<ServiceResponse<PagedResultDto<CampaignResponseDto>>> GetRecentCampaignsAsync(PaginationParametersDto parametersDto, int days = 30);
        Task<ServiceResponse<PagedResultDto<CampaignResponseDto>>> GetUrgentCampaignsAsync(PaginationParametersDto parametersDto, double minPercentage = 75);

        // ===== Deadline Operations =====
        Task<ServiceResponse<PagedResultDto<CampaignResponseDto>>> GetCampaignsByDeadlineAsync(PaginationParametersDto parametersDto, DateTime deadlineDate, bool includeDeleted = false);
        Task<ServiceResponse<PagedResultDto<CampaignResponseDto>>> GetExpiredCampaignsAsync(PaginationParametersDto parametersDto);
        Task<ServiceResponse<PagedResultDto<CampaignResponseDto>>> GetCampaignsExpiringSoonAsync(PaginationParametersDto parametersDto, int daysThreshold = 7);
        Task<ServiceResponse<bool>> ExtendCampaignDeadlineAsync(int campaignId, DateTime newDeadline);
        Task<ServiceResponse<bool>> AutoExpireCampaignsAsync();

        // ===== Bulk Operations =====
        Task<ServiceResponse<int>> BulkUpdateCampaignStatusAsync(CampaignStatus oldStatus, CampaignStatus newStatus);
        Task<ServiceResponse<int>> SoftDeleteExpiredCampaignsAsync(int daysAfterCompletion = 30);
    }
}
