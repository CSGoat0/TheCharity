using TheCharityBLL.DTOs;
using TheCharityBLL.DTOs.DonationDTOs;
using TheCharityBLL.DTOs.PaginationDTOs;

namespace TheCharityBLL.Services.Abstraction.MoneyDonation
{
    public interface IDonationService
    {
        // ===== CRUD =====
        Task<ServiceResponse<PagedResultDto<DonationResponseDto>>> GetAllDonationsAsync(PaginationParametersDto parametersDto, bool includeDeleted = false);
        Task<ServiceResponse<DonationResponseDto?>> GetDonationByIdAsync(int id);
        Task<ServiceResponse<DonationResponseDto>> CreateDonationAsync(CreateDonationDto dto);
        Task<ServiceResponse<DonationResponseDto?>> UpdateDonationAsync(int id, UpdateDonationDto dto);
        Task<ServiceResponse<bool>> DeleteDonationAsync(int id);
        Task<ServiceResponse<bool>> RestoreDonationAsync(int id);

        // ===== Filtering & Search =====
        Task<ServiceResponse<PagedResultDto<DonationResponseDto>>> GetDonationsByUserAsync(PaginationParametersDto parametersDto, string userId);
        Task<ServiceResponse<PagedResultDto<DonationResponseDto>>> GetDonationsByCampaignAsync(PaginationParametersDto parametersDto, int campaignId);
        Task<ServiceResponse<PagedResultDto<DonationResponseDto>>> GetDonationsByAmountRangeAsync(PaginationParametersDto parametersDto, double minAmount, double maxAmount);
        Task<ServiceResponse<PagedResultDto<DonationResponseDto>>> GetDonationsByDateRangeAsync(PaginationParametersDto parametersDto, DateTime startDate, DateTime endDate);
        Task<ServiceResponse<PagedResultDto<DonationResponseDto>>> GetRecentDonationsAsync(PaginationParametersDto parametersDto, int days = 30);
        Task<ServiceResponse<PagedResultDto<DonationResponseDto>>> GetDeletedDonationsAsync(PaginationParametersDto parametersDto);

        // ===== Statistics =====
        Task<ServiceResponse<double>> GetTotalDonationsAmountAsync();
        Task<ServiceResponse<double>> GetTotalDonationsAmountByUserAsync(string userId);
        Task<ServiceResponse<double>> GetTotalDonationsAmountByCampaignAsync(int campaignId);
        Task<ServiceResponse<int>> GetTotalDonationsCountAsync();
        Task<ServiceResponse<int>> GetDonationsCountByUserAsync(string userId);
        Task<ServiceResponse<int>> GetDonationsCountByCampaignAsync(int campaignId);

        // ===== Advanced Analytics =====
        Task<ServiceResponse<double>> GetAverageDonationAmountAsync();
        Task<ServiceResponse<double>> GetAverageDonationAmountByUserAsync(string userId);
        Task<ServiceResponse<double>> GetAverageDonationAmountByCampaignAsync(int campaignId);
        Task<ServiceResponse<Dictionary<string, double>>> GetTopDonorsByAmountAsync(int limit = 10);
        Task<ServiceResponse<Dictionary<int, double>>> GetTopCampaignsByDonationsAsync(int limit = 10);
        Task<ServiceResponse<Dictionary<DateTime, double>>> GetDonationsTrendAsync(int days = 30);
        Task<ServiceResponse<Dictionary<string, int>>> GetDonationFrequencyByUserAsync();

        // ===== Campaign-Specific =====
        Task<ServiceResponse<double>> GetCampaignTotalRaisedAsync(int campaignId);
        Task<ServiceResponse<double>> GetCampaignProgressPercentageAsync(int campaignId);
        Task<ServiceResponse<PagedResultDto<DonationResponseDto>>> GetUsersDonationsOfACampaignAsync(PaginationParametersDto parametersDto, int campaignId);

        // ===== User-Specific =====
        Task<ServiceResponse<PagedResultDto<DonationResponseDto>>> GetUserDonationHistoryAsync(PaginationParametersDto parametersDto, string userId);
        Task<ServiceResponse<DateTime?>> GetUserLastDonationDateAsync(string userId);
        Task<ServiceResponse<PagedResultDto<int>>> GetCampaignsDonatedByUserAsync(PaginationParametersDto parametersDto, string userId);

