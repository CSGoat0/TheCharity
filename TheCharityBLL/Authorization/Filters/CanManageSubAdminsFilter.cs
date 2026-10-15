using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using TheCharityBLL.Services.Abstraction;
using TheCharityDAL.Repositories.Abstraction;
using IAuthorizationService = TheCharityBLL.Services.Abstraction.IAuthorizationService;

namespace TheCharityBLL.Authorization.Filters
{
    /// <summary>
    /// Action filter that enforces the CanManageSubAdmins permission.
    /// Runs after model binding, so it can read the request body when needed.
    ///
    /// Resolves the target organization id from, in order:
    ///   1. Route values "orgId" or "id".
    ///   2. Any action argument with an "OrganizationId" property.
    ///   3. Route value "contactId" → organization id via the organization repository.
    ///
    /// Then delegates to IAuthorizationService.CanManageSubAdminsAsync.
    /// </summary>
    public class CanManageSubAdminsFilter : IAsyncActionFilter
    {
        private readonly IAuthorizationService _authService;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly ILogger<CanManageSubAdminsFilter> _logger;

        public CanManageSubAdminsFilter(
            IAuthorizationService authService,
            IOrganizationRepository organizationRepository,
            ILogger<CanManageSubAdminsFilter> logger)
        {
            _authService = authService;
            _organizationRepository = organizationRepository;
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

            var organizationId = await ResolveOrganizationIdAsync(context);

            if (!organizationId.HasValue)
            {
                _logger.LogWarning(
                    "CanManageSubAdminsFilter could not resolve an organization id for user {UserId} on {Path}",
                    userId, context.HttpContext.Request.Path);
                context.Result = new BadRequestObjectResult(new
                {
                    message = "Organization ID could not be resolved for this request."
                });
                return;
            }

            var canManage = await _authService.CanManageSubAdminsAsync(userId, organizationId.Value);
            if (!canManage)
            {
                _logger.LogWarning(
                    "User {UserId} attempted to manage sub-admins for organization {OrganizationId} without permission",
                    userId, organizationId.Value);
                context.Result = new ForbidResult();
                return;
            }

            await next();
        }

        /// <summary>
        /// Attempts to determine which organization this request targets.
        /// </summary>
        private async Task<int?> ResolveOrganizationIdAsync(ActionExecutingContext context)
        {
            // Strategy 1: route values "orgId" or "id".
            var routeValues = context.HttpContext.Request.RouteValues;

            if (routeValues.TryGetValue("orgId", out var orgIdObj) &&
                int.TryParse(orgIdObj?.ToString(), out var routeOrgId))
            {
                return routeOrgId;
            }

            if (routeValues.TryGetValue("id", out var idObj) &&
                int.TryParse(idObj?.ToString(), out var routeId))
            {
                return routeId;
            }

            // Strategy 2: any action argument with an "OrganizationId" property.
            foreach (var argument in context.ActionArguments.Values)
            {
                if (argument == null) continue;

                var orgIdProp = argument.GetType().GetProperty("OrganizationId");
                if (orgIdProp == null) continue;

                var value = orgIdProp.GetValue(argument);
                if (value is int bodyOrgId && bodyOrgId > 0)
                {
                    return bodyOrgId;
                }
            }

            // Strategy 3: route value "contactId" → organization id via repository.
            if (routeValues.TryGetValue("contactId", out var contactIdObj) &&
                int.TryParse(contactIdObj?.ToString(), out var contactId))
            {
                var orgId = await _organizationRepository.GetOrganizationIdByContactMethodIdAsync(contactId);
                if (orgId.HasValue)
                {
                    return orgId.Value;
                }
            }

            return null;
        }
    }
}