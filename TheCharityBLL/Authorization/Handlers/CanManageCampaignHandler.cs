using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using TheCharityBLL.Authorization.Requirements;
using IAuthorizationService = TheCharityBLL.Services.Abstraction.IAuthorizationService;

namespace TheCharityBLL.Authorization.Handlers
{
    public class CanManageCampaignHandler : AuthorizationHandler<CanManageCampaignRequirement>
    {
        private readonly IAuthorizationService _authService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CanManageCampaignHandler(
            IAuthorizationService authService,
            IHttpContextAccessor httpContextAccessor)
        {
            _authService = authService;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            CanManageCampaignRequirement requirement)
        {
            var user = context.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                context.Fail();
                return;
            }

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                context.Fail();
                return;
            }

            // Get campaign ID from route or query
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                context.Fail();
                return;
            }

            var campaignId = GetCampaignId(httpContext);
            if (!campaignId.HasValue)
            {
                context.Fail();
                return;
            }

            var canManage = await _authService.CanManageCampaignAsync(userId, campaignId.Value);
            if (canManage)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
        }

        private int? GetCampaignId(HttpContext httpContext)
        {
            // Try route values
            if (httpContext.Request.RouteValues.TryGetValue("id", out var idObj) ||
                httpContext.Request.RouteValues.TryGetValue("campaignId", out idObj))
            {
                if (int.TryParse(idObj?.ToString(), out int id))
                    return id;
            }

            // Try query string
            if (httpContext.Request.Query.TryGetValue("campaignId", out var queryValue))
            {
                if (int.TryParse(queryValue, out int id))
                    return id;
            }

            return null;
        }
    }
}
