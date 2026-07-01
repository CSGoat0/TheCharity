using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using TheCharityBLL.Authorization.Requirements;
using IAuthorizationService = TheCharityBLL.Services.Abstraction.IAuthorizationService;

namespace TheCharityBLL.Authorization.Handlers
{
    public class IsSuperAdminHandler : AuthorizationHandler<IsSuperAdminRequirement>
    {
        private readonly IAuthorizationService _authService;
        private readonly ILogger<IsSuperAdminHandler> _logger;

        public IsSuperAdminHandler(IAuthorizationService authService, ILogger<IsSuperAdminHandler> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            IsSuperAdminRequirement requirement)
        {
            var user = context.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                _logger.LogWarning("User is not authenticated");
                context.Fail();
                return;
            }

            // Log all claims to see what's coming from the token
            foreach (var claim in user.Claims)
            {
                _logger.LogInformation("Claim: {Type} = {Value}", claim.Type, claim.Value);
            }

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in claims");
                context.Fail();
                return;
            }

            _logger.LogInformation("Checking if user {UserId} is SuperAdmin", userId);

            var isSuperAdmin = await _authService.IsSuperAdminAsync(userId);

            _logger.LogInformation("Is SuperAdmin: {IsSuperAdmin}", isSuperAdmin);

            if (isSuperAdmin)
            {
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogWarning("User {UserId} is not a SuperAdmin", userId);
                context.Fail();
            }
        }
    }
}
