using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TheCharityBLL.Authorization.Requirements;
using IAuthorizationService = TheCharityBLL.Services.Abstraction.IAuthorizationService;

namespace TheCharityBLL.Authorization.Handlers
{
    public class CanPerformBulkOperationsHandler : AuthorizationHandler<CanPerformBulkOperationsRequirement>
    {
        private readonly IAuthorizationService _authService;

        public CanPerformBulkOperationsHandler(IAuthorizationService authService)
        {
            _authService = authService;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            CanPerformBulkOperationsRequirement requirement)
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

            var canPerform = await _authService.CanPerformBulkOperationsAsync(userId);
            if (canPerform)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
        }
    }
}
