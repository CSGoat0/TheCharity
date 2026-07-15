using TheCharityDAL.Entities;

namespace TheCharityDAL.Repositories.Abstraction
{
    public interface IDonationRepository
    {
        // ===== Donation CRUD Operations =====
        Task<(IEnumerable<Donation> Data, int TotalCount)> GetAllDonationsAsync(int pageNumber, int pageSize, bool includeDeleted = false);
        Task<Donation?> GetDonationByIdAsync(int id);
        Task<Donation> AddDonationAsync(Donation donation);
        Task<Donation> UpdateDonationAsync(Donation donation);
        Task DeleteDonationAsync(int id);
        Task RestoreDonationAsync(int id);

        // ===== Donation Filtering & Search =====
        Task<(IEnumerable<Donation> Data, int TotalCount)> GetDonationsByUserAsync(int pageNumber, int pageSize, string userId);
        Task<(IEnumerable<Donation> Data, int TotalCount)> GetDonationsByCampaignAsync(int pageNumber, int pageSize, int campaignId);
        Task<(IEnumerable<Donation> Data, int TotalCount)> GetDonationsByAmountRangeAsync(int pageNumber, int pageSize, double minAmount, double maxAmount);
        Task<(IEnumerable<Donation> Data, int TotalCount)> GetDonationsByDateRangeAsync(int pageNumber, int pageSize, DateTime startDate, DateTime endDate);
        Task<(IEnumerable<Donation> Data, int TotalCount)> GetRecentDonationsAsync(int pageNumber, int pageSize, int days = 30);
        Task<(IEnumerable<Donation> Data, int TotalCount)> GetDeletedDonationsAsync(int pageNumber, int pageSize);

        // ===== Donation Statistics =====
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

        // ===== Campaign-Specific Operations =====
        Task<double> GetCampaignTotalRaisedAsync(int campaignId);
        Task<double> GetCampaignProgressPercentageAsync(int campaignId);
        Task<(IEnumerable<Donation> Data, int TotalCount)> GetUsersDonationsOfACampaignAsync(int pageNumber, int pageSize, int campaignId);

        // ===== User-Specific Operations =====
        Task<(IEnumerable<Donation> Data, int TotalCount)> GetUserDonationHistoryAsync(int pageNumber, int pageSize, string userId);
        Task<DateTime?> GetUserLastDonationDateAsync(string userId);
        Task<(IEnumerable<int> Data, int TotalCount)> GetCampaignsDonatedByUserAsync(int pageNumber, int pageSize, string userId);

        // ===== Bulk Operations =====
        Task<int> TransferDonationsToCampaignAsync(int fromCampaignId, int toCampaignId);
        Task<int> DeleteOldDonationsAsync(int daysOld = 365);

        // ===== Validation & Checks =====
        Task<bool> DonationExistsAsync(int id);
        Task<bool> HasUserDonatedToCampaignAsync(string userId, int campaignId);
        Task<bool> IsDonationValidAsync(Donation donation);

        // ===== Eager Loading =====
        Task<Donation?> GetDonationWithDetailsAsync(int id);

        // ===== Dashboard & Reporting =====
        Task<(IEnumerable<Donation> Data, int TotalCount)> GetLatestDonationsAsync(int pageNumber, int pageSize, int limit = 10);
        Task<(IEnumerable<Donation> Data, int TotalCount)> GetLargestDonationsAsync(int pageNumber, int pageSize, int limit = 10);
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
        Task<(IEnumerable<Donation> Data, int TotalCount)> GetRecurringDonorsAsync(int pageNumber, int pageSize, int minDonations = 3);
        Task<(IEnumerable<Donation> Data, int TotalCount)> GetFirstTimeDonorsAsync(int pageNumber, int pageSize, DateTime startDate, DateTime endDate);
        Task<Dictionary<string, double>> GetUserLifetimeValueAsync();
        Task<(IEnumerable<string> Data, int TotalCount)> GetLoyalDonorsAsync(int pageNumber, int pageSize, double minTotalAmount = 1000, int minDonations = 5);

        // ===== Search & Filter Combinations =====
        Task<(IEnumerable<Donation> Data, int TotalCount)> SearchDonationsByUserAndCampaignAsync(int pageNumber, int pageSize, string userId, int campaignId);
        Task<(IEnumerable<Donation> Data, int TotalCount)> GetDonationsByMultipleUsersAsync(int pageNumber, int pageSize, IEnumerable<string> userIds);
        Task<(IEnumerable<Donation> Data, int TotalCount)> GetDonationsByMultipleCampaignsAsync(int pageNumber, int pageSize, IEnumerable<int> campaignIds);
        Task<(IEnumerable<Donation> Data, int TotalCount)> GetDonationsByAmountAndDateAsync(int pageNumber, int pageSize, double minAmount, DateTime startDate);

        // ===== Audit & Reconciliation =====
        Task<(IEnumerable<Donation> Data, int TotalCount)> GetSuspiciousDonationsAsync(int pageNumber, int pageSize, double amountThreshold = 10000);

        // ===== Export & Data Management =====
        Task<int> GetDonationRecordCountForPeriodAsync(DateTime startDate, DateTime endDate);
    }
}