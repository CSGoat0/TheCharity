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
        private IUserService _userService;
        public ExternalLoginController(IUserService userService)
        {
            _userService = userService; 
        }
        /// <summary>
        /// login using external provider (Google, Facebook, etc.)
        /// </summary>

        [HttpGet("external-login")]
        public IActionResult ExternalLogin(string provider, string returnUrl)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "ExternalLogin", new { ReturnUrl = returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, provider);
        }
       
       
        [AllowAnonymous]
        [HttpGet("external-login-callback")]
        private async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? "/";

            if (remoteError != null)
                return BadRequest(new ServiceResponse<object?> { Success = false, Message = $"Error from external provider: {remoteError}" });

            // Get the external login info from the authentication cookie
            var authenticateResult = await HttpContext.AuthenticateAsync("ExternalCookie");

            if (!authenticateResult.Succeeded)
                return BadRequest(new ServiceResponse<object?> { Message = "Error loading external login information." ,Success=false});

            // Extract provider info
            var externalUser = authenticateResult.Principal;
            var providerKey = externalUser.FindFirstValue(ClaimTypes.NameIdentifier);
            var loginProvider = authenticateResult.Properties.Items[".AuthScheme"];
            var email = externalUser.FindFirstValue(ClaimTypes.Email);

            if (email == null)
                return BadRequest(new ServiceResponse<object?> { Success = false, Message = $"Email claim not received from: {loginProvider}" });

            // Check if user exists
            var user = await _userService.GetUserByEmailAsync(email);

            if (user == null)
            {
                //  no password needed for external users
                var createResult = await _userService.CreateExternalUserAsync(email);

                if (!createResult.Succeeded)
                    return BadRequest(new ServiceResponse<IEnumerable<string>> { Success = false, Message = "faild to create a new user.", Data = createResult.Errors.Select(e => e.Description) });

                user = await _userService.GetUserByEmailAsync(email);
            }
           
            if (user == null)
                return BadRequest(new ServiceResponse<object?> { Success = false, Message = "Failed to retrieve or create user." });

            // Check if external login is linked


            if (! await _userService.IsExternalLoginLinkedAsync(providerKey,loginProvider,user))
            {
                var loginInfo = new UserLoginInfo(loginProvider, providerKey, loginProvider);
               
                await _userService.AddLoginAsync(user, loginInfo);
            }

            // Generate JWT Token
            var token = await _userService.GenerateJwtTokenAsync(user);

            // Sign out of the external cookie
            await HttpContext.SignOutAsync("ExternalCookie");

            return Ok(new ServiceResponse<Dictionary<string, string>>
            {
                Data = new Dictionary<string, string>
                {
                    ["token"] = token,
                    ["returnUrl"] = returnUrl
                },
                Success = true,
                Message = "login successfully."
            });
        }

       
    }
}
