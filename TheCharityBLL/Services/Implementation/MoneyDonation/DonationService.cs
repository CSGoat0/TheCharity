
using TheCharityBLL.DTOs;

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

        public async Task<ServiceResponse<IEnumerable<DonationResponseDto>>> GetAllDonationsAsync(bool includeDeleted = false)
        {
            var donations = await _repo.GetAllDonationsAsync(includeDeleted);
            var DonationsDto = _mapper.MapToDonationResponseDtos(donations);
            return new ServiceResponse<IEnumerable<DonationResponseDto>>
            {
                Success = true
                ,
                Message = "",
                Data = DonationsDto
            };
        }

        public async Task<ServiceResponse<DonationResponseDto?>> GetDonationByIdAsync(int id)
        {
            var donation = await _repo.GetDonationByIdAsync(id);
            return donation is null ? null : new ServiceResponse<DonationResponseDto?> { Data = _mapper.MapToDonationResponseDto(donation), Success = true, Message = "retrieved donation info by id successfully." };
        }

        public async Task<ServiceResponse<DonationResponseDto>> CreateDonationAsync(CreateDonationDto dto)
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
            return new ServiceResponse<DonationResponseDto> { Success = true, Message = "added donation info successfully.", Data = _mapper.MapToDonationResponseDto(created) };
        }

        public async Task<ServiceResponse<DonationResponseDto?>> UpdateDonationAsync(int id, UpdateDonationDto dto)
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
            return new ServiceResponse<DonationResponseDto?> { Data = _mapper.MapToDonationResponseDto(updated), Success = true, Message = "Updated donation data is done Successfully." };
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

        public async Task<ServiceResponse<IEnumerable<DonationResponseDto>>> GetDonationsByUserAsync(string userId)
            => new ServiceResponse<IEnumerable<DonationResponseDto>> { Data = _mapper.MapToDonationResponseDtos(await _repo.GetDonationsByUserAsync(userId)), Success = true, Message = $"retrieved donations by id:{userId} successfully" };

        public async Task<ServiceResponse<IEnumerable<DonationResponseDto>>> GetDonationsByCampaignAsync(int campaignId)
            => new ServiceResponse<IEnumerable<DonationResponseDto>> { Data = _mapper.MapToDonationResponseDtos(await _repo.GetDonationsByCampaignAsync(campaignId)), Success = true, Message = $"retrieved donations by id:{campaignId} successfully" };

        public async Task<ServiceResponse<IEnumerable<DonationResponseDto>>> GetDonationsByAmountRangeAsync(double minAmount, double maxAmount)
            => new ServiceResponse<IEnumerable<DonationResponseDto>> { Data = _mapper.MapToDonationResponseDtos(await _repo.GetDonationsByAmountRangeAsync(minAmount, maxAmount)), Success = true, Message = "retrieved donations by amount range successfully." };

        public async Task<ServiceResponse<IEnumerable<DonationResponseDto>>> GetDonationsByDateRangeAsync(DateTime startDate, DateTime endDate)
            => new ServiceResponse<IEnumerable<DonationResponseDto>> { Data = _mapper.MapToDonationResponseDtos(await _repo.GetDonationsByDateRangeAsync(startDate, endDate)), Success = true, Message = "retrieved donations by date range successfully." };

        public async Task<ServiceResponse<IEnumerable<DonationResponseDto>>> GetRecentDonationsAsync(int days = 30)
            => new ServiceResponse<IEnumerable<DonationResponseDto>> { Data = _mapper.MapToDonationResponseDtos(await _repo.GetRecentDonationsAsync(days)), Success = true, Message = "retrieved recent donations successfully." };

        public async Task<ServiceResponse<IEnumerable<DonationResponseDto>>> GetDeletedDonationsAsync()
            => new ServiceResponse<IEnumerable<DonationResponseDto>> { Data = _mapper.MapToDonationResponseDtos(await _repo.GetDeletedDonationsAsync()), Success = true, Message = "retrieved deleted donations successfully" };

        // ===== Statistics =====

        public async Task<ServiceResponse<double>> GetTotalDonationsAmountAsync()
            => new ServiceResponse<double> { Data = await _repo.GetTotalDonationsAmountAsync(), Success = true, Message = "retrieved total donations amount successfully" };

        public async Task<ServiceResponse<double>> GetTotalDonationsAmountByUserAsync(string userId)
            => new ServiceResponse<double> { Data = await _repo.GetTotalDonationsAmountByUserAsync(userId), Success = true, Message = "retrieved deleted donations successfully" };

        public async Task<ServiceResponse<double>> GetTotalDonationsAmountByCampaignAsync(int campaignId)
            => new ServiceResponse<double> { Data = await _repo.GetTotalDonationsAmountByCampaignAsync(campaignId), Success = true, Message = "retrieved total donations amount by campaign id successfully" };

        public async Task<ServiceResponse<int>> GetTotalDonationsCountAsync()
            => new ServiceResponse<int> { Data = await _repo.GetTotalDonationsCountAsync(), Success = true, Message = "retrieved count for total donations successfully" };

        public async Task<ServiceResponse<int>> GetDonationsCountByUserAsync(string userId)
            => new ServiceResponse<int> { Data = await _repo.GetDonationsCountByUserAsync(userId), Success = true, Message = "retrieved count for total donations by user id successfully" };

        public async Task<ServiceResponse<int>> GetDonationsCountByCampaignAsync(int campaignId)
            => new ServiceResponse<int> { Data = await _repo.GetDonationsCountByCampaignAsync(campaignId), Success = true, Message = "retrieved count for total donations by campaign id successfully" };

        // ===== Advanced Analytics =====

        public async Task<ServiceResponse<double>> GetAverageDonationAmountAsync()
            => new ServiceResponse<double> { Success = true, Message = "retrieved average for donations balance", Data = await _repo.GetAverageDonationAmountAsync() };

        public async Task<ServiceResponse<double>> GetAverageDonationAmountByUserAsync(string userId)
            => new ServiceResponse<double> { Data = await _repo.GetAverageDonationAmountByUserAsync(userId), Success = true, Message = "retrieved average for donations balance for specific user successfully" };

        public async Task<ServiceResponse<double>> GetAverageDonationAmountByCampaignAsync(int campaignId)
            => new ServiceResponse<double> { Data = await _repo.GetAverageDonationAmountByCampaignAsync(campaignId), Success = true, Message = "retrieved average for donations balance for specific campaign successfully" };

        public async Task<ServiceResponse<Dictionary<string, double>>> GetTopDonorsByAmountAsync(int limit = 10)
            => new ServiceResponse<Dictionary<string, double>> { Data = await _repo.GetTopDonorsByAmountAsync(limit), Success = true, Message = "retrieved top donors successfully" };

        public async Task<ServiceResponse<Dictionary<int, double>>> GetTopCampaignsByDonationsAsync(int limit = 10)
            => new ServiceResponse<Dictionary<int, double>> { Data = await _repo.GetTopCampaignsByDonationsAsync(limit), Success = true, Message = "retrieved top campaign in collecting the donations successfully" };

        public async Task<ServiceResponse<Dictionary<DateTime, double>>> GetDonationsTrendAsync(int days = 30)
            => new ServiceResponse<Dictionary<DateTime, double>> { Data = await _repo.GetDonationsTrendAsync(days), Success = true, Message = "retrieved trend donations successfully" };

        public async Task<ServiceResponse<Dictionary<string, int>>> GetDonationFrequencyByUserAsync()
            => new ServiceResponse<Dictionary<string, int>> { Data = await _repo.GetDonationFrequencyByUserAsync(), Success = true, Message = "retrieved donations frequency successfully" };

        // ===== Campaign-Specific =====

        public async Task<ServiceResponse<double>> GetCampaignTotalRaisedAsync(int campaignId)
            => new ServiceResponse<double> { Data = await _repo.GetCampaignTotalRaisedAsync(campaignId), Success = true, Message = "retrieved total raised for specific campaign successfully." };

        public async Task<ServiceResponse<double>> GetCampaignProgressPercentageAsync(int campaignId)
            => new ServiceResponse<double> { Data = await _repo.GetCampaignProgressPercentageAsync(campaignId), Success = true, Message = "retrieved progress percentage for specific campaign successfully." };

        public async Task<ServiceResponse<IEnumerable<DonationResponseDto>>> GetUsersDonationsOfACampaignAsync(int campaignId)
            => new ServiceResponse<IEnumerable<DonationResponseDto>> { Data = _mapper.MapToDonationResponseDtos(await _repo.GetUsersDonationsOfACampaignAsync(campaignId)), Success = true, Message = "retrieved users , where they donated for specific campaign successfully" };

        // ===== User-Specific =====

        public async Task<ServiceResponse<IEnumerable<DonationResponseDto>>> GetUserDonationHistoryAsync(string userId)
            => new ServiceResponse<IEnumerable<DonationResponseDto>> { Data = _mapper.MapToDonationResponseDtos(await _repo.GetUserDonationHistoryAsync(userId)), Success = true, Message = "retrieved donation history for specific user successfully" };

        public async Task<ServiceResponse<DateTime?>> GetUserLastDonationDateAsync(string userId)
            => new ServiceResponse<DateTime?> { Data = await _repo.GetUserLastDonationDateAsync(userId), Success = true, Message = "retrieved last donation for specific user successfully." };

        public async Task<ServiceResponse<IEnumerable<int>>> GetCampaignsDonatedByUserAsync(string userId)
            => new ServiceResponse<IEnumerable<int>> { Data = await _repo.GetCampaignsDonatedByUserAsync(userId), Success = true, Message = "retrieved list of campaigns where specific user donated for them successfully" };

        // ===== Bulk Operations =====

        public async Task<ServiceResponse<int>> TransferDonationsToCampaignAsync(int fromCampaignId, int toCampaignId)
            => new ServiceResponse<int> { Data = await _repo.TransferDonationsToCampaignAsync(fromCampaignId, toCampaignId), Success = true, Message = "transfer the donations from campaign to another successfuly." };

        public async Task<ServiceResponse<int>> DeleteOldDonationsAsync(int daysOld = 365)
            => new ServiceResponse<int> { Data = await _repo.DeleteOldDonationsAsync(daysOld), Success = true, Message = "deleted old donations successfully." };

        // ===== Validation =====

        public async Task<ServiceResponse<bool>> DonationExistsAsync(int id)
            => new ServiceResponse<bool> { Data = await _repo.DonationExistsAsync(id), Success = true, Message = "scanned if the donation exists or not successfully" };

        public async Task<ServiceResponse<bool>> HasUserDonatedToCampaignAsync(string userId, int campaignId)
            => new ServiceResponse<bool> { Data = await _repo.HasUserDonatedToCampaignAsync(userId, campaignId), Success = true, Message = "scanned if the user donated for specific campaign successfully." };

        // ===== Eager Loading =====

        public async Task<ServiceResponse<DonationResponseDto?>> GetDonationWithDetailsAsync(int id)
        {
            var donation = await _repo.GetDonationWithDetailsAsync(id);
            var result = donation is null ? null : _mapper.MapToDonationResponseDto(donation);
            return new ServiceResponse<DonationResponseDto?> { Data = result, Success = true, Message = "retrieved the details for specific donation successfully" };
        }

        // ===== Dashboard & Reporting =====

        public async Task<ServiceResponse<IEnumerable<DonationResponseDto>>> GetLatestDonationsAsync(int limit = 10)
            => new ServiceResponse<IEnumerable<DonationResponseDto>> { Data = _mapper.MapToDonationResponseDtos(await _repo.GetLatestDonationsAsync(limit)), Success = true, Message = "retrieved latest donations successfully." };

        public async Task<ServiceResponse<IEnumerable<DonationResponseDto>>> GetLargestDonationsAsync(int limit = 10)
            =>new ServiceResponse<IEnumerable<DonationResponseDto>> { Data = _mapper.MapToDonationResponseDtos(await _repo.GetLargestDonationsAsync(limit)), Success = true, Message = "retrieved largest donations successfully" };

        public async Task<ServiceResponse<Dictionary<int, int>>> GetDonationsPerCampaignCountAsync()
            =>new ServiceResponse<Dictionary<int, int>> { Data = await _repo.GetDonationsPerCampaignCountAsync(), Success = true, Message = "retrieved count of the donations per campaign successfully." };

        public async Task<ServiceResponse<Dictionary<string, int>>> GetDonationsPerUserCountAsync()
            =>new ServiceResponse<Dictionary<string, int>> { Data = await _repo.GetDonationsPerUserCountAsync(), Success = true, Message = "retrieved count of the donations per user successfully." };

        public async Task<ServiceResponse<double>> GetTodayDonationsTotalAsync()
            => new ServiceResponse<double> { Data = await _repo.GetTodayDonationsTotalAsync(), Success = true, Message = "retrieved count for this day donations successfully." };

        public async Task<ServiceResponse<double>> GetThisWeekDonationsTotalAsync()
            =>new ServiceResponse<double> { Data = await _repo.GetThisWeekDonationsTotalAsync(), Success = true, Message = "retrieved count for this week donations successfully." };

        public async Task<ServiceResponse<double>> GetThisMonthDonationsTotalAsync()
            =>new ServiceResponse<double> { Data = await _repo.GetThisMonthDonationsTotalAsync(), Success = true, Message = "retrieved count for this month donations successfully." };

        // ===== Financial Reporting =====

        public async Task<ServiceResponse<Dictionary<string, double>>> GetMonthlyDonationsReportAsync(int year)
            =>new ServiceResponse<Dictionary<string, double>> { Data = await _repo.GetMonthlyDonationsReportAsync(year), Success = true, Message = "retrieved Monthly Donations Report successfully." };

        public async Task<ServiceResponse<Dictionary<string, double>>> GetQuarterlyDonationsReportAsync(int year)
            =>new ServiceResponse<Dictionary<string, double>> { Data = await _repo.GetQuarterlyDonationsReportAsync(year), Success = true, Message = "retrieved Quarterly Donations Report successfully." };

        public async Task<ServiceResponse<Dictionary<string, double>>> GetYearlyDonationsReportAsync(int yearsBack = 5)
            =>new ServiceResponse<Dictionary<string, double>> { Data = await _repo.GetYearlyDonationsReportAsync(yearsBack), Success = true, Message = "retrieved yearly donations report successfully." };

        public async Task<ServiceResponse<Dictionary<string, double>>> GetDonationsByTimeOfDayAsync()
            =>new ServiceResponse<Dictionary<string, double>> { Data = await _repo.GetDonationsByTimeOfDayAsync(), Success = true, Message = "retrieved the donations by time of a day successfully." };

        public async Task<ServiceResponse<Dictionary<string, double>>> GetDonationsByDayOfWeekAsync()
            =>new ServiceResponse<Dictionary<string, double>> { Data = await _repo.GetDonationsByDayOfWeekAsync(), Success = true, Message = "retrieved the donations by day of a week successfully." };

        // ===== Campaign Performance =====

        public async Task<ServiceResponse<Dictionary<DateTime, double>>> GetCampaignDonationTimelineAsync(int campaignId)
            =>new ServiceResponse<Dictionary<DateTime, double>> { Data = await _repo.GetCampaignDonationTimelineAsync(campaignId), Success = true, Message = "retrieved the donation time line for specific campaign successfully." };

        // ===== User Engagement =====

        public async Task<ServiceResponse<IEnumerable<DonationResponseDto>>> GetRecurringDonorsAsync(int minDonations = 3)
            =>new ServiceResponse<IEnumerable<DonationResponseDto>> { Data = _mapper.MapToDonationResponseDtos(await _repo.GetRecurringDonorsAsync(minDonations)), Success = true, Message = "retrieved Recurring Donors successfully." };

        public async Task<ServiceResponse<IEnumerable<DonationResponseDto>>> GetFirstTimeDonorsAsync(DateTime startDate, DateTime endDate)
            =>new ServiceResponse<IEnumerable<DonationResponseDto>> { Data = _mapper.MapToDonationResponseDtos(await _repo.GetFirstTimeDonorsAsync(startDate, endDate)), Success = true, Message = "retrieved first time donors successfully." };

        public async Task<ServiceResponse<Dictionary<string, double>>> GetUserLifetimeValueAsync()
            =>new ServiceResponse<Dictionary<string, double>> { Data = await _repo.GetUserLifetimeValueAsync(), Success = true, Message = "retrieved list of users assigned with life time value successfully." };

        public async Task<ServiceResponse<IEnumerable<string>>> GetLoyalDonorsAsync(double minTotalAmount = 1000, int minDonations = 5)
            =>new ServiceResponse<IEnumerable<string>> { Data = await _repo.GetLoyalDonorsAsync(minTotalAmount, minDonations), Success = true, Message = "retrieved the loyal donors successfully." };

        // ===== Search & Filter Combinations =====

        public async Task<ServiceResponse<IEnumerable<DonationResponseDto>>> SearchDonationsByUserAndCampaignAsync(string userId, int campaignId)
            =>new ServiceResponse<IEnumerable<DonationResponseDto>> { Data = _mapper.MapToDonationResponseDtos(await _repo.SearchDonationsByUserAndCampaignAsync(userId, campaignId)), Success = true, Message = "retrieved the donations for specific user and campaign successfuly." };

        public async Task<ServiceResponse<IEnumerable<DonationResponseDto>>> GetDonationsByMultipleUsersAsync(IEnumerable<string> userIds)
            =>new ServiceResponse<IEnumerable<DonationResponseDto>> { Data = _mapper.MapToDonationResponseDtos(await _repo.GetDonationsByMultipleUsersAsync(userIds)), Success = true, Message = "retrieved the Donations by multiple users successfully." };

        public async Task<ServiceResponse<IEnumerable<DonationResponseDto>>> GetDonationsByMultipleCampaignsAsync(IEnumerable<int> campaignIds)
            => new ServiceResponse<IEnumerable<DonationResponseDto>> { Data = _mapper.MapToDonationResponseDtos(await _repo.GetDonationsByMultipleCampaignsAsync(campaignIds)), Success = true, Message = "retrieved the Donations by multiple campaigns successfully." };

        public async Task<ServiceResponse<IEnumerable<DonationResponseDto>>> GetDonationsByAmountAndDateAsync(double minAmount, DateTime startDate)
            => new ServiceResponse<IEnumerable<DonationResponseDto>> { Data = _mapper.MapToDonationResponseDtos(await _repo.GetDonationsByAmountAndDateAsync(minAmount, startDate)), Success = true, Message = "retrieved the donations in specific period limited by specific balance successfully." };

        // ===== Audit =====

        public async Task<ServiceResponse<IEnumerable<DonationResponseDto>>> GetSuspiciousDonationsAsync(double amountThreshold = 10000)
            =>new ServiceResponse<IEnumerable<DonationResponseDto>> { Data = _mapper.MapToDonationResponseDtos(await _repo.GetSuspiciousDonationsAsync(amountThreshold)), Success = true, Message = "retrieved Suspicious Donations successfully." };

        // ===== Export =====

        public async Task<ServiceResponse<int>> GetDonationRecordCountForPeriodAsync(DateTime startDate, DateTime endDate)
            => new ServiceResponse<int> { Data = await _repo.GetDonationRecordCountForPeriodAsync(startDate, endDate), Success = true, Message = "retrieved count for the donations in specific periods successfully." };



    }
}

 
      

    
