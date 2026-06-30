using TheCharityBLL.DTOs.DonationDTOs;
using TheCharityBLL.Events.Abstraction;
using TheCharityBLL.Events.Events.DonationEvents;
using TheCharityBLL.Mapper;
using TheCharityBLL.Services.Abstraction.MoneyDonation;
using TheCharityDAL.Repositories.Abstraction;

namespace TheCharityBLL.Services.Implementation.MoneyDonation
{
    public class DonationService : IDonationService
    {
        private readonly IDonationRepository _repo;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly DonationMapper _mapper;

        public DonationService(IDonationRepository repo, DonationMapper mapper, IEventDispatcher eventDispatcher)
        {
            _repo = repo;
            _mapper = mapper;
            _eventDispatcher = eventDispatcher;
        }

        // ===== CRUD =====

        public async Task<IEnumerable<DonationResponseDto>> GetAllDonationsAsync(bool includeDeleted = false)
        {
            var donations = await _repo.GetAllDonationsAsync(includeDeleted);
            return _mapper.MapToDonationResponseDtos(donations);
        }

        public async Task<DonationResponseDto?> GetDonationByIdAsync(int id)
        {
            var donation = await _repo.GetDonationByIdAsync(id);
            return donation is null ? null : _mapper.MapToDonationResponseDto(donation);
        }

        public async Task<DonationResponseDto> CreateDonationAsync(CreateDonationDto dto)
        {
            var entity = _mapper.MapToDonation(dto);

            var isValid = await _repo.IsDonationValidAsync(entity);
            if (!isValid)
                throw new InvalidOperationException("Donation data is invalid.");

            if (dto.CampaignId.HasValue && dto.Amount.HasValue)
                await _eventDispatcher.DispatchAsync(new CampaignDonationReceivedEvent
                {
                    CampaignId = dto.CampaignId.Value,
                    Amount = dto.Amount.Value
                });

            var created = await _repo.AddDonationAsync(entity);
            return _mapper.MapToDonationResponseDto(created);
        }

        public async Task<DonationResponseDto?> UpdateDonationAsync(int id, UpdateDonationDto dto)
        {
            var donation = await _repo.GetDonationByIdAsync(id);
            if (donation is null) return null;

            donation.EditAmount(dto.Amount);

            if (dto.CampaignId.HasValue)
                donation.EditCampaign(dto.CampaignId.Value);

            var isValid = await _repo.IsDonationValidAsync(donation);
            if (!isValid)
                throw new InvalidOperationException("Updated donation data is invalid.");

            var updated = await _repo.UpdateDonationAsync(donation);
            return _mapper.MapToDonationResponseDto(updated);
        }

        public async Task<bool> DeleteDonationAsync(int id)
        {
            var exists = await _repo.DonationExistsAsync(id);
            if (!exists) return false;

            await _repo.DeleteDonationAsync(id);
            return true;
        }

        public async Task<bool> RestoreDonationAsync(int id)
        {
            var deleted = await _repo.GetDeletedDonationsAsync();
            if (!deleted.Any(d => d.Id == id)) return false;

            await _repo.RestoreDonationAsync(id);
            return true;
        }

        // ===== Filtering & Search =====

        public async Task<IEnumerable<DonationResponseDto>> GetDonationsByUserAsync(string userId)
            => _mapper.MapToDonationResponseDtos(await _repo.GetDonationsByUserAsync(userId));

        public async Task<IEnumerable<DonationResponseDto>> GetDonationsByCampaignAsync(int campaignId)
            => _mapper.MapToDonationResponseDtos(await _repo.GetDonationsByCampaignAsync(campaignId));

        public async Task<IEnumerable<DonationResponseDto>> GetDonationsByAmountRangeAsync(double minAmount, double maxAmount)
            => _mapper.MapToDonationResponseDtos(await _repo.GetDonationsByAmountRangeAsync(minAmount, maxAmount));

        public async Task<IEnumerable<DonationResponseDto>> GetDonationsByDateRangeAsync(DateTime startDate, DateTime endDate)
            => _mapper.MapToDonationResponseDtos(await _repo.GetDonationsByDateRangeAsync(startDate, endDate));

        public async Task<IEnumerable<DonationResponseDto>> GetRecentDonationsAsync(int days = 30)
            => _mapper.MapToDonationResponseDtos(await _repo.GetRecentDonationsAsync(days));

