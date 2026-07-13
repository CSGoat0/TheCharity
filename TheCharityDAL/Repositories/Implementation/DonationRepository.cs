using Microsoft.EntityFrameworkCore;
using TheCharityDAL.Database;
using TheCharityDAL.Entities;
using TheCharityDAL.Extensions;
using TheCharityDAL.Repositories.Abstraction;

namespace TheCharityDAL.Repositories.Implementation
{
    public class DonationRepository : IDonationRepository
    {
        private readonly TheCharityDbContext _context;

        public DonationRepository(TheCharityDbContext context)
        {
            _context = context;
        }

        // ===== Donation CRUD Operations =====
        public async Task<(IEnumerable<Donation> Data, int TotalCount)> GetAllDonationsAsync(int pageNumber, int pageSize, bool includeDeleted = false)
        {
            var query = _context.Donations
                .Include(d => d.User)
                .Include(d => d.Campaign)
                .AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(d => !d.IsDeleted);
            }

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<Donation?> GetDonationByIdAsync(int id)
        {
            return await _context.Donations
                .Where(d => d.Id == id && (d.IsDeleted == false))
                .Include(d => d.User)
                .Include(d => d.Campaign)
                .FirstOrDefaultAsync();
        }

        public async Task<Donation> AddDonationAsync(Donation donation)
        {
            _context.Donations.Add(donation);
            await _context.SaveChangesAsync();
            return donation;
        }

        public async Task<Donation> UpdateDonationAsync(Donation donation)
        {
            _context.Entry(donation).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return donation;
        }

        public async Task DeleteDonationAsync(int id)
        {
            var donation = await GetDonationByIdAsync(id);
            if (donation != null)
            {
                donation.Delete();
                await _context.SaveChangesAsync();
            }
        }

        public async Task RestoreDonationAsync(int id)
        {
            var donation = await _context.Donations
                .Where(d => d.Id == id && d.IsDeleted == true)
                .FirstOrDefaultAsync();

            if (donation != null)
            {
                donation.Restore();
                await _context.SaveChangesAsync();
            }
        }

