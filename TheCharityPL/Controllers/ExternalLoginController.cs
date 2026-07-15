using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TheCharityBLL.DTOs;
using TheCharityBLL.Services.Abstraction;

namespace TheCharityPL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExternalLoginController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<ExternalLoginController> _logger;

        public ExternalLoginController(
            IUserService userService,
            ILogger<ExternalLoginController> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ==============================
        // GET: api/externallogin/external-login
        // ==============================

        /// <summary>
        /// Login using external provider (Google, Facebook, etc.)
        /// </summary>
        [HttpGet("external-login")]
        public IActionResult ExternalLogin(string provider, string returnUrl)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "ExternalLogin", new { ReturnUrl = returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, provider);
        }

        // ==============================
        // GET: api/externallogin/external-login-callback
        // ==============================

        [AllowAnonymous]
        [HttpGet("external-login-callback")]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? "/";

            if (remoteError != null)
            {
                return BadRequest(new ServiceResponse<object?>
                {
                    Success = false,
                    Message = $"Error from external provider: {remoteError}"
                });
            }

            try
            {
                _logger.LogInformation("Processing external login callback");

                // Get the external login info from the authentication cookie
                var authenticateResult = await HttpContext.AuthenticateAsync("ExternalCookie");

                if (!authenticateResult.Succeeded)
                {
                    return BadRequest(new ServiceResponse<object?>
                    {
                        Success = false,
                        Message = "Error loading external login information."
                    });
                }

                // Extract provider info
                var externalUser = authenticateResult.Principal;
                var providerKey = externalUser?.FindFirstValue(ClaimTypes.NameIdentifier);
                var loginProvider = authenticateResult.Properties?.Items[".AuthScheme"];
                var email = externalUser?.FindFirstValue(ClaimTypes.Email);

                if (string.IsNullOrEmpty(email))
                {
                    return BadRequest(new ServiceResponse<object?>
                    {
                        Success = false,
                        Message = $"Email claim not received from: {loginProvider}"
                    });
                }

                // Check if user exists
                var userResult = await _userService.GetUserByEmailAsync(email);

                if (!userResult.Success || userResult.Data == null)
                {
                    // Create new user (no password needed for external users)
                    var createResult = await _userService.CreateExternalUserAsync(email);

                    if (!createResult.Success)
                    {
                        return BadRequest(new ServiceResponse<IEnumerable<string>>
                        {
                            Success = false,
                            Message = "Failed to create a new user.",
                            Data = createResult.Data?.Errors.Select(e => e.Description) ?? new List<string>()
                        });
                    }

                    // Get the newly created user
                    userResult = await _userService.GetUserByEmailAsync(email);
                }

                if (!userResult.Success || userResult.Data == null)
                {
                    return BadRequest(new ServiceResponse<object?>
                    {
                        Success = false,
                        Message = "Failed to retrieve or create user."
                    });
                }

                // Check if external login is linked
                if (!await _userService.IsExternalLoginLinkedAsync(providerKey, loginProvider, userResult.Data))
                {
                    var loginInfo = new UserLoginInfo(loginProvider, providerKey, loginProvider);
                    await _userService.AddLoginAsync(userResult.Data, loginInfo);
                }

                // Generate JWT Token
                var token = await _userService.GenerateJwtTokenAsync(userResult.Data);

                // Sign out of the external cookie
                await HttpContext.SignOutAsync("ExternalCookie");

                _logger.LogInformation("External login successful for user: {Email}", email);

                return Ok(new ServiceResponse<Dictionary<string, string>>
                {
                    Success = true,
                    Data = new Dictionary<string, string>
                    {
                        ["token"] = token,
                        ["returnUrl"] = returnUrl
                    },
                    Message = "Login successful."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing external login callback");
                return StatusCode(500, new ServiceResponse<object?>
                {
                    Success = false,
                    Message = $"An error occurred during external login: {ex.Message}"
                });
            }
        }
    }
}