        public async Task<IEnumerable<DonationResponseDto>> GetDeletedDonationsAsync()
            => _mapper.MapToDonationResponseDtos(await _repo.GetDeletedDonationsAsync());

        // ===== Statistics =====

        public Task<double> GetTotalDonationsAmountAsync()
            => _repo.GetTotalDonationsAmountAsync();

        public Task<double> GetTotalDonationsAmountByUserAsync(string userId)
            => _repo.GetTotalDonationsAmountByUserAsync(userId);

        public Task<double> GetTotalDonationsAmountByCampaignAsync(int campaignId)
            => _repo.GetTotalDonationsAmountByCampaignAsync(campaignId);

        public Task<int> GetTotalDonationsCountAsync()
            => _repo.GetTotalDonationsCountAsync();

        public Task<int> GetDonationsCountByUserAsync(string userId)
            => _repo.GetDonationsCountByUserAsync(userId);

        public Task<int> GetDonationsCountByCampaignAsync(int campaignId)
            => _repo.GetDonationsCountByCampaignAsync(campaignId);

        // ===== Advanced Analytics =====

        public Task<double> GetAverageDonationAmountAsync()
            => _repo.GetAverageDonationAmountAsync();

        public Task<double> GetAverageDonationAmountByUserAsync(string userId)
            => _repo.GetAverageDonationAmountByUserAsync(userId);

        public Task<double> GetAverageDonationAmountByCampaignAsync(int campaignId)
            => _repo.GetAverageDonationAmountByCampaignAsync(campaignId);

        public Task<Dictionary<string, double>> GetTopDonorsByAmountAsync(int limit = 10)
            => _repo.GetTopDonorsByAmountAsync(limit);

        public Task<Dictionary<int, double>> GetTopCampaignsByDonationsAsync(int limit = 10)
            => _repo.GetTopCampaignsByDonationsAsync(limit);

        public Task<Dictionary<DateTime, double>> GetDonationsTrendAsync(int days = 30)
            => _repo.GetDonationsTrendAsync(days);

        public Task<Dictionary<string, int>> GetDonationFrequencyByUserAsync()
            => _repo.GetDonationFrequencyByUserAsync();

        // ===== Campaign-Specific =====

        public Task<double> GetCampaignTotalRaisedAsync(int campaignId)
            => _repo.GetCampaignTotalRaisedAsync(campaignId);

        public Task<double> GetCampaignProgressPercentageAsync(int campaignId)
            => _repo.GetCampaignProgressPercentageAsync(campaignId);

        public async Task<IEnumerable<DonationResponseDto>> GetUsersDonationsOfACampaignAsync(int campaignId)
            => _mapper.MapToDonationResponseDtos(await _repo.GetUsersDonationsOfACampaignAsync(campaignId));

        // ===== User-Specific =====

        public async Task<IEnumerable<DonationResponseDto>> GetUserDonationHistoryAsync(string userId)
            => _mapper.MapToDonationResponseDtos(await _repo.GetUserDonationHistoryAsync(userId));

        public Task<DateTime?> GetUserLastDonationDateAsync(string userId)
            => _repo.GetUserLastDonationDateAsync(userId);

        public Task<IEnumerable<int>> GetCampaignsDonatedByUserAsync(string userId)
            => _repo.GetCampaignsDonatedByUserAsync(userId);

        // ===== Bulk Operations =====

        public Task<int> TransferDonationsToCampaignAsync(int fromCampaignId, int toCampaignId)
            => _repo.TransferDonationsToCampaignAsync(fromCampaignId, toCampaignId);

        public Task<int> DeleteOldDonationsAsync(int daysOld = 365)
            => _repo.DeleteOldDonationsAsync(daysOld);

        // ===== Validation =====

        public Task<bool> DonationExistsAsync(int id)
            => _repo.DonationExistsAsync(id);

        public Task<bool> HasUserDonatedToCampaignAsync(string userId, int campaignId)
            => _repo.HasUserDonatedToCampaignAsync(userId, campaignId);

        // ===== Eager Loading =====

        public async Task<DonationResponseDto?> GetDonationWithDetailsAsync(int id)
        {
            var donation = await _repo.GetDonationWithDetailsAsync(id);
            return donation is null ? null : _mapper.MapToDonationResponseDto(donation);
        }

        // ===== Dashboard & Reporting =====

        public async Task<IEnumerable<DonationResponseDto>> GetLatestDonationsAsync(int limit = 10)
            => _mapper.MapToDonationResponseDtos(await _repo.GetLatestDonationsAsync(limit));

