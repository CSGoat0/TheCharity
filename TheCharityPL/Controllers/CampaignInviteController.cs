using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TheCharityBLL.Authorization.Attributes;
using TheCharityBLL.DTOs;
using TheCharityBLL.DTOs.CampaignDTOs;
using TheCharityBLL.DTOs.PaginationDTOs;
using TheCharityBLL.Services.Abstraction;

namespace TheCharityPL.Controllers
{
    [Route("api/campaigns")]
    [ApiController]
    [Authorize]
    public class CampaignInviteController : ControllerBase
    {
        private readonly ICampaignInviteService _inviteService;
        private readonly IUserService _userService;
        private readonly ILogger<CampaignInviteController> _logger;

        public CampaignInviteController(
            ICampaignInviteService inviteService,
            ILogger<CampaignInviteController> logger,
            IUserService userService)
        {
            _inviteService = inviteService;
            _logger = logger;
            _userService = userService;
        }

        // ==============================
        // Send Invite
        // ==============================

        /// <summary>
        /// Send an invite to an organization to join a shared campaign
        /// </summary>
        [HttpPost("{campaignId}/invites")]
        [IsSharedCampaignCreator] // Only creator org Admin/SubAdmin + SuperAdmin
        public async Task<IActionResult> SendInvite(
            int campaignId,
            [FromBody] SendInviteRequestDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _inviteService.SendInviteAsync(
                campaignId,
                request.OrganizationId,
                userId,
                request.ExpiresInDays);

            return HandleResponse(result);
        }

        // ==============================
        // Accept Invite
        // ==============================

        /// <summary>
        /// Accept an invite to join a shared campaign
        /// </summary>
        [HttpPost("invites/{inviteId}/accept")]
        [Authorize] // User must be authenticated, check happens in service
        public async Task<IActionResult> AcceptInvite(int inviteId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _inviteService.AcceptInviteAsync(inviteId, userId);
            return HandleResponse(result);
        }

        // ==============================
        // Reject Invite
        // ==============================

        /// <summary>
        /// Reject an invite to join a shared campaign
        /// </summary>
        [HttpPost("invites/{inviteId}/reject")]
        [Authorize] // User must be authenticated, check happens in service
        public async Task<IActionResult> RejectInvite(int inviteId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _inviteService.RejectInviteAsync(inviteId, userId);
            return HandleResponse(result);
        }

        // ==============================
        // Get Invites For Campaign
        // ==============================

        /// <summary>
        /// Get all invites for a shared campaign with pagination
        /// </summary>
        [HttpGet("{campaignId}/invites")]
        [IsSharedCampaignCreator] // Only creator org Admin/SubAdmin + SuperAdmin
        public async Task<IActionResult> GetInvitesForCampaign(
            [FromQuery] PaginationParametersDto parametersDto,
            int campaignId)
        {
            var result = await _inviteService.GetInvitesForCampaignAsync(parametersDto, campaignId);
            return HandleResponse(result);
        }

        // ==============================
        // Get Pending Invites For Organization
        // ==============================

        /// <summary>
        /// Get all pending invites for the current user's organization with pagination
        /// </summary>
        [HttpGet("invites/pending")]
        [Authorize]
        public async Task<IActionResult> GetPendingInvitesForOrganization(
            [FromQuery] PaginationParametersDto parametersDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Use paginated method to get organizations the user manages
            var organizationsResult = await _userService.GetOrganizationsUserManagesAsync(parametersDto, userId);
            if (!organizationsResult.Success || organizationsResult.Data?.Items == null)
                return Ok(new PagedResultDto<InviteResponseDto>
                {
                    Items = Enumerable.Empty<InviteResponseDto>(),
                    TotalCount = 0,
                    PageNumber = parametersDto.PageNumber,
                    PageSize = parametersDto.PageSize
                });

            // For this endpoint, we need all organizations to get all invites
            var allOrgsParams = new PaginationParametersDto
            {
                PageNumber = 1,
                PageSize = int.MaxValue
            };

            var allOrgsResult = await _userService.GetOrganizationsUserManagesAsync(allOrgsParams, userId);
            if (!allOrgsResult.Success || allOrgsResult.Data?.Items == null)
                return Ok(new PagedResultDto<InviteResponseDto>
                {
                    Items = Enumerable.Empty<InviteResponseDto>(),
                    TotalCount = 0,
                    PageNumber = parametersDto.PageNumber,
                    PageSize = parametersDto.PageSize
                });

            var organizations = allOrgsResult.Data.Items;

            // Get ALL invites from all organizations
            var allInvites = new List<InviteResponseDto>();
            foreach (var org in organizations)
            {
                var orgInvitesParams = new PaginationParametersDto
                {
                    PageNumber = 1,
                    PageSize = int.MaxValue
                };

                var result = await _inviteService.GetPendingInvitesForOrganizationAsync(orgInvitesParams, org.Id);
                if (result.Success && result.Data?.Items != null)
                {
                    allInvites.AddRange(result.Data.Items);
                }
            }

            // Apply pagination to the combined list
            var totalCount = allInvites.Count;
            var pagedResult = allInvites
                .Skip((parametersDto.PageNumber - 1) * parametersDto.PageSize)
                .Take(parametersDto.PageSize)
                .ToList();

            var response = new PagedResultDto<InviteResponseDto>
            {
                Items = pagedResult,
                TotalCount = totalCount,
                PageNumber = parametersDto.PageNumber,
                PageSize = parametersDto.PageSize
            };

            return Ok(new ServiceResponse<PagedResultDto<InviteResponseDto>>
            {
                Success = true,
                Data = response,
                Message = "Pending invites retrieved successfully."
            });
        }

        // ==============================
        // Get Invites Sent By User
        // ==============================

        /// <summary>
        /// Get all invites sent by the current user with pagination
        /// </summary>
        [HttpGet("invites/sent")]
        [Authorize]
        public async Task<IActionResult> GetInvitesSentByUser([FromQuery] PaginationParametersDto parametersDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _inviteService.GetInvitesSentByUserAsync(parametersDto, userId);
            return HandleResponse(result);
        }

        // ==============================
        // Has Pending Invite
        // ==============================

        /// <summary>
        /// Check if an organization has a pending invite for a campaign
        /// </summary>
        [HttpGet("{campaignId}/invites/pending/{organizationId}")]
        [IsSharedCampaignCreator] // Only creator org Admin/SubAdmin + SuperAdmin
        public async Task<IActionResult> HasPendingInvite(int campaignId, int organizationId)
        {
            var result = await _inviteService.HasPendingInviteAsync(campaignId, organizationId);
            return HandleResponse(result);
        }

        // ==============================
        // Get Expired Invites
        // ==============================

        /// <summary>
        /// Get all expired invites with pagination
        /// </summary>
        [HttpGet("invites/expired")]
        [IsSuperAdmin] // Only SuperAdmin can view expired invites
        public async Task<IActionResult> GetExpiredInvites(
            [FromQuery] PaginationParametersDto parametersDto,
            [FromQuery] DateTime cutoffDate)
        {
            var result = await _inviteService.GetExpiredInvitesAsync(parametersDto, cutoffDate);
            return HandleResponse(result);
        }

        // ==============================
        // Helper Methods
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