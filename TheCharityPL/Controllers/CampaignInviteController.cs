using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TheCharityBLL.Authorization.Attributes;
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

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

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

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

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

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Get all invites for a shared campaign
        /// </summary>
        [HttpGet("{campaignId}/invites")]
        [IsSharedCampaignCreator] // Only creator org Admin/SubAdmin + SuperAdmin
        public async Task<IActionResult> GetInvitesForCampaign(int campaignId)
        {
            var result = await _inviteService.GetInvitesForCampaignAsync(campaignId);
            return Ok(result);
        }

        /// <summary>
        /// Get all pending invites for the current user's organization
        /// </summary>
        [HttpGet("invites/pending")]
        [Authorize]
        public async Task<IActionResult> GetPendingInvitesForOrganization()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Use paginated method with int.MaxValue to get ALL organizations
            var allParams = new PaginationParametersDto
            {
                PageNumber = 1,
                PageSize = int.MaxValue
            };

            var organizationsResult = await _userService.GetOrganizationsUserManagesAsync(allParams, userId);
            if (!organizationsResult.Success || organizationsResult.Data?.Items == null)
                return Ok(new List<InviteResponseDto>());

            var organizations = organizationsResult.Data.Items;

            // Get pending invites for all organizations the user manages
            var allInvites = new List<InviteResponseDto>();
            foreach (var org in organizations)
            {
                var result = await _inviteService.GetPendingInvitesForOrganizationAsync(org.Id);
                if (result.Success && result.Data != null)
                {
                    allInvites.AddRange(result.Data);
                }
            }

            return Ok(allInvites);
        }

        /// <summary>
        /// Get all invites sent by the current user
        /// </summary>
        [HttpGet("invites/sent")]
        [Authorize]
        public async Task<IActionResult> GetInvitesSentByUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _inviteService.GetInvitesSentByUserAsync(userId);
            return Ok(result);
        }

        /// <summary>
        /// Check if an organization has a pending invite for a campaign
        /// </summary>
        [HttpGet("{campaignId}/invites/pending/{organizationId}")]
        [IsSharedCampaignCreator] // Only creator org Admin/SubAdmin + SuperAdmin
        public async Task<IActionResult> HasPendingInvite(int campaignId, int organizationId)
        {
            var result = await _inviteService.HasPendingInviteAsync(campaignId, organizationId);
            return Ok(result);
        }
    }
}