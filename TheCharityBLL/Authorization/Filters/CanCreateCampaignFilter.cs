using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using TheCharityBLL.Services.Abstraction;

namespace TheCharityBLL.Authorization.Filters
{
    public class CanCreateCampaignFilter : IAsyncActionFilter
    {
        private readonly IAuthorizationService _authService;
        private readonly ILogger<CanCreateCampaignFilter> _logger;

        public CanCreateCampaignFilter(
            IAuthorizationService authService,
            ILogger<CanCreateCampaignFilter> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = context.HttpContext.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Get organization ID from the bound DTO
            int? organizationId = null;

            foreach (var argument in context.ActionArguments.Values)
            {
                if (argument == null) continue;

                // Check for OrganizationId (CreateSoloCampaignDto)
                var orgIdProp = argument.GetType().GetProperty("OrganizationId");
                if (orgIdProp != null)
                {
                    organizationId = orgIdProp.GetValue(argument) as int?;
                    break;
                }

                // Check for CreatorOrganizationId (CreateSharedCampaignDto)
                var creatorOrgIdProp = argument.GetType().GetProperty("CreatorOrganizationId");
                if (creatorOrgIdProp != null)
                {
                    organizationId = creatorOrgIdProp.GetValue(argument) as int?;
                    break;
                }
            }

            if (!organizationId.HasValue)
            {
                context.Result = new BadRequestObjectResult(new
                {
                    message = "Organization ID is required to create a campaign."
                });
                return;
            }

            var canCreate = await _authService.CanCreateCampaignForOrganizationAsync(userId, organizationId.Value);
            if (!canCreate)
            {
                _logger.LogWarning("User {UserId} attempted to create campaign for organization {OrganizationId} without permission",
                    userId, organizationId);
                context.Result = new ForbidResult();
                return;
            }

            await next();
        }
    }
}
