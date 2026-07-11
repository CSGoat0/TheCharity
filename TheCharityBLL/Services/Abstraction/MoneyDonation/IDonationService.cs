using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheCharityBLL.DTOs;
using TheCharityBLL.DTOs.DonationDTOs;

namespace TheCharityBLL.Services.Abstraction.MoneyDonation
{
    public interface IDonationService
    {
        // ===== CRUD =====
        Task<ServiceResponse<IEnumerable<DonationResponseDto>>> GetAllDonationsAsync(bool includeDeleted = false);
        Task<ServiceResponse<DonationResponseDto?>> GetDonationByIdAsync(int id);
        Task<ServiceResponse<DonationResponseDto>> CreateDonationAsync(CreateDonationDto dto);
        Task<ServiceResponse<DonationResponseDto?>> UpdateDonationAsync(int id, UpdateDonationDto dto);
        Task<bool> DeleteDonationAsync(int id);
        Task<bool> RestoreDonationAsync(int id);

        // ===== Filtering & Search =====
        Task<ServiceResponse<IEnumerable<DonationResponseDto>>> GetDonationsByUserAsync(string userId);
        Task<ServiceResponse<IEnumerable <DonationResponseDto>>> GetDonationsByCampaignAsync(int campaignId);
        Task<ServiceResponse<IEnumerable <DonationResponseDto>>> GetDonationsByAmountRangeAsync(double minAmount, double maxAmount);
        Task<ServiceResponse<IEnumerable <DonationResponseDto>>> GetDonationsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<ServiceResponse< IEnumerable<DonationResponseDto>>> GetRecentDonationsAsync(int days = 30);
        Task<ServiceResponse< IEnumerable<DonationResponseDto>>> GetDeletedDonationsAsync();

        // ===== Statistics =====
        Task<ServiceResponse< double>> GetTotalDonationsAmountAsync();
        Task<ServiceResponse< double>> GetTotalDonationsAmountByUserAsync(string userId);
        Task<ServiceResponse< double>> GetTotalDonationsAmountByCampaignAsync(int campaignId);
        Task<ServiceResponse< int>> GetTotalDonationsCountAsync();
        Task<ServiceResponse< int>> GetDonationsCountByUserAsync(string userId);
        Task<ServiceResponse< int>> GetDonationsCountByCampaignAsync(int campaignId);

        // ===== Advanced Analytics =====
        Task<ServiceResponse< double>> GetAverageDonationAmountAsync();
        Task<ServiceResponse< double>> GetAverageDonationAmountByUserAsync(string userId);
        Task<ServiceResponse< double>> GetAverageDonationAmountByCampaignAsync(int campaignId);
        Task<ServiceResponse< Dictionary<string, double>>> GetTopDonorsByAmountAsync(int limit = 10);
        Task<ServiceResponse< Dictionary<int, double>>> GetTopCampaignsByDonationsAsync(int limit = 10);
        Task<ServiceResponse<Dictionary<DateTime, double>>> GetDonationsTrendAsync(int days = 30);
        Task<ServiceResponse< Dictionary<string, int>>> GetDonationFrequencyByUserAsync();

        // ===== Campaign-Specific =====
        Task<ServiceResponse< double>> GetCampaignTotalRaisedAsync(int campaignId);
        Task<ServiceResponse< double>> GetCampaignProgressPercentageAsync(int campaignId);
        Task<ServiceResponse< IEnumerable<DonationResponseDto>>> GetUsersDonationsOfACampaignAsync(int campaignId);

        // ===== User-Specific =====
        Task<ServiceResponse< IEnumerable<DonationResponseDto>>> GetUserDonationHistoryAsync(string userId);
        Task<ServiceResponse< DateTime?>> GetUserLastDonationDateAsync(string userId);
        Task<ServiceResponse< IEnumerable<int>>> GetCampaignsDonatedByUserAsync(string userId);

        // ===== Bulk Operations =====
        Task<ServiceResponse< int>> TransferDonationsToCampaignAsync(int fromCampaignId, int toCampaignId);
        Task<ServiceResponse< int>> DeleteOldDonationsAsync(int daysOld = 365);

        // ===== Validation & Checks =====
        Task<ServiceResponse< bool>> DonationExistsAsync(int id);
        Task<ServiceResponse< bool>> HasUserDonatedToCampaignAsync(string userId, int campaignId);

        // ===== Eager Loading =====
        Task<ServiceResponse< DonationResponseDto?>> GetDonationWithDetailsAsync(int id);

        // ===== Dashboard & Reporting =====
        Task<ServiceResponse< IEnumerable<DonationResponseDto>>> GetLatestDonationsAsync(int limit = 10);
        Task< ServiceResponse<IEnumerable<DonationResponseDto>>> GetLargestDonationsAsync(int limit = 10);
        Task<ServiceResponse<Dictionary<int, int>>> GetDonationsPerCampaignCountAsync();
        Task<ServiceResponse< Dictionary<string, int>>> GetDonationsPerUserCountAsync();
        Task<ServiceResponse< double>> GetTodayDonationsTotalAsync();
        Task<ServiceResponse< double>> GetThisWeekDonationsTotalAsync();
        Task<ServiceResponse< double>> GetThisMonthDonationsTotalAsync();

        // ===== Financial Reporting =====
        Task<ServiceResponse<Dictionary<string, double>>> GetMonthlyDonationsReportAsync(int year);
        Task<ServiceResponse<Dictionary<string, double>>> GetYearlyDonationsReportAsync(int yearsBack = 5);
        Task<ServiceResponse<Dictionary<string, double>>> GetDonationsByTimeOfDayAsync();
        Task<ServiceResponse<Dictionary<string, double>>> GetQuarterlyDonationsReportAsync(int year);
        Task<ServiceResponse<Dictionary<string, double>>> GetDonationsByDayOfWeekAsync();

        // ===== Campaign Performance =====
        Task<ServiceResponse<Dictionary<DateTime, double>>> GetCampaignDonationTimelineAsync(int campaignId);

        // ===== User Engagement =====
        Task<ServiceResponse<IEnumerable<DonationResponseDto>>> GetRecurringDonorsAsync(int minDonations = 3);
        Task<ServiceResponse<IEnumerable<DonationResponseDto>>> GetFirstTimeDonorsAsync(DateTime startDate, DateTime endDate);
        Task<ServiceResponse<Dictionary<string, double>>> GetUserLifetimeValueAsync();
        Task<ServiceResponse<IEnumerable<string>>> GetLoyalDonorsAsync(double minTotalAmount = 1000, int minDonations = 5);

        // ===== Search & Filter Combinations =====
        Task<ServiceResponse<IEnumerable<DonationResponseDto>>> SearchDonationsByUserAndCampaignAsync(string userId, int campaignId);
        Task<ServiceResponse<IEnumerable<DonationResponseDto>>> GetDonationsByMultipleUsersAsync(IEnumerable<string> userIds);
        Task<ServiceResponse<IEnumerable<DonationResponseDto>>> GetDonationsByMultipleCampaignsAsync(IEnumerable<int> campaignIds);
        Task<ServiceResponse<IEnumerable<DonationResponseDto>>> GetDonationsByAmountAndDateAsync(double minAmount, DateTime startDate);

        // ===== Audit =====
        Task<ServiceResponse<IEnumerable<DonationResponseDto>>> GetSuspiciousDonationsAsync(double amountThreshold = 10000);

        // ===== Export =====
        Task<ServiceResponse<int>> GetDonationRecordCountForPeriodAsync(DateTime startDate, DateTime endDate);
    }
}
