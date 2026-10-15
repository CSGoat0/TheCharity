using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using TheCharityDAL.Repositories.Abstraction;
using IAuthorizationService = TheCharityBLL.Services.Abstraction.IAuthorizationService;

namespace TheCharityBLL.Authorization.Filters
{
    /// <summary>
    /// Action filter that enforces the CanUpdatePaymentInfo permission.
    /// Runs after model binding, so it can read the request body when needed.
    ///
    /// Resolves the target organization id from, in order:
    ///   1. Route values "orgId" or "id".
    ///   2. Any action argument with an "OrganizationId" property.
    ///   3. Route value "paymentInfoId" → organization id via the organization repository.
    ///
    /// Then delegates to IAuthorizationService.CanUpdatePaymentInfoAsync.
    /// </summary>
    public class CanUpdatePaymentInfoFilter : IAsyncActionFilter
    {
        private readonly IAuthorizationService _authService;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly ILogger<CanUpdatePaymentInfoFilter> _logger;

        public CanUpdatePaymentInfoFilter(
            IAuthorizationService authService,
            IOrganizationRepository organizationRepository,
            ILogger<CanUpdatePaymentInfoFilter> logger)
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
                    "CanUpdatePaymentInfoFilter could not resolve an organization id for user {UserId} on {Path}",
                    userId, context.HttpContext.Request.Path);
                context.Result = new BadRequestObjectResult(new
                {
                    message = "Organization ID could not be resolved for this request."
                });
                return;
            }

            var canUpdate = await _authService.CanUpdatePaymentInfoAsync(userId, organizationId.Value);
            if (!canUpdate)
            {
                _logger.LogWarning(
                    "User {UserId} attempted to update payment info for organization {OrganizationId} without permission",
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

            // Strategy 3: route value "paymentInfoId" → organization id via repository.
            if (routeValues.TryGetValue("paymentInfoId", out var paymentInfoIdObj) &&
                int.TryParse(paymentInfoIdObj?.ToString(), out var paymentInfoId))
            {
                var orgId = await _organizationRepository.GetOrganizationIdByPaymentInfoIdAsync(paymentInfoId);
                if (orgId.HasValue)
                {
                    return orgId.Value;
                }
            }

            return null;
        }
    }
}