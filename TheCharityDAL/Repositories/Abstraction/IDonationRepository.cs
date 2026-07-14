using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheCharityDAL.Entities;

namespace TheCharityDAL.Repositories.Abstraction
{
    public interface IDonationRepository
    {
        // ===== Donation CRUD Operations =====
        Task<IEnumerable<Donation>> GetAllDonationsAsync(bool includeDeleted = false);
        Task<Donation?> GetDonationByIdAsync(int id);
        Task<Donation> AddDonationAsync(Donation donation);
        Task<Donation> UpdateDonationAsync(Donation donation);
        Task DeleteDonationAsync(int id);
        Task RestoreDonationAsync(int id);

        // ===== Donation Filtering & Search =====
        Task<IEnumerable<Donation>> GetDonationsByUserAsync(string userId);
        Task<IEnumerable<Donation>> GetDonationsByCampaignAsync(int campaignId);
        Task<IEnumerable<Donation>> GetDonationsByAmountRangeAsync(double minAmount, double maxAmount);
        Task<IEnumerable<Donation>> GetDonationsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Donation>> GetRecentDonationsAsync(int days = 30);
        Task<IEnumerable<Donation>> GetDeletedDonationsAsync();

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
        Task<IEnumerable<Donation>> GetUsersDonationsOfACampaignAsync(int campaignId);

        // ===== User-Specific Operations =====
        Task<IEnumerable<Donation>> GetUserDonationHistoryAsync(string userId);
        Task<DateTime?> GetUserLastDonationDateAsync(string userId);
        Task<IEnumerable<int>> GetCampaignsDonatedByUserAsync(string userId);

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
        Task<IEnumerable<Donation>> GetLatestDonationsAsync(int limit = 10);
        Task<IEnumerable<Donation>> GetLargestDonationsAsync(int limit = 10);
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
        Task<IEnumerable<Donation>> GetRecurringDonorsAsync(int minDonations = 3);
        Task<IEnumerable<Donation>> GetFirstTimeDonorsAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<string, double>> GetUserLifetimeValueAsync();
        Task<IEnumerable<string>> GetLoyalDonorsAsync(double minTotalAmount = 1000, int minDonations = 5);

        // ===== Search & Filter Combinations =====
        Task<IEnumerable<Donation>> SearchDonationsByUserAndCampaignAsync(string userId, int campaignId);
        Task<IEnumerable<Donation>> GetDonationsByMultipleUsersAsync(IEnumerable<string> userIds);
        Task<IEnumerable<Donation>> GetDonationsByMultipleCampaignsAsync(IEnumerable<int> campaignIds);
        Task<IEnumerable<Donation>> GetDonationsByAmountAndDateAsync(double minAmount, DateTime startDate);

        // ===== Audit & Reconciliation =====
        Task<IEnumerable<Donation>> GetSuspiciousDonationsAsync(double amountThreshold = 10000);

        // ===== Export & Data Management =====
        Task<int> GetDonationRecordCountForPeriodAsync(DateTime startDate, DateTime endDate);
    }
}