        public async Task<IEnumerable<DonationResponseDto>> GetLargestDonationsAsync(int limit = 10)
            => _mapper.MapToDonationResponseDtos(await _repo.GetLargestDonationsAsync(limit));

        public Task<Dictionary<int, int>> GetDonationsPerCampaignCountAsync()
            => _repo.GetDonationsPerCampaignCountAsync();

        public Task<Dictionary<string, int>> GetDonationsPerUserCountAsync()
            => _repo.GetDonationsPerUserCountAsync();

        public Task<double> GetTodayDonationsTotalAsync()
            => _repo.GetTodayDonationsTotalAsync();

        public Task<double> GetThisWeekDonationsTotalAsync()
            => _repo.GetThisWeekDonationsTotalAsync();

        public Task<double> GetThisMonthDonationsTotalAsync()
            => _repo.GetThisMonthDonationsTotalAsync();

        // ===== Financial Reporting =====

        public Task<Dictionary<string, double>> GetMonthlyDonationsReportAsync(int year)
            => _repo.GetMonthlyDonationsReportAsync(year);

        public Task<Dictionary<string, double>> GetQuarterlyDonationsReportAsync(int year)
            => _repo.GetQuarterlyDonationsReportAsync(year);

        public Task<Dictionary<string, double>> GetYearlyDonationsReportAsync(int yearsBack = 5)
            => _repo.GetYearlyDonationsReportAsync(yearsBack);

        public Task<Dictionary<string, double>> GetDonationsByTimeOfDayAsync()
            => _repo.GetDonationsByTimeOfDayAsync();

        public Task<Dictionary<string, double>> GetDonationsByDayOfWeekAsync()
            => _repo.GetDonationsByDayOfWeekAsync();

        // ===== Campaign Performance =====

        public Task<Dictionary<DateTime, double>> GetCampaignDonationTimelineAsync(int campaignId)
            => _repo.GetCampaignDonationTimelineAsync(campaignId);

        // ===== User Engagement =====

        public async Task<IEnumerable<DonationResponseDto>> GetRecurringDonorsAsync(int minDonations = 3)
            => _mapper.MapToDonationResponseDtos(await _repo.GetRecurringDonorsAsync(minDonations));

        public async Task<IEnumerable<DonationResponseDto>> GetFirstTimeDonorsAsync(DateTime startDate, DateTime endDate)
            => _mapper.MapToDonationResponseDtos(await _repo.GetFirstTimeDonorsAsync(startDate, endDate));

        public Task<Dictionary<string, double>> GetUserLifetimeValueAsync()
            => _repo.GetUserLifetimeValueAsync();

        public Task<IEnumerable<string>> GetLoyalDonorsAsync(double minTotalAmount = 1000, int minDonations = 5)
            => _repo.GetLoyalDonorsAsync(minTotalAmount, minDonations);

        // ===== Search & Filter Combinations =====

        public async Task<IEnumerable<DonationResponseDto>> SearchDonationsByUserAndCampaignAsync(string userId, int campaignId)
            => _mapper.MapToDonationResponseDtos(await _repo.SearchDonationsByUserAndCampaignAsync(userId, campaignId));

        public async Task<IEnumerable<DonationResponseDto>> GetDonationsByMultipleUsersAsync(IEnumerable<string> userIds)
            => _mapper.MapToDonationResponseDtos(await _repo.GetDonationsByMultipleUsersAsync(userIds));

        public async Task<IEnumerable<DonationResponseDto>> GetDonationsByMultipleCampaignsAsync(IEnumerable<int> campaignIds)
            => _mapper.MapToDonationResponseDtos(await _repo.GetDonationsByMultipleCampaignsAsync(campaignIds));

        public async Task<IEnumerable<DonationResponseDto>> GetDonationsByAmountAndDateAsync(double minAmount, DateTime startDate)
            => _mapper.MapToDonationResponseDtos(await _repo.GetDonationsByAmountAndDateAsync(minAmount, startDate));

        // ===== Audit =====

        public async Task<IEnumerable<DonationResponseDto>> GetSuspiciousDonationsAsync(double amountThreshold = 10000)
            => _mapper.MapToDonationResponseDtos(await _repo.GetSuspiciousDonationsAsync(amountThreshold));

        // ===== Export =====

        public Task<int> GetDonationRecordCountForPeriodAsync(DateTime startDate, DateTime endDate)
            => _repo.GetDonationRecordCountForPeriodAsync(startDate, endDate);
    }
}
