using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TheCharityBLL.DTOs.PaginationDTOs;
using TheCharityBLL.DTOs.UserDTOs;
using TheCharityBLL.Services.Abstraction;
using TheCharityBLL.ViewModels.User;

namespace TheCharityPL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
       
        private readonly ILogger<UserController> _logger;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public UserController(
            IUserService userService,
           
            ILogger<UserController> logger,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        // ─── Helpers ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Reads the authenticated user's ID directly from the JWT claim.
        /// No service/DB call needed — replaces the old GetCurrentUserAsync().
        /// </summary>
        private string? GetCurrentUserId()
            => User.FindFirstValue(ClaimTypes.NameIdentifier);

        // ─── GET api/user ────────────────────────────────────────────────────────────

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParametersDto paginationDto, [FromQuery] bool showDeleted = false)
        {
            try
            {
                _logger.LogInformation("Loading all users");

                var users = await _userService.GetAllUsersAsync(paginationDto);

                if (!showDeleted)
                    users.Items = users.Items.Where(u => !u.IsDeleted);

                var result = new PagedResultDto<UserListViewModel>
                {
                    Items = users.Items
             .Select(u => new UserListViewModel
             {
                 Id = u.Id,
                 UserName = u.UserName,
                 Email = u.Email,
                 FullName = u.FullName,
                 PhoneNumber = u.PhoneNumber,
                 IsDeleted = u.IsDeleted,
                 RegistrationDate = u.RegistrationDate,
                 EmailConfirmed = u.EmailConfirmed
             })
             .OrderByDescending(u => u.RegistrationDate)
             .ToList(),

                    TotalCount = users.TotalCount,
                    PageNumber = users.PageNumber,
                    PageSize = users.PageSize
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users");
                return StatusCode(500, new { message = "An error occurred while loading users." });
            }
        }

        // ─── GET api/user/{id} ───────────────────────────────────────────────────────

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                _logger.LogInformation("Loading details for user ID: {UserId}", id);

                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found", id);
                    return NotFound(new { message = $"User with ID '{id}' not found." });
                }

                var viewModel = new UserDetailViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    IsDeleted = user.IsDeleted,
                    DeletedOn = user.DeletedOn,
                    RegistrationDate = user.RegistrationDate,
                    UpdatedOn = user.UpdatedOn,
                    EmailConfirmed = user.EmailConfirmed,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    LockoutEnabled = user.LockoutEnabled,
                    LockoutEnd = user.LockoutEnd,
                    AccessFailedCount = user.AccessFailedCount
                };

                return Ok(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading details for user ID: {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while loading user details." });
            }
        }

        // ─── POST api/user/register ──────────────────────────────────────────────────

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] CreateUserViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _logger.LogInformation("Creating new user with email: {Email}", viewModel.Email);

                var existingUsers = await _userService.GetAllUsersAsync();

                if (existingUsers.Any(u => u.Email == viewModel.Email))
                    return Conflict(new { message = "This email is already registered." });

                // FIX: original used return View() which is MVC-only — replaced with proper API response
                if (existingUsers.Any(u => u.UserName == viewModel.UserName))
                    return Conflict(new { message = "This username is already taken." });

                var createUserDTO = new CreateUserDTO
                {
                    Email = viewModel.Email,
                    UserName = viewModel.UserName,
                    FullName = viewModel.FullName,
                    PhoneNumber = viewModel.PhoneNumber,
                    Address = viewModel.Address,
                    Password = viewModel.Password
                };

                var result = await _userService.CreateUserAsync(createUserDTO);

                if (result.Succeeded)
                {
                    var token = await _userService.GenerateEmailConfirmationTokenAsync(createUserDTO.Email);
                    var confirmationLink = BuildFrontendLink("api/User/confirm-email", createUserDTO.Email, token);
                    await _emailService.SendEmailConfirmationAsync(createUserDTO.Email, confirmationLink);

                    return Ok(new { message = "Registration successful. Please check your email to confirm your account." });
                }

                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, new { message = "An error occurred while creating the user." });
            }
        }

        // ─── POST api/user/login ─────────────────────────────────────────────────────

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _logger.LogInformation("Login attempt for: {UserName}", viewModel.UserName);

                // Check email confirmed before attempting login
                var user = await _userService.GetUserByEmailAsync(viewModel.UserName)
                           ?? (await _userService.GetAllUsersAsync())
                               .FirstOrDefault(u => u.UserName == viewModel.UserName && !u.IsDeleted);

                if (user == null)
                    return Unauthorized(new { message = "Invalid credentials." });

                if (!user.EmailConfirmed)
                    return Unauthorized(new { message = "Please confirm your email before logging in." });

                // AuthService handles password validation, lockout, and JWT generation
                var token = await _userService.LoginAsync(viewModel.UserName, viewModel.Password);

                if (token == null)
                {
                    _logger.LogWarning("Login failed for: {UserName}", viewModel.UserName);
                    return Unauthorized(new { message = "Invalid credentials." });
                }

                _logger.LogInformation("User logged in successfully: {UserName}", viewModel.UserName);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for: {UserName}", viewModel.UserName);
                return StatusCode(500, new { message = "An error occurred during login." });
            }
        }

        // ─── POST api/user/resend-confirmation ───────────────────────────────────────

        [HttpPost("resend-confirmation")]
        [AllowAnonymous]
        public async Task<IActionResult> ResendEmailConfirmation([FromBody] string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest(new { message = "Email is required." });

            try
            {
                var user = await _userService.GetUserByEmailAsync(email);
                if (user == null)
                    return NotFound(new { message = "User not found." });

                if (user.EmailConfirmed)
                    return BadRequest(new { message = "Email is already confirmed." });

                var token = await _userService.GenerateEmailConfirmationTokenAsync(email);
                var confirmationLink = BuildFrontendLink("api/User/confirm-email", email, token);
                await _emailService.SendEmailConfirmationAsync(email, confirmationLink);

                return Ok(new { message = "If the email exists, a confirmation link has been sent." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending confirmation email");
                return StatusCode(500, new { message = "An error occurred while resending the confirmation email." });
            }
        }

        // ─── POST api/user/confirm-email ─────────────────────────────────────────────

        [HttpGet("confirm-email")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string encodedToken)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(encodedToken))
                return BadRequest(new { message = "Email and token are required." });

            try
            {
                var result = await _userService.ConfirmEmailAsync(email, encodedToken);

                if (result.Succeeded)
                    return Ok(new { message = "Email confirmed successfully." });

                return BadRequest(new { message = "Email confirmation failed.", errors = result.Errors.Select(e => e.Description) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming email");
                return StatusCode(500, new { message = "An error occurred while confirming the email." });
            }
        }

        // ─── POST api/user/forgot-password ───────────────────────────────────────────

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest(new { message = "Email is required." });

            try
            {
                var user = await _userService.GetUserByEmailAsync(email);

                // Always return Ok to avoid revealing whether the email exists
                if (user != null)
                {
                    var token = await _userService.GeneratePasswordResetTokenAsync(user.Id);
                    var resetLink = BuildFrontendLink("reset-password", email, token);
                    await _emailService.SendPasswordResetAsync(email, resetLink);
                }

                return Ok(new { message = "If your email is registered, you will receive a password reset link shortly." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing forgot password request");
                return StatusCode(500, new { message = "An error occurred while processing your request." });
            }
        }

        // ─── POST api/user/reset-password ────────────────────────────────────────────

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var user = await _userService.GetUserByEmailAsync(model.Email);

                // Return Ok to avoid email enumeration
                if (user == null)
                    return Ok(new { message = "Password has been reset successfully." });

                var result = await _userService.ResetPasswordAsync(user.Id, model.Token, model.Password);

                if (result.Succeeded)
                {
                    try { await _emailService.SendPasswordChangedNotificationAsync(user.Email); }
                    catch (Exception ex) { _logger.LogError(ex, "Failed to send password change notification"); }

                    return Ok(new { message = "Password has been reset successfully." });
                }

                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password");
                return StatusCode(500, new { message = "An error occurred while resetting the password." });
            }
        }

        // ─── PUT api/user/{id} ───────────────────────────────────────────────────────

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] EditUserViewModel viewModel)
        {
            if (id != viewModel.Id)
                return BadRequest(new { message = "ID mismatch." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _logger.LogInformation("Updating user ID: {UserId}", id);

                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound(new { message = $"User with ID '{id}' not found." });

                var existingUsers = await _userService.GetAllUsersAsync();

                if (existingUsers.Any(u => u.UserName == viewModel.UserName && u.Id != id))
                    return Conflict(new { message = "This username is already taken." });

                if (existingUsers.Any(u => u.Email == viewModel.Email && u.Id != id))
                    return Conflict(new { message = "This email is already registered." });

                var updateUserDTO = new UpdateUserDTO
                {
                    Id = id,
                    UserName = viewModel.UserName,
                    Email = viewModel.Email,
                    PhoneNumber = viewModel.PhoneNumber,
                    Address = viewModel.Address
                };

                var result = await _userService.UpdateUserAsync(updateUserDTO);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User updated successfully with ID: {UserId}", id);
                    return Ok(new { message = $"User '{viewModel.UserName}' updated successfully." });
                }

                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user ID: {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the user." });
            }
        }

        // ─── PUT api/user/{id}/change-password ───────────────────────────────────────

        [HttpPut("{id}/change-password")]
        public async Task<IActionResult> ChangePassword(string id, [FromBody] ChangePasswordViewModel viewModel)
        {
            if (id != viewModel.UserId)
                return BadRequest(new { message = "ID mismatch." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _logger.LogInformation("Changing password for user ID: {UserId}", id);

                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound(new { message = $"User with ID '{id}' not found." });

                var changePasswordDTO = new ChangePasswordDTO
                {
                    CurrentPassword = viewModel.CurrentPassword,
                    NewPassword = viewModel.NewPassword,
                    ConfirmPassword = viewModel.ConfirmPassword
                };

                var result = await _userService.ChangeUserPasswordAsync(user.Id, changePasswordDTO);

                if (result.Succeeded)
                {
                    await _emailService.SendPasswordChangedNotificationAsync(user.Email);
                    _logger.LogInformation("Password changed successfully for user ID: {UserId}", id);
                    return Ok(new { message = "Password changed successfully." });
                }

                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user ID: {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while changing the password." });
            }
        }

        // ─── DELETE api/user/{id} ────────────────────────────────────────────────────

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                _logger.LogInformation("Deleting user ID: {UserId}", id);

                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound(new { message = $"User with ID '{id}' not found." });

                var currentUserId = GetCurrentUserId();
                var result = await _userService.DeleteUserAsync(user.Id);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User deleted successfully with ID: {UserId}", id);

                    // FIX: removed LogoutAsync() call — JWT is stateless, client discards the token
                    if (currentUserId == user.Id)
                        return Ok(new { message = "Your account has been deleted. Please discard your token.", selfDeleted = true });

                    return Ok(new { message = $"User '{user.UserName}' deleted successfully." });
                }

                return BadRequest(new { message = "An error occurred while deleting the user." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user ID: {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the user." });
            }
        }

        // ─── POST api/user/restore/{id} ──────────────────────────────────────────────

        [HttpPost("restore/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Restore(string id)  // FIX: removed [FromBody] — id comes from route
        {
            try
            {
                _logger.LogInformation("Restoring user ID: {UserId}", id);

                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound(new { message = $"User with ID '{id}' not found." });

                var result = await _userService.RestoreUserAsync(id);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User restored successfully with ID: {UserId}", id);
                    return Ok(new { message = "User restored successfully." });
                }

                return BadRequest(new { message = "An error occurred while restoring the user." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring user ID: {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while restoring the user." });
            }
        }

        // ─── POST api/user/send-notification ─────────────────────────────────────────

        [HttpPost("send-notification")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendNotification([FromBody] SendNotificationViewModel model)
        {
            try
            {
                await _emailService.SendNotificationAsync(model.Email, model.Subject, model.Message);
                return Ok(new { message = "Notification sent successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification");
                return StatusCode(500, new { message = "Failed to send notification. Please try again." });
            }
        }

        // ─── Private Helpers ─────────────────────────────────────────────────────────

        private string BuildFrontendLink(string path, string email, string token)
        {
            var frontendUrl = _configuration["FrontendUrl"];
            var encodedToken = Uri.EscapeDataString(token);
            return $"{frontendUrl}/{path}?email={email}&encodedToken={encodedToken}";
        }
    }
}
