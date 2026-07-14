
using Microsoft.AspNetCore.Mvc;
using TheCharityBLL.DTOs;
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
        /// <summary>
        /// get all donations
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParametersDto parametersDto, [FromQuery] bool includeDeleted = false)
        {
            var result = await _service.GetAllDonationsAsync(parametersDto, includeDeleted);
            return Ok(result);
        }

        // GET api/donations/5
        /// <summary>
        /// get specific donation included his user and campaign by donation id
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetDonationByIdAsync(id);
            return result is null ? NotFound() : Ok(result);
        }
        /// <summary>
        /// get specific donation  included his user and campaign by donation id
        /// </summary>
        // GET api/donations/5/details
        [HttpGet("{id:int}/details")]
        public async Task<IActionResult> GetWithDetails(int id)
        {
            var result = await _service.GetDonationWithDetailsAsync(id);
            return result is null ? NotFound() : Ok(result);
        }
        /// <summary>
        /// create donation 
        /// </summary>
        // POST api/donations
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDonationDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var created = await _service.CreateDonationAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Data.Id }, created);
        }
        /// <summary>
        /// update specific donation 
        /// </summary>
        // PUT api/donations/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateDonationDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var updated = await _service.UpdateDonationAsync(id, dto);
            return updated is null ? NotFound(new ServiceResponse<object?>{Success=false,Message="invalid user id." }) : Ok(updated);
        }
        /// <summary>
        /// delete specific donation  
        /// </summary>
        // DELETE api/donations/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteDonationAsync(id);
            return success ? Ok(new ServiceResponse<object?> { Success=true,Message="Deleted Successfully."}) : NotFound(new ServiceResponse<object?> { Success = false, Message = "invalid user id." });
        }
        /// <summary>
        /// restore specific donation  after delete it  
        /// </summary>
        // PATCH api/donations/5/restore
        [HttpPatch("{id:int}/restore")]
        public async Task<IActionResult> Restore(int id)
        {
            var success = await _service.RestoreDonationAsync(id);
            return success ? Ok(new ServiceResponse<object?> { Success = true, Message = "Restored Successfully." }) : NotFound(new ServiceResponse<object?> { Success = false, Message = "invalid user id." });
        }

        // =====================================================================
        // Filtering & Search
        // =====================================================================

        // GET api/donations/deleted
        /// <summary>
        /// display all deleted donations  
        /// </summary>
        [HttpGet("deleted")]
        public async Task<IActionResult> GetDeleted([FromQuery] PaginationParametersDto parametersDto)
            => Ok(await _service.GetDeletedDonationsAsync(parametersDto));

        // GET api/donations/recent?days=30
        /// <summary>
        /// get recent donations  based on num days , where if days=2,get recent transaction for last two days  (default 30 days)
        /// </summary>
        [HttpGet("recent")]
        public async Task<IActionResult> GetRecent([FromQuery] PaginationParametersDto parametersDto, [FromQuery] int days = 30)
            => Ok(await _service.GetRecentDonationsAsync(parametersDto, days));

        /// <summary>
        /// get donation related to user id 
        /// </summary>
        // GET api/donations/by-user/userId123
        [HttpGet("by-user/{userId}")]
        public async Task<IActionResult> GetByUser([FromQuery] PaginationParametersDto parametersDto, string userId)
            => Ok(await _service.GetDonationsByUserAsync(parametersDto, userId));
        /// <summary>
        /// get donation related to campaign id by the admin
        /// </summary>
        // GET api/donations/by-campaign/3
        [HttpGet("by-campaign/{campaignId:int}")]
        public async Task<IActionResult> GetByCampaign([FromQuery] PaginationParametersDto parametersDto, int campaignId)
            => Ok(await _service.GetDonationsByCampaignAsync(parametersDto, campaignId));
        /// <summary>
        /// get donations , where their amount in range ( min , max ) 
        /// </summary>
        // GET api/donations/by-amount-range?min=100&max=5000
        [HttpGet("by-amount-range")]
        public async Task<IActionResult> GetByAmountRange([FromQuery] PaginationParametersDto parametersDto,
            [FromQuery] double min, [FromQuery] double max)
            => Ok(await _service.GetDonationsByAmountRangeAsync(parametersDto, min, max));

        // GET api/donations/by-date-range?startDate=2024-01-01&endDate=2024-12-31
        [HttpGet("by-date-range")]
        public async Task<IActionResult> GetByDateRange([FromQuery] PaginationParametersDto parametersDto,
            [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
            => Ok(await _service.GetDonationsByDateRangeAsync(parametersDto, startDate, endDate));

        /// <summary>
        /// get list of donations related to both user id and campaign id 
        /// </summary>
        // GET api/donations/search?userId=xxx&campaignId=3
        [HttpGet("search")]
        public async Task<IActionResult> SearchByUserAndCampaign([FromQuery] PaginationParametersDto parametersDto,
            [FromQuery] string userId, [FromQuery] int campaignId)
            => Ok(await _service.SearchDonationsByUserAndCampaignAsync(parametersDto, userId, campaignId));

        /// <summary>
        /// get list of donations related to both min amount ( condition for minimum amount ) and start date   
        /// </summary>
        // GET api/donations/by-amount-and-date?minAmount=100&startDate=2024-01-01
        [HttpGet("by-amount-and-date")]
        public async Task<IActionResult> GetByAmountAndDate([FromQuery] PaginationParametersDto parametersDto,
            [FromQuery] double minAmount, [FromQuery] DateTime startDate)
            => Ok(await _service.GetDonationsByAmountAndDateAsync(parametersDto, minAmount, startDate));

        /// <summary>
        /// get list of donations related to list of user ids   
        /// </summary>
        // POST api/donations/by-users
        [HttpPost("by-users")]
        public async Task<IActionResult> GetByMultipleUsers([FromQuery] PaginationParametersDto parametersDto, [FromBody] IEnumerable<string> userIds)
            => Ok(await _service.GetDonationsByMultipleUsersAsync(parametersDto,userIds));

        /// <summary>
        /// get list of donations related to list of campaign ids   
        /// </summary>
        // POST api/donations/by-campaigns
        [HttpPost("by-campaigns")]
        public async Task<IActionResult> GetByMultipleCampaigns([FromQuery] PaginationParametersDto parametersDto, [FromBody] IEnumerable<int> campaignIds)
            => Ok(await _service.GetDonationsByMultipleCampaignsAsync(parametersDto, campaignIds));

        // =====================================================================
        // Statistics
        // =====================================================================
        /// <summary>
        /// get the balance of all donations 
        /// </summary>
        // GET api/donations/stats/total-amount
        [HttpGet("stats/total-amount")]
        public async Task<IActionResult> GetTotalAmount()
            => Ok(await _service.GetTotalDonationsAmountAsync());
        /// <summary>
        /// get number of all donations 
        /// </summary>
        // GET api/donations/stats/total-count
        [HttpGet("stats/total-count")]
        public async Task<IActionResult> GetTotalCount()
            => Ok(await _service.GetTotalDonationsCountAsync());
        /// <summary>
        /// get the balance of all donations related to specific user by user id
        /// </summary>
        // GET api/donations/stats/total-amount/by-user/userId123
        [HttpGet("stats/total-amount/by-user/{userId}")]
        public async Task<IActionResult> GetTotalAmountByUser(string userId)
            => Ok(await _service.GetTotalDonationsAmountByUserAsync(userId));
        /// <summary>
        /// get the balance of all donations related to specific campaign by campaign id
        /// </summary>
        // GET api/donations/stats/total-amount/by-campaign/3
        [HttpGet("stats/total-amount/by-campaign/{campaignId:int}")]
        public async Task<IActionResult> GetTotalAmountByCampaign(int campaignId)
            => Ok(await _service.GetTotalDonationsAmountByCampaignAsync(campaignId));
        /// <summary>
        /// get number of all donations related to specific user by user id
        /// </summary>
        // GET api/donations/stats/count/by-user/userId123
        [HttpGet("stats/count/by-user/{userId}")]
        public async Task<IActionResult> GetCountByUser(string userId)
            => Ok(await _service.GetDonationsCountByUserAsync(userId));
        /// <summary>
        /// get number of all donations related to specific campaign by campaign id
        /// </summary>
        // GET api/donations/stats/count/by-campaign/3
        [HttpGet("stats/count/by-campaign/{campaignId:int}")]
        public async Task<IActionResult> GetCountByCampaign(int campaignId)
            => Ok(await _service.GetDonationsCountByCampaignAsync(campaignId));

        // =====================================================================
        // Advanced Analytics
        // =====================================================================
        /// <summary>
        /// get the balance average of all donations 
        /// </summary>
        // GET api/donations/analytics/average
        [HttpGet("analytics/average")]
        public async Task<IActionResult> GetAverage()
            => Ok(await _service.GetAverageDonationAmountAsync());
        /// <summary>
        /// get the balance average of all donations related to specific user by user id
        /// </summary>
        // GET api/donations/analytics/average/by-user/userId123
        [HttpGet("analytics/average/by-user/{userId}")]
        public async Task<IActionResult> GetAverageByUser(string userId)
            => Ok(await _service.GetAverageDonationAmountByUserAsync(userId));
        /// <summary>
        /// get the balance average of all donations related to specific campaign by campaign id
        /// </summary>
        // GET api/donations/analytics/average/by-campaign/3
        [HttpGet("analytics/average/by-campaign/{campaignId:int}")]
        public async Task<IActionResult> GetAverageByCampaign(int campaignId)
            => Ok(await _service.GetAverageDonationAmountByCampaignAsync(campaignId));
        /// <summary>
        /// get list of the top donors,limit parm:The maximum number of top donors to return. Defaults to 10
        /// </summary>

        // GET api/donations/analytics/top-donors?limit=10
        [HttpGet("analytics/top-donors")]
        public async Task<IActionResult> GetTopDonors([FromQuery] int limit = 10)
            => Ok(await _service.GetTopDonorsByAmountAsync(limit));
        /// <summary>
        /// get list of the top campaigns in donation,limit parm:The maximum number of top campaigns to return. Defaults to 10
        /// </summary>

        // GET api/donations/analytics/top-campaigns?limit=10
        [HttpGet("analytics/top-campaigns")]
        public async Task<IActionResult> GetTopCampaigns([FromQuery] int limit = 10)
            => Ok(await _service.GetTopCampaignsByDonationsAsync(limit));

        // GET api/donations/analytics/trend?days=30
        /// <summary>
        /// get list of days included the total amount of money donated on each individual day within that timeframe
        /// </summary>

        [HttpGet("analytics/trend")]
        public async Task<IActionResult> GetTrend([FromQuery] int days = 30)
            => Ok(await _service.GetDonationsTrendAsync(days));
        /// <summary>
        /// get list of user ids included the count of donations for each user id within that timeframe
        /// </summary>

        // GET api/donations/analytics/frequency-by-user
        [HttpGet("analytics/frequency-by-user")]
        public async Task<IActionResult> GetFrequencyByUser()
            => Ok(await _service.GetDonationFrequencyByUserAsync());

        // =====================================================================
        // Dashboard & Reporting
        // =====================================================================

        // GET api/donations/dashboard/latest?limit=10
        /// <summary>
        /// get list of latest donations,limit parm:The maximum number of latest donation to return. Defaults to 10
        /// </summary>

        [HttpGet("dashboard/latest")]
        public async Task<IActionResult> GetLatest([FromQuery] PaginationParametersDto parametersDto, [FromQuery] int limit = 10)
            => Ok(await _service.GetLatestDonationsAsync(parametersDto, limit));

        /// <summary>
        /// get list of largest donations,limit parm:The maximum number of largest donation to return. Defaults to 10
        /// </summary>
        // GET api/donations/dashboard/largest?limit=10
        [HttpGet("dashboard/largest")]
        public async Task<IActionResult> GetLargest([FromQuery] PaginationParametersDto parametersDto, [FromQuery] int limit = 10)
            => Ok(await _service.GetLargestDonationsAsync(parametersDto, limit));

        // GET api/donations/dashboard/per-campaign-count
        /// <summary>
        /// get list of campaign ids with count for their donations
        /// </summary>
        [HttpGet("dashboard/per-campaign-count")]
        public async Task<IActionResult> GetPerCampaignCount()
            => Ok(await _service.GetDonationsPerCampaignCountAsync());
        /// <summary>
        /// get list of user ids with count for their donations
        /// </summary>
        // GET api/donations/dashboard/per-user-count
        [HttpGet("dashboard/per-user-count")]
        public async Task<IActionResult> GetPerUserCount()
            => Ok(await _service.GetDonationsPerUserCountAsync());
        /// <summary>
        /// get the balance of today donations
        /// </summary>
        // GET api/donations/dashboard/today-total
        [HttpGet("dashboard/today-total")]
        public async Task<IActionResult> GetTodayTotal()
            => Ok(await _service.GetTodayDonationsTotalAsync());
        /// <summary>
        /// get the balance of this week donations
        /// </summary>
        // GET api/donations/dashboard/week-total
        [HttpGet("dashboard/week-total")]
        public async Task<IActionResult> GetWeekTotal()
            => Ok(await _service.GetThisWeekDonationsTotalAsync());
        /// <summary>
        /// get the balance of this month donations
        /// </summary>
        // GET api/donations/dashboard/month-total
        [HttpGet("dashboard/month-total")]
        public async Task<IActionResult> GetMonthTotal()
            => Ok(await _service.GetThisMonthDonationsTotalAsync());

        // =====================================================================
        // Financial Reporting
        // =====================================================================
        /// <summary>
        /// get list of donations ( sum of balance ) grouped by month for a specific year, where year parm: The year for which to generate the report.
        /// </summary>
        // GET api/donations/reports/monthly?year=2024
        [HttpGet("reports/monthly")]
        public async Task<IActionResult> GetMonthlyReport([FromQuery] int year)
            => Ok(await _service.GetMonthlyDonationsReportAsync(year));
        /// <summary>
        /// get list of donations ( sum of balance ) grouped by quarter for a specific year, where year parm: The year for which to generate the report.
        /// </summary>
        // GET api/donations/reports/quarterly?year=2024
        [HttpGet("reports/quarterly")]
        public async Task<IActionResult> GetQuarterlyReport([FromQuery] int year)
            => Ok(await _service.GetQuarterlyDonationsReportAsync(year));
        /// <summary>
        /// get list of donations ( sum of balance ) grouped by year for a specific years,where yearsback parm: The number of past years to include in the report. Defaults to 5.   
        /// </summary>
        // GET api/donations/reports/yearly?yearsBack=5
        [HttpGet("reports/yearly")]
        public async Task<IActionResult> GetYearlyReport([FromQuery] int yearsBack = 5)
            => Ok(await _service.GetYearlyDonationsReportAsync(yearsBack));
        /// <summary>
        /// Calculates the total donation amounts grouped by specific time-of-day segments.
        /// where returns A dictionary mapping predefined time-of-day categories (Morning, Afternoon, Evening, Night) 
        /// to the sum of donation amounts received during those hours.
        /// </summary>
        // GET api/donations/reports/by-time-of-day
        [HttpGet("reports/by-time-of-day")]
        public async Task<IActionResult> GetByTimeOfDay()
            => Ok(await _service.GetDonationsByTimeOfDayAsync());
        /// <summary>
        /// Calculates the total donation amounts grouped by the day of the week.
        /// where returns A dictionary mapping each day of the week (Monday through Sunday) 
        /// to the total sum of donations received on that day.
        /// </summary>
        // GET api/donations/reports/by-day-of-week
        [HttpGet("reports/by-day-of-week")]
        public async Task<IActionResult> GetByDayOfWeek()
            => Ok(await _service.GetDonationsByDayOfWeekAsync());
        /// <summary>
        /// Retrieves the total number of donation records registered within a specific date range.
        /// </summary>
        // GET api/donations/reports/record-count?startDate=2024-01-01&endDate=2024-12-31
        [HttpGet("reports/record-count")]
        public async Task<IActionResult> GetRecordCount(
            [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
            => Ok(await _service.GetDonationRecordCountForPeriodAsync(startDate, endDate));

        // =====================================================================
        // Campaign-Specific
        // =====================================================================
        /// <summary>
        /// get Total Donations Amount for specific By Campaign id
        /// </summary>
        // GET api/donations/campaigns/3/total-raised
        [HttpGet("campaigns/{campaignId:int}/total-raised")]
        public async Task<IActionResult> GetCampaignTotalRaised(int campaignId)
            => Ok(await _service.GetCampaignTotalRaisedAsync(campaignId));
        /// <summary>
        /// get Campaign Progress Percentage by campaign id
        /// </summary>
        // GET api/donations/campaigns/3/progress
        [HttpGet("campaigns/{campaignId:int}/progress")]
        public async Task<IActionResult> GetCampaignProgress(int campaignId)
            => Ok(await _service.GetCampaignProgressPercentageAsync(campaignId));
        /// <summary>
        /// Retrieves a collection of donations for a specific campaign, ordered from highest to lowest amount.
        ///containing the active donations associated with the campaign, 
        /// including the details of the user who made each donation.
        /// </summary>
        // GET api/donations/campaigns/3/donors
        [HttpGet("campaigns/{campaignId:int}/donors")]
        public async Task<IActionResult> GetCampaignDonors([FromQuery] PaginationParametersDto parametersDto, int campaignId)
            => Ok(await _service.GetUsersDonationsOfACampaignAsync(parametersDto, campaignId));

        /// <summary>
        ///get A dictionary mapping each specific date to the total sum of donations for that date, for a specific campaign.
        /// </summary>
        // GET api/donations/campaigns/3/timeline
        [HttpGet("campaigns/{campaignId:int}/timeline")]
        public async Task<IActionResult> GetCampaignTimeline(int campaignId)
            => Ok(await _service.GetCampaignDonationTimelineAsync(campaignId));

        // =====================================================================
        // User-Specific
        // =====================================================================
        /// <summary>
        /// get list of user donations history, including the details of the campaigns they donated to and the amounts donated.
        /// </summary>

        // GET api/donations/users/userId123/history
        [HttpGet("users/{userId}/history")]
        public async Task<IActionResult> GetUserHistory([FromQuery] PaginationParametersDto parametersDto, string userId)
            => Ok(await _service.GetUserDonationHistoryAsync(parametersDto, userId));

        /// <summary>
        /// get the date of the last donation made by a specific user, where userId parm: The unique identifier of the user for whom to retrieve the last donation date.
        /// </summary>
              // GET api/donations/users/userId123/last-donation-date
        [HttpGet("users/{userId}/last-donation-date")]
        public async Task<IActionResult> GetUserLastDonationDate(string userId)
        {
            var date = await _service.GetUserLastDonationDateAsync(userId);
            return date is null ?  NotFound(new ServiceResponse<object?> { Success = false, Message = "invalid user id." }) : Ok(date);
        }
        /// <summary>
        /// get list of campaigns that a specific user has donated to, where userId parm: The unique identifier of the user for whom to retrieve the list of donated campaigns.
        /// </summary>

        // GET api/donations/users/userId123/campaigns
        [HttpGet("users/{userId}/campaigns")]
        public async Task<IActionResult> GetCampaignsDonatedByUser(
            [FromQuery] PaginationParametersDto parametersDto,
            string userId)
            => Ok(await _service.GetCampaignsDonatedByUserAsync(parametersDto, userId));

        // =====================================================================
        // Bulk Operations
        // =====================================================================
        /// <summary>
        /// get the total number of donations transferred from one campaign to another, where from parm: The ID of the source campaign from which donations will be transferred. to parm: The ID of the target campaign to which donations will be transferred.
        /// </summary>

        // POST api/donations/bulk/transfer?from=1&to=2
        [HttpPost("bulk/transfer")]
        public async Task<IActionResult> TransferDonations(
            [FromQuery] int from, [FromQuery] int to)
        {
            var count = await _service.TransferDonationsToCampaignAsync(from, to);
            return Ok( count );
        }
        /// <summary>
        /// get the total number of donations deleted that are older than a specified number of days, where daysOld parm: The age threshold in days. Donations older than this number of days will be deleted. Defaults to 365 days.
        /// </summary>

        // DELETE api/donations/bulk/old?daysOld=365
        [HttpDelete("bulk/old")]
        public async Task<IActionResult> DeleteOldDonations([FromQuery] int daysOld = 365)
        {
            var count = await _service.DeleteOldDonationsAsync(daysOld);
            return Ok( count );
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
        /// <summary>
        /// get list of users who have made recurring donations, where minDonations parm: The minimum number of donations a user must have made to be considered a recurring donor. Defaults to 3.
        /// </summary>

        // GET api/donations/engagement/recurring?minDonations=3
        [HttpGet("engagement/recurring")]
        public async Task<IActionResult> GetRecurringDonors([FromQuery] PaginationParametersDto parametersDto, [FromQuery] int minDonations = 3)
            => Ok(await _service.GetRecurringDonorsAsync(parametersDto, minDonations));

        /// <summary>
        /// get list of users who made their first donation within a specified date range, where startDate parm: The start date of the range. endDate parm: The end date of the range.
        /// </summary>        
        // GET api/donations/engagement/first-time?startDate=2024-01-01&endDate=2024-12-31
        [HttpGet("engagement/first-time")]
        public async Task<IActionResult> GetFirstTimeDonors([FromQuery] PaginationParametersDto parametersDto,
            [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
            => Ok(await _service.GetFirstTimeDonorsAsync(parametersDto, startDate, endDate));

        /// <summary>
        /// get A dictionary mapping each unique user identifier to their total lifetime donation amount, 
        /// ordered from the highest lifetime value to the lowest.
        /// </summary>
        // GET api/donations/engagement/lifetime-value
        [HttpGet("engagement/lifetime-value")]
        public async Task<IActionResult> GetUserLifetimeValue()
            => Ok(await _service.GetUserLifetimeValueAsync());
        /// <summary>
        /// get list of users who have donated a total amount exceeding a specified threshold and have made a minimum number of donations, where minTotalAmount parm: The minimum total donation amount a user must have contributed to be considered loyal. Defaults to 1000. minDonations parm: The minimum number of donations a user must have made to be considered loyal. Defaults to 5.
        /// </summary>

        // GET api/donations/engagement/loyal?minAmount=1000&minDonations=5
        [HttpGet("engagement/loyal")]
        public async Task<IActionResult> GetLoyalDonors(
            [FromQuery] PaginationParametersDto parametersDto,
            [FromQuery] double minTotalAmount = 1000,
            [FromQuery] int minDonations = 5)
            => Ok(await _service.GetLoyalDonorsAsync(parametersDto, minTotalAmount, minDonations));

        // =====================================================================
        // Audit
        // =====================================================================
        /// <summary>
        /// get list of donations that exceed a specified threshold amount, where threshold parm: The donation amount threshold. Donations exceeding this amount will be considered suspicious. Defaults to 10000.
        /// </summary>

        // GET api/donations/audit/suspicious?threshold=10000
        [HttpGet("audit/suspicious")]
        public async Task<IActionResult> GetSuspicious([FromQuery] PaginationParametersDto parametersDto, [FromQuery] double threshold = 10000)
            => Ok(await _service.GetSuspiciousDonationsAsync(parametersDto, threshold));
    }
}
