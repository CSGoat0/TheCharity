using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheCharityBLL.DTOs.DonationDTOs;
using TheCharityBLL.DTOs.PaginationDTOs;

namespace TheCharityBLL.Services.Abstraction.MoneyDonation
{
    public interface IDonationService
    {
        // ===== CRUD =====
        Task<PagedResultDto<DonationResponseDto>> GetAllDonationsAsync(PaginationParametersDto parametersDto,bool includeDeleted = false);
        Task<DonationResponseDto?> GetDonationByIdAsync(int id);
        Task<DonationResponseDto> CreateDonationAsync(CreateDonationDto dto);
        Task<DonationResponseDto?> UpdateDonationAsync(int id, UpdateDonationDto dto);
        Task<bool> DeleteDonationAsync(int id);
        Task<bool> RestoreDonationAsync(int id);

        // ===== Filtering & Search =====
        Task<PagedResultDto<DonationResponseDto>> GetDonationsByUserAsync(PaginationParametersDto parametersDto, string userId);
        Task<PagedResultDto<DonationResponseDto>> GetDonationsByCampaignAsync(PaginationParametersDto parametersDto, int campaignId);
        Task<PagedResultDto<DonationResponseDto>> GetDonationsByAmountRangeAsync(PaginationParametersDto parametersDto, double minAmount, double maxAmount);
        Task<PagedResultDto<DonationResponseDto>> GetDonationsByDateRangeAsync(PaginationParametersDto parametersDto, DateTime startDate, DateTime endDate);
        Task<PagedResultDto<DonationResponseDto>> GetRecentDonationsAsync(PaginationParametersDto parametersDto, int days = 30);
        Task<PagedResultDto<DonationResponseDto>> GetDeletedDonationsAsync(PaginationParametersDto parametersDto);

        // ===== Statistics =====
        Task<double> GetTotalDonationsAmountAsync();
        Task<double> GetTotalDonationsAmountByUserAsync(string userId);
        Task<double> GetTotalDonationsAmountByCampaignAsync(int campaignId);
        Task<int> GetTotalDonationsCountAsync();
        Task<int> GetDonationsCountByUserAsync(string userId);
        Task<int> GetDonationsCountByCampaignAsync(int campaignId);

        // ===== Advanced Analytics =====
        Task<double> GetAverageDonationAmountAsync();
        Task<double> GetAverageDonationAmountByUserAsync(string userId);
        Task<double> GetAverageDonationAmountByCampaignAsync(int campaignId);
        Task<Dictionary<string, double>> GetTopDonorsByAmountAsync(int limit = 10);
        Task<Dictionary<int, double>> GetTopCampaignsByDonationsAsync(int limit = 10);
        Task<Dictionary<DateTime, double>> GetDonationsTrendAsync(int days = 30);
        Task<Dictionary<string, int>> GetDonationFrequencyByUserAsync();

        // ===== Campaign-Specific =====
        Task<double> GetCampaignTotalRaisedAsync(int campaignId);
        Task<double> GetCampaignProgressPercentageAsync(int campaignId);
        Task<PagedResultDto<DonationResponseDto>> GetUsersDonationsOfACampaignAsync(PaginationParametersDto parametersDto, int campaignId);

        // ===== User-Specific =====
        Task<PagedResultDto<DonationResponseDto>> GetUserDonationHistoryAsync(PaginationParametersDto parametersDto, string userId);
        Task<DateTime?> GetUserLastDonationDateAsync(string userId);
        Task<IEnumerable<int>> GetCampaignsDonatedByUserAsync(string userId);

        // ===== Bulk Operations =====
        Task<int> TransferDonationsToCampaignAsync(int fromCampaignId, int toCampaignId);
        Task<int> DeleteOldDonationsAsync(int daysOld = 365);

        // ===== Validation & Checks =====
        Task<bool> DonationExistsAsync(int id);
        Task<bool> HasUserDonatedToCampaignAsync(string userId, int campaignId);

        // ===== Eager Loading =====
        Task<DonationResponseDto?> GetDonationWithDetailsAsync(int id);

        // ===== Dashboard & Reporting =====
        Task<PagedResultDto<DonationResponseDto>> GetLatestDonationsAsync(PaginationParametersDto parametersDto, int limit = 10);
        Task<PagedResultDto<DonationResponseDto>> GetLargestDonationsAsync(PaginationParametersDto parametersDto, int limit = 10);
        Task<Dictionary<int, int>> GetDonationsPerCampaignCountAsync();
        Task<Dictionary<string, int>> GetDonationsPerUserCountAsync();
        Task<double> GetTodayDonationsTotalAsync();
        Task<double> GetThisWeekDonationsTotalAsync();
        Task<double> GetThisMonthDonationsTotalAsync();

        // ===== Financial Reporting =====
        Task<Dictionary<string, double>> GetMonthlyDonationsReportAsync(int year);
        Task<Dictionary<string, double>> GetQuarterlyDonationsReportAsync(int year);
        Task<Dictionary<string, double>> GetYearlyDonationsReportAsync(int yearsBack = 5);
        Task<Dictionary<string, double>> GetDonationsByTimeOfDayAsync();
        Task<Dictionary<string, double>> GetDonationsByDayOfWeekAsync();

        // ===== Campaign Performance =====
        Task<Dictionary<DateTime, double>> GetCampaignDonationTimelineAsync(int campaignId);

        // ===== User Engagement =====
        Task<PagedResultDto<DonationResponseDto>> GetRecurringDonorsAsync(PaginationParametersDto parametersDto, int minDonations = 3);
        Task<PagedResultDto<DonationResponseDto>> GetFirstTimeDonorsAsync(PaginationParametersDto parametersDto, DateTime startDate, DateTime endDate);
        Task<Dictionary<string, double>> GetUserLifetimeValueAsync();
        Task<IEnumerable<string>> GetLoyalDonorsAsync(double minTotalAmount = 1000, int minDonations = 5);

        // ===== Search & Filter Combinations =====
        Task<PagedResultDto<DonationResponseDto>> SearchDonationsByUserAndCampaignAsync(PaginationParametersDto parametersDto, string userId, int campaignId);
        Task<PagedResultDto<DonationResponseDto>> GetDonationsByMultipleUsersAsync(PaginationParametersDto parametersDto, IEnumerable<string> userIds);
        Task<PagedResultDto<DonationResponseDto>> GetDonationsByMultipleCampaignsAsync(PaginationParametersDto parametersDto, IEnumerable<int> campaignIds);
        Task<PagedResultDto<DonationResponseDto>> GetDonationsByAmountAndDateAsync(PaginationParametersDto parametersDto, double minAmount, DateTime startDate);

        // ===== Audit =====
        Task<PagedResultDto<DonationResponseDto>> GetSuspiciousDonationsAsync(PaginationParametersDto parametersDto, double amountThreshold = 10000);

        // ===== Export =====
        Task<int> GetDonationRecordCountForPeriodAsync(DateTime startDate, DateTime endDate);
    }
}
