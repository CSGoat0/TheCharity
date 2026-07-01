using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TheCharityBLL.DTOs.UserDTOs;
using TheCharityBLL.Services.Abstraction;
using TheCharityDAL.Entities;

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
                return BadRequest($"Error from external provider: {remoteError}");

            // Get the external login info from the authentication cookie
            var authenticateResult = await HttpContext.AuthenticateAsync("ExternalCookie");

            if (!authenticateResult.Succeeded)
                return BadRequest("Error loading external login information.");

            // Extract provider info
            var externalUser = authenticateResult.Principal;
            var providerKey = externalUser.FindFirstValue(ClaimTypes.NameIdentifier);
            var loginProvider = authenticateResult.Properties.Items[".AuthScheme"];
            var email = externalUser.FindFirstValue(ClaimTypes.Email);

            if (email == null)
                return BadRequest($"Email claim not received from: {loginProvider}");

            // Check if user exists
            var user = await _userService.GetUserByEmailAsync(email);

            if (user == null)
            {
                //  no password needed for external users
                var createResult = await _userService.CreateExternalUserAsync(email);

                if (!createResult.Succeeded)
                    return BadRequest(createResult.Errors);

                user = await _userService.GetUserByEmailAsync(email);
            }
           
            if (user == null)
                return BadRequest("Failed to retrieve or create user.");

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

            return Ok(new
            {
                token,
                returnUrl
            });
        }

       
    }
}