        // ===== Bulk Operations =====
        Task<ServiceResponse<int>> TransferDonationsToCampaignAsync(int fromCampaignId, int toCampaignId);
        Task<ServiceResponse<int>> DeleteOldDonationsAsync(int daysOld = 365);

        // ===== Validation & Checks =====
        Task<ServiceResponse<bool>> DonationExistsAsync(int id);
        Task<ServiceResponse<bool>> HasUserDonatedToCampaignAsync(string userId, int campaignId);

        // ===== Eager Loading =====
        Task<ServiceResponse<DonationResponseDto?>> GetDonationWithDetailsAsync(int id);

        // ===== Dashboard & Reporting =====
        Task<ServiceResponse<PagedResultDto<DonationResponseDto>>> GetLatestDonationsAsync(PaginationParametersDto parametersDto, int limit = 10);
        Task<ServiceResponse<PagedResultDto<DonationResponseDto>>> GetLargestDonationsAsync(PaginationParametersDto parametersDto, int limit = 10);
        Task<ServiceResponse<Dictionary<int, int>>> GetDonationsPerCampaignCountAsync();
        Task<ServiceResponse<Dictionary<string, int>>> GetDonationsPerUserCountAsync();
        Task<ServiceResponse<double>> GetTodayDonationsTotalAsync();
        Task<ServiceResponse<double>> GetThisWeekDonationsTotalAsync();
        Task<ServiceResponse<double>> GetThisMonthDonationsTotalAsync();

        // ===== Financial Reporting =====
        Task<ServiceResponse<Dictionary<string, double>>> GetMonthlyDonationsReportAsync(int year);
        Task<ServiceResponse<Dictionary<string, double>>> GetQuarterlyDonationsReportAsync(int year);
        Task<ServiceResponse<Dictionary<string, double>>> GetYearlyDonationsReportAsync(int yearsBack = 5);
        Task<ServiceResponse<Dictionary<string, double>>> GetDonationsByTimeOfDayAsync();
        Task<ServiceResponse<Dictionary<string, double>>> GetDonationsByDayOfWeekAsync();

        // ===== Campaign Performance =====
        Task<ServiceResponse<Dictionary<DateTime, double>>> GetCampaignDonationTimelineAsync(int campaignId);

        // ===== User Engagement =====
        Task<ServiceResponse<PagedResultDto<DonationResponseDto>>> GetRecurringDonorsAsync(PaginationParametersDto parametersDto, int minDonations = 3);
        Task<ServiceResponse<PagedResultDto<DonationResponseDto>>> GetFirstTimeDonorsAsync(PaginationParametersDto parametersDto, DateTime startDate, DateTime endDate);
        Task<ServiceResponse<Dictionary<string, double>>> GetUserLifetimeValueAsync();
        Task<ServiceResponse<PagedResultDto<string>>> GetLoyalDonorsAsync(PaginationParametersDto parametersDto, double minTotalAmount = 1000, int minDonations = 5);

        // ===== Search & Filter Combinations =====
        Task<ServiceResponse<PagedResultDto<DonationResponseDto>>> SearchDonationsByUserAndCampaignAsync(PaginationParametersDto parametersDto, string userId, int campaignId);
        Task<ServiceResponse<PagedResultDto<DonationResponseDto>>> GetDonationsByMultipleUsersAsync(PaginationParametersDto parametersDto, IEnumerable<string> userIds);
        Task<ServiceResponse<PagedResultDto<DonationResponseDto>>> GetDonationsByMultipleCampaignsAsync(PaginationParametersDto parametersDto, IEnumerable<int> campaignIds);
        Task<ServiceResponse<PagedResultDto<DonationResponseDto>>> GetDonationsByAmountAndDateAsync(PaginationParametersDto parametersDto, double minAmount, DateTime startDate);

        // ===== Audit =====
        Task<ServiceResponse<PagedResultDto<DonationResponseDto>>> GetSuspiciousDonationsAsync(PaginationParametersDto parametersDto, double amountThreshold = 10000);

        // ===== Export =====
        Task<ServiceResponse<int>> GetDonationRecordCountForPeriodAsync(DateTime startDate, DateTime endDate);
    }
}