        // ===== Donation Filtering & Search =====
        public async Task<(IEnumerable<Donation> Data, int TotalCount)> GetDonationsByUserAsync(int pageNumber, int pageSize, string userId)
        {
            var query = _context.Donations
                .Where(d => d.UserId == userId &&
                           (d.IsDeleted == false))
                .Include(d => d.Campaign)
                .OrderByDescending(d => d.RegistrationDate)
                .AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<(IEnumerable<Donation> Data, int TotalCount)> GetDonationsByCampaignAsync(int pageNumber, int pageSize, int campaignId)
        {
            var query = _context.Donations
                .Where(d => d.CampaignId == campaignId &&
                           (d.IsDeleted == false))
                .Include(d => d.User)
                .OrderByDescending(d => d.RegistrationDate)
                .AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<(IEnumerable<Donation> Data, int TotalCount)> GetDonationsByAmountRangeAsync(int pageNumber, int pageSize, double minAmount, double maxAmount)
        {
            var query = _context.Donations
                .Where(d => d.Amount >= minAmount &&
                           d.Amount <= maxAmount &&
                           (d.IsDeleted == false))
                .Include(d => d.User)
                .Include(d => d.Campaign)
                .OrderByDescending(d => d.Amount)
                .AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<(IEnumerable<Donation> Data, int TotalCount)> GetDonationsByDateRangeAsync(int pageNumber, int pageSize, DateTime startDate, DateTime endDate)
        {
            var query = _context.Donations
                .Where(d => d.RegistrationDate >= startDate &&
                           d.RegistrationDate <= endDate &&
                           (d.IsDeleted == false))
                .Include(d => d.User)
                .Include(d => d.Campaign)
                .OrderBy(d => d.RegistrationDate)
                .AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<(IEnumerable<Donation> Data, int TotalCount)> GetRecentDonationsAsync(int pageNumber, int pageSize, int days = 30)
        {
            var cutoffDate = DateTime.Now.AddDays(-days);

            var query = _context.Donations
                .Where(d => d.RegistrationDate >= cutoffDate &&
                           (d.IsDeleted == false))
                .Include(d => d.User)
                .Include(d => d.Campaign)
                .OrderByDescending(d => d.RegistrationDate)
                .AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<(IEnumerable<Donation> Data, int TotalCount)> GetDeletedDonationsAsync(int pageNumber, int pageSize)
        {
            var query = _context.Donations
                .Where(d => d.IsDeleted == true)
                .Include(d => d.User)
                .Include(d => d.Campaign)
                .AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        // ===== Donation Statistics =====
        public async Task<double> GetTotalDonationsAmountAsync()
        {
            return await _context.Donations
                .Where(d => d.IsDeleted == false)
                .SumAsync(d => d.Amount ?? 0);
        }

        public async Task<double> GetTotalDonationsAmountByUserAsync(string userId)
        {
            return await _context.Donations
                .Where(d => d.UserId == userId &&
                           (d.IsDeleted == false))
                .SumAsync(d => d.Amount ?? 0);
        }

        public async Task<double> GetTotalDonationsAmountByCampaignAsync(int campaignId)
        {
            return await _context.Donations
                .Where(d => d.CampaignId == campaignId &&
                           (d.IsDeleted == false))
                .SumAsync(d => d.Amount ?? 0);
        }

        public async Task<int> GetTotalDonationsCountAsync()
        {
            return await _context.Donations
                .Where(d => d.IsDeleted == false)
                .CountAsync();
        }

        public async Task<int> GetDonationsCountByUserAsync(string userId)
        {
            return await _context.Donations
                .Where(d => d.UserId == userId &&
                           (d.IsDeleted == false))
                .CountAsync();
        }

        public async Task<int> GetDonationsCountByCampaignAsync(int campaignId)
        {
            return await _context.Donations
                .Where(d => d.CampaignId == campaignId &&
                           (d.IsDeleted == false))
                .CountAsync();
        }

        // ===== Advanced Analytics =====
        public async Task<double> GetAverageDonationAmountAsync()
        {
            var totalAmount = await GetTotalDonationsAmountAsync();
            var totalCount = await GetTotalDonationsCountAsync();

            return totalCount > 0 ? totalAmount / totalCount : 0;
        }

        public async Task<double> GetAverageDonationAmountByUserAsync(string userId)
        {
            var totalAmount = await GetTotalDonationsAmountByUserAsync(userId);
            var totalCount = await GetDonationsCountByUserAsync(userId);

            return totalCount > 0 ? totalAmount / totalCount : 0;
        }

        public async Task<double> GetAverageDonationAmountByCampaignAsync(int campaignId)
        {
            var totalAmount = await GetTotalDonationsAmountByCampaignAsync(campaignId);
            var totalCount = await GetDonationsCountByCampaignAsync(campaignId);

            return totalCount > 0 ? totalAmount / totalCount : 0;
        }

        public async Task<Dictionary<string, double>> GetTopDonorsByAmountAsync(int limit = 10)
        {
            var result = await _context.Donations
                .Where(d => d.IsDeleted == false)
                .GroupBy(d => d.UserId)
                .Select(g => new { UserId = g.Key, TotalAmount = g.Sum(d => d.Amount ?? 0) })
                .OrderByDescending(x => x.TotalAmount)
                .Take(limit)
                .ToListAsync();

            return result
                .Where(x => !string.IsNullOrEmpty(x.UserId))
                .ToDictionary(x => x.UserId, x => x.TotalAmount);
        }

        public async Task<Dictionary<int, double>> GetTopCampaignsByDonationsAsync(int limit = 10)
        {
            var result = await _context.Donations
                .Where(d => d.IsDeleted == false)
                .GroupBy(d => d.CampaignId)
                .Select(g => new { CampaignId = g.Key, TotalAmount = g.Sum(d => d.Amount ?? 0) })
                .OrderByDescending(x => x.TotalAmount)
                .Take(limit)
                .ToListAsync();

            return result
                .ToDictionary(x => x.CampaignId, x => x.TotalAmount);
        }

        public async Task<Dictionary<DateTime, double>> GetDonationsTrendAsync(int days = 30)
        {
            var startDate = DateTime.Now.AddDays(-days).Date;

            var trend = await _context.Donations
                .Where(d => (d.IsDeleted == false) &&
                           d.RegistrationDate >= startDate)
                .GroupBy(d => d.RegistrationDate.Date)
                .Select(g => new { Date = g.Key, TotalAmount = g.Sum(d => d.Amount ?? 0) })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return trend.ToDictionary(x => x.Date, x => x.TotalAmount);
        }

        public async Task<Dictionary<string, int>> GetDonationFrequencyByUserAsync()
        {
            var result = await _context.Donations
                .Where(d => d.IsDeleted == false)
                .GroupBy(d => d.UserId)
                .Select(g => new { UserId = g.Key, Frequency = g.Count() })
                .OrderByDescending(x => x.Frequency)
                .ToListAsync();

            return result
                .Where(x => !string.IsNullOrEmpty(x.UserId))
                .ToDictionary(x => x.UserId, x => x.Frequency);
        }

        // ===== Campaign-Specific Operations =====
        public async Task<double> GetCampaignTotalRaisedAsync(int campaignId)
        {
            return await GetTotalDonationsAmountByCampaignAsync(campaignId);
        }

        public async Task<double> GetCampaignProgressPercentageAsync(int campaignId)
        {
            var campaign = await _context.Campaigns
                .Where(c => c.Id == campaignId &&
                           (c.IsDeleted == false))
                .FirstOrDefaultAsync();

            if (campaign == null || !campaign.Target.HasValue || campaign.Target.Value == 0)
                return 0;

            var totalRaised = await GetCampaignTotalRaisedAsync(campaignId);
            return (totalRaised / campaign.Target.Value) * 100;
        }

        public async Task<(IEnumerable<Donation> Data, int TotalCount)> GetUsersDonationsOfACampaignAsync(int pageNumber, int pageSize, int campaignId)
        {
            var query = _context.Donations
                .Where(d => d.CampaignId == campaignId &&
                           (d.IsDeleted == false))
                .Include(d => d.User)
                .AsQueryable();
            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        // ===== User-Specific Operations =====

        public async Task<(IEnumerable<Donation> Data, int TotalCount)> GetUserDonationHistoryAsync(int pageNumber, int pageSize, string userId)
        {
            return await GetDonationsByUserAsync(pageNumber,pageSize,userId);
        }

        public async Task<DateTime?> GetUserLastDonationDateAsync(string userId)
        {
            return await _context.Donations
                .Where(d => d.UserId == userId &&
                           (d.IsDeleted == false))
                .OrderByDescending(d => d.RegistrationDate)
                .Select(d => d.RegistrationDate)
                .FirstOrDefaultAsync();
        }

        public async Task<(IEnumerable<int> Data, int TotalCount)> GetCampaignsDonatedByUserAsync(int pageNumber, int pageSize, string userId)
        {
            var query = _context.Donations
                .Where(d => d.UserId == userId &&
                           (d.IsDeleted == false))
                .Select(d => d.CampaignId)
                .Distinct()
                .AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        // ===== Bulk Operations =====
        public async Task<int> TransferDonationsToCampaignAsync(int fromCampaignId, int toCampaignId)
        {
            var donations = await _context.Donations
                .Where(d => d.CampaignId == fromCampaignId &&
                           (d.IsDeleted == false))
                .ToListAsync();

            foreach (var donation in donations)
            {
                donation.EditCampaign(toCampaignId);
            }

            await _context.SaveChangesAsync();
            return donations.Count;
        }

        public async Task<int> DeleteOldDonationsAsync(int daysOld = 365)
        {
            var cutoffDate = DateTime.Now.AddDays(-daysOld);

            var donations = await _context.Donations
                .Where(d => (d.IsDeleted == false) &&
                           d.RegistrationDate < cutoffDate)
                .ToListAsync();

            foreach (var donation in donations)
            {
                donation.Delete();
            }

            await _context.SaveChangesAsync();
            return donations.Count;
        }

        // ===== Validation & Checks =====
        public async Task<bool> DonationExistsAsync(int id)
        {
            return await _context.Donations.AnyAsync(d => d.Id == id);
        }

        public async Task<bool> HasUserDonatedToCampaignAsync(string userId, int campaignId)
        {
            return await _context.Donations
                .AnyAsync(d => d.UserId == userId &&
                              d.CampaignId == campaignId &&
                              (d.IsDeleted == false));
        }

        public async Task<bool> IsDonationValidAsync(Donation donation)
        {
            // Check if user exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == donation.UserId);
            if (!userExists) return false;


            var campaignExists = await _context.Campaigns
                .AnyAsync(c => c.Id == donation.CampaignId);
            if (!campaignExists) return false;

            // Check amount
            if (!donation.Amount.HasValue || donation.Amount.Value <= 0)
                return false;

            return true;
        }

        // ===== Eager Loading =====
        public async Task<Donation?> GetDonationWithDetailsAsync(int id)
        {
            return await _context.Donations
                .Where(d => d.Id == id && (d.IsDeleted == false))
                .Include(d => d.User)
                .Include(d => d.Campaign)
                .FirstOrDefaultAsync();
        }

        // ===== Dashboard & Reporting =====
        public async Task<(IEnumerable<Donation> Data, int TotalCount)> GetLatestDonationsAsync(int pageNumber, int pageSize, int limit = 10)
        {
            var query = _context.Donations
                .Where(d => d.IsDeleted == false)
                .Include(d => d.User)
                .Include(d => d.Campaign)
                .OrderByDescending(d => d.RegistrationDate)
                .Take(limit)
                .AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<(IEnumerable<Donation> Data, int TotalCount)> GetLargestDonationsAsync(int pageNumber, int pageSize, int limit = 10)
        {
            var query = _context.Donations
                .Where(d => d.IsDeleted == false)
                .Include(d => d.User)
                .Include(d => d.Campaign)
                .OrderByDescending(d => d.Amount)
                .Take(limit)
                .AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<Dictionary<int, int>> GetDonationsPerCampaignCountAsync()
        {
            var result = await _context.Donations
                .Where(d => d.IsDeleted == false)
                .GroupBy(d => d.CampaignId)
                .Select(g => new { CampaignId = g.Key, Count = g.Count() })
                .ToListAsync();

            return result
                .ToDictionary(x => x.CampaignId, x => x.Count);
        }

        public async Task<Dictionary<string, int>> GetDonationsPerUserCountAsync()
        {
            var result = await _context.Donations
                .Where(d => d.IsDeleted == false)
                .GroupBy(d => d.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToListAsync();

            return result
                .Where(x => !string.IsNullOrEmpty(x.UserId))
                .ToDictionary(x => x.UserId, x => x.Count);
        }

        public async Task<double> GetTodayDonationsTotalAsync()
        {
            var today = DateTime.Now.Date;
            var tomorrow = today.AddDays(1);

            return await _context.Donations
                .Where(d => d.RegistrationDate >= today &&
                           d.RegistrationDate < tomorrow &&
                           (d.IsDeleted == false))
                .SumAsync(d => d.Amount ?? 0);
        }

        public async Task<double> GetThisWeekDonationsTotalAsync()
        {
            var startOfWeek = DateTime.Now.Date.AddDays(-(int)DateTime.Now.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(7);

            return await _context.Donations
                .Where(d => d.RegistrationDate >= startOfWeek &&
                           d.RegistrationDate < endOfWeek &&
                           (d.IsDeleted == false))
                .SumAsync(d => d.Amount ?? 0);
        }

        public async Task<double> GetThisMonthDonationsTotalAsync()
        {
            var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1);

            return await _context.Donations
                .Where(d => d.RegistrationDate >= startOfMonth &&
                           d.RegistrationDate < endOfMonth &&
                           (d.IsDeleted == false))
                .SumAsync(d => d.Amount ?? 0);
        }

        // ===== Financial Reporting =====
        public async Task<Dictionary<string, double>> GetMonthlyDonationsReportAsync(int year)
        {
            var result = new Dictionary<string, double>();

            for (int month = 1; month <= 12; month++)
            {
                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1);

                var monthlyTotal = await _context.Donations
                    .Where(d => d.RegistrationDate >= startDate &&
                               d.RegistrationDate < endDate &&
                               (d.IsDeleted == false))
                    .SumAsync(d => d.Amount ?? 0);

                result.Add(startDate.ToString("MMMM"), monthlyTotal);
            }

            return result;
        }

        public async Task<Dictionary<string, double>> GetQuarterlyDonationsReportAsync(int year)
        {
            var result = new Dictionary<string, double>();

            for (int quarter = 1; quarter <= 4; quarter++)
            {
                var startMonth = (quarter - 1) * 3 + 1;
                var startDate = new DateTime(year, startMonth, 1);
                var endDate = startDate.AddMonths(3);

                var quarterlyTotal = await _context.Donations
                    .Where(d => d.RegistrationDate >= startDate &&
                               d.RegistrationDate < endDate &&
                               (d.IsDeleted == false))
                    .SumAsync(d => d.Amount ?? 0);

                result.Add($"Q{quarter}", quarterlyTotal);
            }

            return result;
        }

        public async Task<Dictionary<string, double>> GetYearlyDonationsReportAsync(int yearsBack = 5)
        {
            var result = new Dictionary<string, double>();
            var currentYear = DateTime.Now.Year;

            for (int i = 0; i < yearsBack; i++)
            {
                var year = currentYear - i;
                var startDate = new DateTime(year, 1, 1);
                var endDate = new DateTime(year + 1, 1, 1);

                var yearlyTotal = await _context.Donations
                    .Where(d => d.RegistrationDate >= startDate &&
                               d.RegistrationDate < endDate &&
                               (d.IsDeleted == false))
                    .SumAsync(d => d.Amount ?? 0);

                result.Add(year.ToString(), yearlyTotal);
            }

            return result;
        }

        public async Task<Dictionary<string, double>> GetDonationsByTimeOfDayAsync()
        {
            var donations = await _context.Donations
                .Where(d => d.IsDeleted == false)
                .ToListAsync();

            var result = new Dictionary<string, double>
            {
                { "Morning (6AM-12PM)", 0 },
                { "Afternoon (12PM-6PM)", 0 },
                { "Evening (6PM-12AM)", 0 },
                { "Night (12AM-6AM)", 0 }
            };

            foreach (var donation in donations)
            {
                var hour = donation.RegistrationDate.Hour;
                var amount = donation.Amount ?? 0;

                if (hour >= 6 && hour < 12)
                    result["Morning (6AM-12PM)"] += amount;
                else if (hour >= 12 && hour < 18)
                    result["Afternoon (12PM-6PM)"] += amount;
                else if (hour >= 18 && hour < 24)
                    result["Evening (6PM-12AM)"] += amount;
                else
                    result["Night (12AM-6AM)"] += amount;
            }

            return result;
        }

        public async Task<Dictionary<string, double>> GetDonationsByDayOfWeekAsync()
        {
            var donations = await _context.Donations
                .Where(d => d.IsDeleted == false)
                .ToListAsync();

            var result = new Dictionary<string, double>
            {
                { "Monday", 0 },
                { "Tuesday", 0 },
                { "Wednesday", 0 },
                { "Thursday", 0 },
                { "Friday", 0 },
                { "Saturday", 0 },
                { "Sunday", 0 }
            };

            foreach (var donation in donations)
            {
                var day = donation.RegistrationDate.DayOfWeek.ToString();
                var amount = donation.Amount ?? 0;

                if (result.ContainsKey(day))
                    result[day] += amount;
            }

            return result;
        }

        // ===== Campaign Performance =====
        public async Task<Dictionary<DateTime, double>> GetCampaignDonationTimelineAsync(int campaignId)
        {
            const int pageNumber = 1;
            const int pageSize = int.MaxValue;

            var (donations, totalCount) = await GetDonationsByCampaignAsync(pageNumber, pageSize, campaignId);

            return donations
                .GroupBy(d => d.RegistrationDate.Date)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.Sum(d => d.Amount ?? 0));
        }

        // ===== User Engagement =====
        public async Task<(IEnumerable<Donation> Data, int TotalCount)> GetRecurringDonorsAsync(int pageNumber, int pageSize, int minDonations = 3)
        {
            // Get recurring user IDs
            var recurringUserIds = await _context.Donations
                .Where(d => d.IsDeleted == false)
                .GroupBy(d => d.UserId)
                .Where(g => g.Count() >= minDonations)
                .Select(g => g.Key)
                .ToListAsync();

            if (!recurringUserIds.Any())
                return (Enumerable.Empty<Donation>(), 0);

            // Single query to get all donations from recurring users
            var query = _context.Donations
                .Where(d => recurringUserIds.Contains(d.UserId) && !d.IsDeleted)
                .Include(d => d.User)
                .Include(d => d.Campaign)
                .OrderByDescending(d => d.RegistrationDate);

            var totalCount = await query.CountAsync();
            var pagedResult = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (pagedResult, totalCount);
        }

        public async Task<(IEnumerable<Donation> Data, int TotalCount)> GetFirstTimeDonorsAsync(int pageNumber, int pageSize, DateTime startDate, DateTime endDate)
        {
            // Get all donations in date range using the paged method with int.MaxValue
            var (donationsInRange, _) = await GetDonationsByDateRangeAsync(1, int.MaxValue, startDate, endDate);

            // Get users who donated before the start date
            var existingDonors = await _context.Donations
                .Where(d => d.RegistrationDate < startDate &&
                           (d.IsDeleted == false))
                .Select(d => d.UserId)
                .Distinct()
                .ToListAsync();

            // Filter donations to only include first-time donors
            var result = donationsInRange
                .Where(d => !existingDonors.Contains(d.UserId))
                .ToList();

            var totalCount = result.Count;
            result = result.Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (result, totalCount);
        }

        public async Task<Dictionary<string, double>> GetUserLifetimeValueAsync()
        {
            var result = await _context.Donations
                .Where(d => d.IsDeleted == false)
                .GroupBy(d => d.UserId)
                .Select(g => new { UserId = g.Key, LifetimeValue = g.Sum(d => d.Amount ?? 0) })
                .OrderByDescending(x => x.LifetimeValue)
                .ToListAsync();

            return result
                .Where(x => !string.IsNullOrEmpty(x.UserId))
                .ToDictionary(x => x.UserId, x => x.LifetimeValue);
        }

        public async Task<(IEnumerable<string> Data, int TotalCount)> GetLoyalDonorsAsync( int pageNumber, int pageSize, double minTotalAmount = 1000, int minDonations = 5)
        {
            var query = _context.Donations
                .Where(d => d.IsDeleted == false)
                .GroupBy(d => d.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    TotalAmount = g.Sum(d => d.Amount ?? 0),
                    DonationCount = g.Count()
                })
                .Where(x => x.TotalAmount >= minTotalAmount &&
                           x.DonationCount >= minDonations)
                .Select(x => x.UserId)
                .Where(id => !string.IsNullOrEmpty(id))
                .AsQueryable();

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        // ===== Search & Filter Combinations =====
        public async Task<(IEnumerable<Donation> Data, int TotalCount)> SearchDonationsByUserAndCampaignAsync(int pageNumber, int pageSize, string userId, int campaignId)
        {
            var query = _context.Donations
                .Where(d => d.UserId == userId &&
                           d.CampaignId == campaignId &&
                           (d.IsDeleted == false))
                .Include(d => d.User)
                .Include(d => d.Campaign)
                .OrderByDescending(d => d.RegistrationDate)
                .AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<(IEnumerable<Donation> Data, int TotalCount)> GetDonationsByMultipleUsersAsync(int pageNumber, int pageSize, IEnumerable<string> userIds)
        {
            var query = _context.Donations
                .Where(d => userIds.Contains(d.UserId) &&
                           (d.IsDeleted == false))
                .Include(d => d.User)
                .Include(d => d.Campaign)
                .OrderByDescending(d => d.RegistrationDate)
                .AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }
        public async Task<(IEnumerable<Donation> Data, int TotalCount)> GetDonationsByMultipleCampaignsAsync(int pageNumber, int pageSize, IEnumerable<int> campaignIds)
        {
            var query = _context.Donations
                .Where(d => campaignIds.Contains(d.CampaignId) &&
                           (d.IsDeleted == false))
                .Include(d => d.User)
                .Include(d => d.Campaign)
                .OrderByDescending(d => d.RegistrationDate)
                .AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<(IEnumerable<Donation> Data, int TotalCount)> GetDonationsByAmountAndDateAsync(int pageNumber, int pageSize, double minAmount, DateTime startDate)
        {
            var query = _context.Donations
                .Where(d => d.Amount >= minAmount &&
                           d.RegistrationDate >= startDate &&
                           (d.IsDeleted == false))
                .Include(d => d.User)
                .Include(d => d.Campaign)
                .OrderByDescending(d => d.Amount)
                .AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        // ===== Audit & Reconciliation =====
        public async Task<(IEnumerable<Donation> Data, int TotalCount)> GetSuspiciousDonationsAsync(int pageNumber, int pageSize, double amountThreshold = 10000)
        {
            var query = _context.Donations
                .Where(d => d.Amount >= amountThreshold &&
                           (d.IsDeleted == false))
                .Include(d => d.User)
                .Include(d => d.Campaign)
                .OrderByDescending(d => d.Amount)
                .AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        // ===== Export & Data Management =====
        public async Task<int> GetDonationRecordCountForPeriodAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Donations
                .Where(d => d.RegistrationDate >= startDate &&
                           d.RegistrationDate <= endDate &&
                           (d.IsDeleted == false))
                .CountAsync();
        }
    }
}
