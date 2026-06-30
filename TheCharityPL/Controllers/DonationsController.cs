using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TheCharityBLL.DTOs.DonationDTOs;
using TheCharityBLL.DTOs.PaginationDTOs;
using TheCharityBLL.Services.Abstraction.MoneyDonation;

namespace TheCharityPL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonationsController : ControllerBase
    {
        private readonly IDonationService _service;

        public DonationsController(IDonationService service)
        {
            _service = service;
        }

        // =====================================================================
        // CRUD
        // =====================================================================

        // GET api/donations
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParametersDto parametersDto, [FromQuery] bool includeDeleted = false)
        {
            var result = await _service.GetAllDonationsAsync(parametersDto, includeDeleted);
            return Ok(result);
        }

        // GET api/donations/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetDonationByIdAsync(id);
            return result is null ? NotFound() : Ok(result);
        }

        // GET api/donations/5/details
        [HttpGet("{id:int}/details")]
        public async Task<IActionResult> GetWithDetails(int id)
        {
            var result = await _service.GetDonationWithDetailsAsync(id);
            return result is null ? NotFound() : Ok(result);
        }

        // POST api/donations
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDonationDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var created = await _service.CreateDonationAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT api/donations/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateDonationDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var updated = await _service.UpdateDonationAsync(id, dto);
            return updated is null ? NotFound() : Ok(updated);
        }

        // DELETE api/donations/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteDonationAsync(id);
            return success ? NoContent() : NotFound();
        }

        // PATCH api/donations/5/restore
        [HttpPatch("{id:int}/restore")]
        public async Task<IActionResult> Restore(int id)
        {
            var success = await _service.RestoreDonationAsync(id);
            return success ? NoContent() : NotFound();
        }

        // =====================================================================
        // Filtering & Search
        // =====================================================================

        // GET api/donations/deleted
        [HttpGet("deleted")]
        public async Task<IActionResult> GetDeleted([FromQuery] PaginationParametersDto parametersDto)
            => Ok(await _service.GetDeletedDonationsAsync(parametersDto));

        // GET api/donations/recent?days=30
        [HttpGet("recent")]
        public async Task<IActionResult> GetRecent([FromQuery] PaginationParametersDto parametersDto, [FromQuery] int days = 30)
            => Ok(await _service.GetRecentDonationsAsync(parametersDto,days));

        // GET api/donations/by-user/userId123
        [HttpGet("by-user/{userId}")]
        public async Task<IActionResult> GetByUser([FromQuery] PaginationParametersDto parametersDto, string userId)
            => Ok(await _service.GetDonationsByUserAsync(parametersDto,userId));

        // GET api/donations/by-campaign/3
        [HttpGet("by-campaign/{campaignId:int}")]
        public async Task<IActionResult> GetByCampaign([FromQuery] PaginationParametersDto parametersDto, int campaignId)
            => Ok(await _service.GetDonationsByCampaignAsync(parametersDto,campaignId));

        // GET api/donations/by-amount-range?min=100&max=5000
        [HttpGet("by-amount-range")]
        public async Task<IActionResult> GetByAmountRange([FromQuery] PaginationParametersDto parametersDto,
            [FromQuery] double min, [FromQuery] double max)
            => Ok(await _service.GetDonationsByAmountRangeAsync(parametersDto,min, max));

        // GET api/donations/by-date-range?startDate=2024-01-01&endDate=2024-12-31
        [HttpGet("by-date-range")]
        public async Task<IActionResult> GetByDateRange([FromQuery] PaginationParametersDto parametersDto,
            [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
            => Ok(await _service.GetDonationsByDateRangeAsync(parametersDto,startDate, endDate));

        // GET api/donations/search?userId=xxx&campaignId=3
        [HttpGet("search")]
        public async Task<IActionResult> SearchByUserAndCampaign([FromQuery] PaginationParametersDto parametersDto,
            [FromQuery] string userId, [FromQuery] int campaignId)
            => Ok(await _service.SearchDonationsByUserAndCampaignAsync(parametersDto, userId, campaignId));

        // GET api/donations/by-amount-and-date?minAmount=100&startDate=2024-01-01
        [HttpGet("by-amount-and-date")]
        public async Task<IActionResult> GetByAmountAndDate([FromQuery] PaginationParametersDto parametersDto,
            [FromQuery] double minAmount, [FromQuery] DateTime startDate)
            => Ok(await _service.GetDonationsByAmountAndDateAsync(parametersDto,minAmount, startDate));

        // POST api/donations/by-users
        [HttpPost("by-users")]
        public async Task<IActionResult> GetByMultipleUsers([FromQuery] PaginationParametersDto parametersDto, [FromBody] IEnumerable<string> userIds)
            => Ok(await _service.GetDonationsByMultipleUsersAsync(parametersDto,userIds));

        // POST api/donations/by-campaigns
        [HttpPost("by-campaigns")]
        public async Task<IActionResult> GetByMultipleCampaigns([FromQuery] PaginationParametersDto parametersDto, [FromBody] IEnumerable<int> campaignIds)
            => Ok(await _service.GetDonationsByMultipleCampaignsAsync(parametersDto,campaignIds));

        // =====================================================================
        // Statistics
        // =====================================================================

        // GET api/donations/stats/total-amount
        [HttpGet("stats/total-amount")]
        public async Task<IActionResult> GetTotalAmount()
            => Ok(await _service.GetTotalDonationsAmountAsync());

        // GET api/donations/stats/total-count
        [HttpGet("stats/total-count")]
        public async Task<IActionResult> GetTotalCount()
            => Ok(await _service.GetTotalDonationsCountAsync());

        // GET api/donations/stats/total-amount/by-user/userId123
        [HttpGet("stats/total-amount/by-user/{userId}")]
        public async Task<IActionResult> GetTotalAmountByUser(string userId)
            => Ok(await _service.GetTotalDonationsAmountByUserAsync(userId));

        // GET api/donations/stats/total-amount/by-campaign/3
        [HttpGet("stats/total-amount/by-campaign/{campaignId:int}")]
        public async Task<IActionResult> GetTotalAmountByCampaign(int campaignId)
            => Ok(await _service.GetTotalDonationsAmountByCampaignAsync(campaignId));

        // GET api/donations/stats/count/by-user/userId123
        [HttpGet("stats/count/by-user/{userId}")]
        public async Task<IActionResult> GetCountByUser(string userId)
            => Ok(await _service.GetDonationsCountByUserAsync(userId));

        // GET api/donations/stats/count/by-campaign/3
        [HttpGet("stats/count/by-campaign/{campaignId:int}")]
        public async Task<IActionResult> GetCountByCampaign(int campaignId)
            => Ok(await _service.GetDonationsCountByCampaignAsync(campaignId));

        // =====================================================================
        // Advanced Analytics
        // =====================================================================

        // GET api/donations/analytics/average
        [HttpGet("analytics/average")]
        public async Task<IActionResult> GetAverage()
            => Ok(await _service.GetAverageDonationAmountAsync());

        // GET api/donations/analytics/average/by-user/userId123
        [HttpGet("analytics/average/by-user/{userId}")]
        public async Task<IActionResult> GetAverageByUser(string userId)
            => Ok(await _service.GetAverageDonationAmountByUserAsync(userId));

        // GET api/donations/analytics/average/by-campaign/3
        [HttpGet("analytics/average/by-campaign/{campaignId:int}")]
        public async Task<IActionResult> GetAverageByCampaign(int campaignId)
            => Ok(await _service.GetAverageDonationAmountByCampaignAsync(campaignId));

        // GET api/donations/analytics/top-donors?limit=10
        [HttpGet("analytics/top-donors")]
        public async Task<IActionResult> GetTopDonors([FromQuery] int limit = 10)
            => Ok(await _service.GetTopDonorsByAmountAsync(limit));

        // GET api/donations/analytics/top-campaigns?limit=10
        [HttpGet("analytics/top-campaigns")]
        public async Task<IActionResult> GetTopCampaigns([FromQuery] int limit = 10)
            => Ok(await _service.GetTopCampaignsByDonationsAsync(limit));

        // GET api/donations/analytics/trend?days=30
        [HttpGet("analytics/trend")]
        public async Task<IActionResult> GetTrend([FromQuery] int days = 30)
            => Ok(await _service.GetDonationsTrendAsync(days));

        // GET api/donations/analytics/frequency-by-user
        [HttpGet("analytics/frequency-by-user")]
        public async Task<IActionResult> GetFrequencyByUser()
            => Ok(await _service.GetDonationFrequencyByUserAsync());

        // =====================================================================
        // Dashboard & Reporting
        // =====================================================================

        // GET api/donations/dashboard/latest?limit=10
        [HttpGet("dashboard/latest")]
        public async Task<IActionResult> GetLatest([FromQuery] PaginationParametersDto parametersDto, [FromQuery] int limit = 10)
            => Ok(await _service.GetLatestDonationsAsync(parametersDto,limit));

        // GET api/donations/dashboard/largest?limit=10
        [HttpGet("dashboard/largest")]
        public async Task<IActionResult> GetLargest([FromQuery] PaginationParametersDto parametersDto, [FromQuery] int limit = 10)
            => Ok(await _service.GetLargestDonationsAsync(parametersDto,limit));

        // GET api/donations/dashboard/per-campaign-count
        [HttpGet("dashboard/per-campaign-count")]
        public async Task<IActionResult> GetPerCampaignCount()
            => Ok(await _service.GetDonationsPerCampaignCountAsync());

        // GET api/donations/dashboard/per-user-count
        [HttpGet("dashboard/per-user-count")]
        public async Task<IActionResult> GetPerUserCount()
            => Ok(await _service.GetDonationsPerUserCountAsync());

        // GET api/donations/dashboard/today-total
        [HttpGet("dashboard/today-total")]
        public async Task<IActionResult> GetTodayTotal()
            => Ok(await _service.GetTodayDonationsTotalAsync());

        // GET api/donations/dashboard/week-total
        [HttpGet("dashboard/week-total")]
        public async Task<IActionResult> GetWeekTotal()
            => Ok(await _service.GetThisWeekDonationsTotalAsync());

        // GET api/donations/dashboard/month-total
        [HttpGet("dashboard/month-total")]
        public async Task<IActionResult> GetMonthTotal()
            => Ok(await _service.GetThisMonthDonationsTotalAsync());

        // =====================================================================
        // Financial Reporting
        // =====================================================================

        // GET api/donations/reports/monthly?year=2024
        [HttpGet("reports/monthly")]
        public async Task<IActionResult> GetMonthlyReport([FromQuery] int year)
            => Ok(await _service.GetMonthlyDonationsReportAsync(year));

        // GET api/donations/reports/quarterly?year=2024
        [HttpGet("reports/quarterly")]
        public async Task<IActionResult> GetQuarterlyReport([FromQuery] int year)
            => Ok(await _service.GetQuarterlyDonationsReportAsync(year));

        // GET api/donations/reports/yearly?yearsBack=5
        [HttpGet("reports/yearly")]
        public async Task<IActionResult> GetYearlyReport([FromQuery] int yearsBack = 5)
            => Ok(await _service.GetYearlyDonationsReportAsync(yearsBack));

        // GET api/donations/reports/by-time-of-day
        [HttpGet("reports/by-time-of-day")]
        public async Task<IActionResult> GetByTimeOfDay()
            => Ok(await _service.GetDonationsByTimeOfDayAsync());

        // GET api/donations/reports/by-day-of-week
        [HttpGet("reports/by-day-of-week")]
        public async Task<IActionResult> GetByDayOfWeek()
            => Ok(await _service.GetDonationsByDayOfWeekAsync());

        // GET api/donations/reports/record-count?startDate=2024-01-01&endDate=2024-12-31
        [HttpGet("reports/record-count")]
        public async Task<IActionResult> GetRecordCount(
            [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
            => Ok(await _service.GetDonationRecordCountForPeriodAsync(startDate, endDate));

        // =====================================================================
        // Campaign-Specific
        // =====================================================================

        // GET api/donations/campaigns/3/total-raised
        [HttpGet("campaigns/{campaignId:int}/total-raised")]
        public async Task<IActionResult> GetCampaignTotalRaised(int campaignId)
            => Ok(await _service.GetCampaignTotalRaisedAsync(campaignId));

        // GET api/donations/campaigns/3/progress
        [HttpGet("campaigns/{campaignId:int}/progress")]
        public async Task<IActionResult> GetCampaignProgress(int campaignId)
            => Ok(await _service.GetCampaignProgressPercentageAsync(campaignId));

        // GET api/donations/campaigns/3/donors
        [HttpGet("campaigns/{campaignId:int}/donors")]
        public async Task<IActionResult> GetCampaignDonors([FromQuery] PaginationParametersDto parametersDto, int campaignId)
            => Ok(await _service.GetUsersDonationsOfACampaignAsync(parametersDto, campaignId));

        // GET api/donations/campaigns/3/timeline
        [HttpGet("campaigns/{campaignId:int}/timeline")]
        public async Task<IActionResult> GetCampaignTimeline(int campaignId)
            => Ok(await _service.GetCampaignDonationTimelineAsync(campaignId));

        // =====================================================================
        // User-Specific
        // =====================================================================

        // GET api/donations/users/userId123/history
        [HttpGet("users/{userId}/history")]
        public async Task<IActionResult> GetUserHistory([FromQuery] PaginationParametersDto parametersDto, string userId)
            => Ok(await _service.GetUserDonationHistoryAsync(parametersDto,userId));

        // GET api/donations/users/userId123/last-donation-date
        [HttpGet("users/{userId}/last-donation-date")]
        public async Task<IActionResult> GetUserLastDonationDate(string userId)
        {
            var date = await _service.GetUserLastDonationDateAsync(userId);
            return date is null ? NotFound() : Ok(date);
        }

        // GET api/donations/users/userId123/campaigns
        [HttpGet("users/{userId}/campaigns")]
        public async Task<IActionResult> GetCampaignsDonatedByUser(string userId)
            => Ok(await _service.GetCampaignsDonatedByUserAsync(userId));

        // =====================================================================
        // Bulk Operations
        // =====================================================================

        // POST api/donations/bulk/transfer?from=1&to=2
        [HttpPost("bulk/transfer")]
        public async Task<IActionResult> TransferDonations(
            [FromQuery] int from, [FromQuery] int to)
        {
            var count = await _service.TransferDonationsToCampaignAsync(from, to);
            return Ok(new { TransferredCount = count });
        }

        // DELETE api/donations/bulk/old?daysOld=365
        [HttpDelete("bulk/old")]
        public async Task<IActionResult> DeleteOldDonations([FromQuery] int daysOld = 365)
        {
            var count = await _service.DeleteOldDonationsAsync(daysOld);
            return Ok(new { DeletedCount = count });
        }

        // =====================================================================
        // Validation & Checks
        // =====================================================================

        // GET api/donations/5/exists
        [HttpGet("{id:int}/exists")]
        public async Task<IActionResult> Exists(int id)
            => Ok(await _service.DonationExistsAsync(id));

        // GET api/donations/check-donated?userId=xxx&campaignId=3
        [HttpGet("check-donated")]
        public async Task<IActionResult> HasUserDonated(
            [FromQuery] string userId, [FromQuery] int campaignId)
            => Ok(await _service.HasUserDonatedToCampaignAsync(userId, campaignId));

        // =====================================================================
        // User Engagement
        // =====================================================================

        // GET api/donations/engagement/recurring?minDonations=3
        [HttpGet("engagement/recurring")]
        public async Task<IActionResult> GetRecurringDonors([FromQuery] PaginationParametersDto parametersDto, [FromQuery] int minDonations = 3)
            => Ok(await _service.GetRecurringDonorsAsync(parametersDto, minDonations));

        // GET api/donations/engagement/first-time?startDate=2024-01-01&endDate=2024-12-31
        [HttpGet("engagement/first-time")]
        public async Task<IActionResult> GetFirstTimeDonors([FromQuery] PaginationParametersDto parametersDto,
            [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
            => Ok(await _service.GetFirstTimeDonorsAsync(parametersDto,startDate, endDate));

        // GET api/donations/engagement/lifetime-value
        [HttpGet("engagement/lifetime-value")]
        public async Task<IActionResult> GetUserLifetimeValue()
            => Ok(await _service.GetUserLifetimeValueAsync());

        // GET api/donations/engagement/loyal?minAmount=1000&minDonations=5
        [HttpGet("engagement/loyal")]
        public async Task<IActionResult> GetLoyalDonors(
            [FromQuery] double minTotalAmount = 1000, [FromQuery] int minDonations = 5)
            => Ok(await _service.GetLoyalDonorsAsync(minTotalAmount, minDonations));

        // =====================================================================
        // Audit
        // =====================================================================

        // GET api/donations/audit/suspicious?threshold=10000
        [HttpGet("audit/suspicious")]
        public async Task<IActionResult> GetSuspicious([FromQuery] PaginationParametersDto parametersDto, [FromQuery] double threshold = 10000)
            => Ok(await _service.GetSuspiciousDonationsAsync(parametersDto, threshold));
    }
}
