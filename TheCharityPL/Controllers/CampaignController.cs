using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheCharityBLL.DTOs;
using TheCharityBLL.Authorization.Attributes;
using TheCharityBLL.DTOs.CampaignDTOs;
using TheCharityBLL.Services.Abstraction;
using TheCharityDAL.Enums;

namespace TheCharityPL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CampaignController : ControllerBase
    {
        private readonly ICampaignService _campaignService;
        private readonly ILogger<CampaignController> _logger;

        public CampaignController(
            ICampaignService campaignService,
            ILogger<CampaignController> logger)
        {
            _campaignService = campaignService;
            _logger = logger;
        }

        // ==============================
        // Base Campaign Operations
        // ==============================

        /// <summary>
        /// Get all campaigns (with optional include deleted)
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] bool includeDeleted = false)
        {
            var result = await _campaignService.GetAllCampaignsAsync(includeDeleted);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get campaign by ID
        /// </summary>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _campaignService.GetCampaignByIdAsync(id);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Get campaign details with donations and statistics
        /// </summary>
        [HttpGet("{id:int}/details")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDetails(int id)
        {
            var result = await _campaignService.GetCampaignDetailsByIdAsync(id);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Update campaign
        /// </summary>
        [HttpPut("{id:int}")]
        [CanManageCampaign] // ← SuperAdmin + OrganizationAdmin + SubAdmin
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCampaignDto dto)
        {
            dto.Id = id;
            var result = await _campaignService.UpdateCampaignAsync(dto);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Delete campaign (soft delete)
        /// </summary>
        [HttpDelete("{id:int}")]
        [IsSuperAdmin] // ← ONLY SuperAdmin
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _campaignService.DeleteCampaignAsync(id);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Restore deleted campaign
        /// </summary>
        [HttpPatch("{id:int}/restore")]
        [IsSuperAdmin] // ← ONLY SuperAdmin
        public async Task<IActionResult> Restore(int id)
        {
            var result = await _campaignService.RestoreCampaignAsync(id);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        // ==============================
        // Campaign Deadline Operations
        // ==============================

        /// <summary>
        /// Get campaigns expiring soon
        /// </summary>
        /// <param name="daysThreshold">Number of days threshold (default 7)</param>
        [HttpGet("expiring-soon")]
        [AllowAnonymous]
        public async Task<IActionResult> GetExpiringSoon([FromQuery] int daysThreshold = 7)
        {
            var result = await _campaignService.GetCampaignsExpiringSoonAsync(daysThreshold);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get expired campaigns
        /// </summary>
        [HttpGet("expired")]
        [AllowAnonymous]
        public async Task<IActionResult> GetExpired()
        {
            var result = await _campaignService.GetExpiredCampaignsAsync();
            return HandleResponse(result);
        }

        /// <summary>
        /// Extend campaign deadline
        /// </summary>
        /// <param name="id">Campaign ID</param>
        /// <param name="newDeadline">New deadline date</param>
        [HttpPatch("{id:int}/extend-deadline")]
        [CanManageCampaign] // ← SuperAdmin + OrganizationAdmin + SubAdmin
        public async Task<IActionResult> ExtendDeadline(int id, [FromQuery] DateTime newDeadline)
        {
            var result = await _campaignService.ExtendCampaignDeadlineAsync(id, newDeadline);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Auto-expire campaigns (admin endpoint)
        /// </summary>
        [HttpPost("auto-expire")]
        [IsSuperAdmin] // ← ONLY SuperAdmin
        public async Task<IActionResult> AutoExpireCampaigns()
        {
            var result = await _campaignService.AutoExpireCampaignsAsync();
            return HandleResponse(result);
        }

        // ==============================
        // Solo Campaign Operations
        // ==============================

        /// <summary>
        /// Get all solo campaigns
        /// </summary>
        [HttpGet("solo")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllSolo([FromQuery] bool includeDeleted = false)
        {
            var result = await _campaignService.GetAllSoloCampaignsAsync(includeDeleted);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get solo campaign by ID
        /// </summary>
        [HttpGet("solo/{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSoloById(int id)
        {
            var result = await _campaignService.GetSoloCampaignByIdAsync(id);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Create solo campaign
        /// </summary>
        [HttpPost("solo")]
        [CanCreateCampaign] // ← SuperAdmin + OrganizationAdmin + SubAdmin
        public async Task<IActionResult> CreateSolo([FromBody] CreateSoloCampaignDto dto)
        {
            var result = await _campaignService.CreateSoloCampaignAsync(dto);
            if (!result.Success) return BadRequest(result);

            return CreatedAtAction(nameof(GetSoloById), new { id = result.Data }, result);
        }

        /// <summary>
        /// Update solo campaign
        /// </summary>
        [HttpPut("solo/{id:int}")]
        [CanManageCampaign] // ← SuperAdmin + OrganizationAdmin + SubAdmin
        public async Task<IActionResult> UpdateSolo(int id, [FromBody] UpdateSoloCampaignDto dto)
        {
            dto.Id = id;
            var result = await _campaignService.UpdateSoloCampaignAsync(dto);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Get solo campaigns by organization
        /// </summary>
        [HttpGet("solo/by-organization/{organizationId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSoloByOrganization(int organizationId)
        {
            var result = await _campaignService.GetSoloCampaignsByOrganizationIdAsync(organizationId);
            return HandleResponse(result);
        }

        // ==============================
        // Shared Campaign Operations
        // ==============================

        /// <summary>
        /// Get all shared campaigns
        /// </summary>
        [HttpGet("shared")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllShared([FromQuery] bool includeDeleted = false)
        {
            var result = await _campaignService.GetAllSharedCampaignsAsync(includeDeleted);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get shared campaign by ID
        /// </summary>
        [HttpGet("shared/{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSharedById(int id)
        {
            var result = await _campaignService.GetSharedCampaignByIdAsync(id);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Create shared campaign
        /// </summary>
        [HttpPost("shared")]
        [CanCreateCampaign] // ← SuperAdmin + OrganizationAdmin + SubAdmin
        public async Task<IActionResult> CreateShared([FromBody] CreateSharedCampaignDto dto)
        {
            var result = await _campaignService.CreateSharedCampaignAsync(dto);
            if (!result.Success) return BadRequest(result);

            return CreatedAtAction(nameof(GetSharedById), new { id = result.Data }, result);
        }

        /// <summary>
        /// Update shared campaign
        /// </summary>
        [HttpPut("shared/{id:int}")]
        [CanManageCampaign] // ← SuperAdmin + OrganizationAdmin + SubAdmin
        public async Task<IActionResult> UpdateShared(int id, [FromBody] UpdateSharedCampaignDto dto)
        {
            dto.Id = id;
            var result = await _campaignService.UpdateSharedCampaignAsync(dto);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Get shared campaigns by organization
        /// </summary>
        [HttpGet("shared/by-organization/{organizationId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSharedByOrganization(int organizationId)
        {
            var result = await _campaignService.GetSharedCampaignsByOrganizationIdAsync(organizationId);
            return HandleResponse(result);
        }

        /// <summary>
        /// Add organization to shared campaign
        /// </summary>
        [HttpPost("shared/{sharedCampaignId:int}/add-organization/{organizationId:int}")]
        [IsSharedCampaignCreator] // ← Only creator org Admin/SubAdmin + SuperAdmin
        public async Task<IActionResult> AddOrganizationToShared(int sharedCampaignId, int organizationId)
        {
            var result = await _campaignService.AddOrganizationToSharedCampaignAsync(sharedCampaignId, organizationId);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Remove organization from shared campaign
        /// </summary>
        [HttpDelete("shared/{sharedCampaignId:int}/remove-organization/{organizationId:int}")]
        [IsSharedCampaignCreator] // ← Only creator org Admin/SubAdmin + SuperAdmin
        public async Task<IActionResult> RemoveOrganizationFromShared(int sharedCampaignId, int organizationId)
        {
            var result = await _campaignService.RemoveOrganizationFromSharedCampaignAsync(sharedCampaignId, organizationId);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Get organization count for shared campaign
        /// </summary>
        [HttpGet("shared/{sharedCampaignId:int}/organization-count")]
        [AllowAnonymous]
        public async Task<IActionResult> GetOrganizationCountForShared(int sharedCampaignId)
        {
            var result = await _campaignService.GetOrganizationCountForSharedCampaignAsync(sharedCampaignId);
            return HandleResponse(result);
        }

        // ==============================
        // Campaign Progress Operations
        // ==============================

        /// <summary>
        /// Update campaign achieved money
        /// </summary>
        [HttpPatch("{campaignId:int}/money")]
        [CanManageCampaign] // ← SuperAdmin + OrganizationAdmin + SubAdmin
        public async Task<IActionResult> UpdateCampaignMoney(int campaignId, [FromBody] UpdateCampaignMoneyDto dto)
        {
            var result = await _campaignService.UpdateCampaignMoneyAsync(campaignId, dto.Amount);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Increment campaign money (add donation)
        /// </summary>
        [HttpPatch("{campaignId:int}/increment-money")]
        [AllowAnonymous]
        public async Task<IActionResult> IncrementCampaignMoney(int campaignId, [FromBody] IncrementCampaignMoneyDto dto)
        {
            var result = await _campaignService.IncrementCampaignMoneyAsync(campaignId, dto.Amount);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Update campaign status
        /// </summary>
        [HttpPatch("{campaignId:int}/status")]
        [CanManageCampaign] // ← SuperAdmin + OrganizationAdmin + SubAdmin
        public async Task<IActionResult> UpdateCampaignStatus(int campaignId, [FromQuery] CampaignStatus status)
        {
            var result = await _campaignService.UpdateCampaignStatusAsync(campaignId, status);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        // ==============================
        // Filtering & Querying
        // ==============================

        /// <summary>
        /// Get campaigns by status
        /// </summary>
        [HttpGet("filter/by-status")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByStatus([FromQuery] CampaignStatus status)
        {
            var result = await _campaignService.GetCampaignsByStatusAsync(status);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get campaigns by type
        /// </summary>
        [HttpGet("filter/by-type")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByType([FromQuery] CampaignType type)
        {
            var result = await _campaignService.GetCampaignsByTypeAsync(type);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get active campaigns only
        /// </summary>
        [HttpGet("active")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActive()
        {
            var result = await _campaignService.GetActiveCampaignsAsync();
            return HandleResponse(result);
        }

        /// <summary>
        /// Search campaigns by title or description
        /// </summary>
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> Search([FromQuery] string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return BadRequest(new { message = "Search term is required." });

            var result = await _campaignService.SearchCampaignsAsync(term);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get deleted campaigns
        /// </summary>
        [HttpGet("deleted")]
        [IsSuperAdmin] // ← ONLY SuperAdmin
        public async Task<IActionResult> GetDeleted()
        {
            var result = await _campaignService.GetDeletedCampaignsAsync();
            return HandleResponse(result);
        }

        /// <summary>
        /// Get solo campaigns by status
        /// </summary>
        [HttpGet("solo/by-status")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSoloByStatus([FromQuery] CampaignStatus status)
        {
            var result = await _campaignService.GetSoloCampaignsByStatusAsync(status);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get shared campaigns by status
        /// </summary>
        [HttpGet("shared/by-status")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSharedByStatus([FromQuery] CampaignStatus status)
        {
            var result = await _campaignService.GetSharedCampaignsByStatusAsync(status);
            return HandleResponse(result);
        }

        // ==============================
        // Advanced Filtering
        // ==============================

        /// <summary>
        /// Get campaigns by target range
        /// </summary>
        [HttpGet("filter/by-target-range")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByTargetRange([FromQuery] double minTarget, [FromQuery] double maxTarget)
        {
            var result = await _campaignService.GetCampaignsByTargetRangeAsync(minTarget, maxTarget);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get campaigns by achievement percentage
        /// </summary>
        [HttpGet("filter/by-achievement")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByAchievement([FromQuery] double minPercentage)
        {
            var result = await _campaignService.GetCampaignsByAchievementPercentageAsync(minPercentage);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get campaigns ending soon (remaining target amount)
        /// </summary>
        [HttpGet("ending-soon")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEndingSoon([FromQuery] double remainingValue = 1000)
        {
            var result = await _campaignService.GetCampaignsEndingSoonAsync(remainingValue);
            return HandleResponse(result);
        }

        // ==============================
        // Statistics & Analytics
        // ==============================

        /// <summary>
        /// Get total campaigns count
        /// </summary>
        [HttpGet("statistics/total-count")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTotalCount([FromQuery] bool includeDeleted = false)
        {
            var result = await _campaignService.GetTotalCampaignsCountAsync(includeDeleted);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get active campaigns count
        /// </summary>
        [HttpGet("statistics/active-count")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActiveCount()
        {
            var result = await _campaignService.GetTotalActiveCampaignsCountAsync();
            return HandleResponse(result);
        }

        /// <summary>
        /// Get solo campaigns count
        /// </summary>
        [HttpGet("statistics/solo-count")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSoloCount()
        {
            var result = await _campaignService.GetSoloCampaignsCountAsync();
            return HandleResponse(result);
        }

        /// <summary>
        /// Get shared campaigns count
        /// </summary>
        [HttpGet("statistics/shared-count")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSharedCount()
        {
            var result = await _campaignService.GetSharedCampaignsCountAsync();
            return HandleResponse(result);
        }

        /// <summary>
        /// Get total money raised
        /// </summary>
        [HttpGet("statistics/total-money")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTotalMoneyRaised()
        {
            var result = await _campaignService.GetTotalMoneyRaisedAsync();
            return HandleResponse(result);
        }

        /// <summary>
        /// Get average achievement percentage
        /// </summary>
        [HttpGet("statistics/average-achievement")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAverageAchievement()
        {
            var result = await _campaignService.GetAverageAchievementPercentageAsync();
            return HandleResponse(result);
        }

        /// <summary>
        /// Get campaign count by type
        /// </summary>
        [HttpGet("statistics/count-by-type")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCountByType()
        {
            var result = await _campaignService.GetCampaignCountByTypeAsync();
            return HandleResponse(result);
        }

        /// <summary>
        /// Get campaign count by status
        /// </summary>
        [HttpGet("statistics/count-by-status")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCountByStatus()
        {
            var result = await _campaignService.GetCampaignCountByStatusAsync();
            return HandleResponse(result);
        }

        /// <summary>
        /// Get complete campaign statistics dashboard
        /// </summary>
        [HttpGet("statistics/dashboard")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStatistics()
        {
            var result = await _campaignService.GetCampaignStatisticsAsync();
            return HandleResponse(result);
        }

        // ==============================
        // Featured & Trending
        // ==============================

        /// <summary>
        /// Get top campaigns by achievement percentage
        /// </summary>
        [HttpGet("trending/top-by-achievement")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTopByAchievement([FromQuery] int limit = 10)
        {
            var result = await _campaignService.GetTopCampaignsByAchievementAsync(limit);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get top campaigns by donation amount
        /// </summary>
        [HttpGet("trending/top-by-donations")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTopByDonations([FromQuery] int limit = 10)
        {
            var result = await _campaignService.GetTopCampaignsByDonationsAsync(limit);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get recent campaigns (last N days)
        /// </summary>
        [HttpGet("trending/recent")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRecent([FromQuery] int days = 30)
        {
            var result = await _campaignService.GetRecentCampaignsAsync(days);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get urgent campaigns (high achievement percentage)
        /// </summary>
        [HttpGet("trending/urgent")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUrgent([FromQuery] double minPercentage = 75)
        {
            var result = await _campaignService.GetUrgentCampaignsAsync(minPercentage);
            return HandleResponse(result);
        }

        // ==============================
        // Bulk Operations
        // ==============================

        /// <summary>
        /// Bulk update campaign status
        /// </summary>
        [HttpPatch("bulk/update-status")]
        [CanPerformBulkOperations] // ← ONLY SuperAdmin
        public async Task<IActionResult> BulkUpdateStatus([FromQuery] CampaignStatus oldStatus, [FromQuery] CampaignStatus newStatus)
        {
            var result = await _campaignService.BulkUpdateCampaignStatusAsync(oldStatus, newStatus);
            return HandleResponse(result);
        }

        /// <summary>
        /// Soft delete expired campaigns
        /// </summary>
        [HttpDelete("bulk/delete-expired")]
        [CanPerformBulkOperations] // ← ONLY SuperAdmin
        public async Task<IActionResult> DeleteExpired([FromQuery] int daysAfterCompletion = 30)
        {
            var result = await _campaignService.SoftDeleteExpiredCampaignsAsync(daysAfterCompletion);
            return HandleResponse(result);
        }

        // ==============================
        // Helper Methods
        // ==============================

        /// <summary>
        /// Get available campaign status options
        /// </summary>
        [HttpGet("options/statuses")]
        [AllowAnonymous]
        public IActionResult GetStatusOptions()
        {
            var values = Enum.GetValues(typeof(CampaignStatus))
                             .Cast<CampaignStatus>()
                             .Select(e => new
                             {
                                 Id = (int)e,
                                 Name = e.ToString()
                             });

            return Ok(values);
        }

        /// <summary>
        /// Get available campaign type options
        /// </summary>
        [HttpGet("options/types")]
        [AllowAnonymous]
        public IActionResult GetTypeOptions()
        {
            var values = Enum.GetValues(typeof(CampaignType))
                             .Cast<CampaignType>()
                             .Select(e => new
                             {
                                 Id = (int)e,
                                 Name = e.ToString()
                             });

            return Ok(values);
        }

        // ==============================
        // Response Handler
        // ==============================

        private IActionResult HandleResponse<T>(
            ServiceResponse<T> response,
            bool notFoundOnFailure = false)
        {
            if (!response.Success)
            {
                if (notFoundOnFailure)
                    return NotFound(response);

                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}