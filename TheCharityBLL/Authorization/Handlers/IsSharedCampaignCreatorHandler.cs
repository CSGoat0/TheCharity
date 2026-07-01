using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using TheCharityBLL.Authorization.Requirements;
using IAuthorizationService = TheCharityBLL.Services.Abstraction.IAuthorizationService;

namespace TheCharityBLL.Authorization.Handlers
{
    public class IsSharedCampaignCreatorHandler : AuthorizationHandler<IsSharedCampaignCreatorRequirement>
    {
        private readonly IAuthorizationService _authService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IsSharedCampaignCreatorHandler(
            IAuthorizationService authService,
            IHttpContextAccessor httpContextAccessor)
        {
            _authService = authService;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            IsSharedCampaignCreatorRequirement requirement)
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

            var isCreator = await _authService.IsSharedCampaignCreatorAsync(userId, campaignId.Value);
            if (isCreator)
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
            if (httpContext.Request.RouteValues.TryGetValue("id", out var idObj) ||
                httpContext.Request.RouteValues.TryGetValue("sharedCampaignId", out idObj))
            {
                if (int.TryParse(idObj?.ToString(), out int id))
                    return id;
            }

            if (httpContext.Request.Query.TryGetValue("sharedCampaignId", out var queryValue))
            {
                if (int.TryParse(queryValue, out int id))
                    return id;
            }

            return null;
        }
    }
}
