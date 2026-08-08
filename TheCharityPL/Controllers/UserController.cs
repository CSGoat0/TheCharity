using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;
using TheCharityBLL.Authorization.Attributes;
using TheCharityBLL.DTOs;
using TheCharityBLL.DTOs.PaginationDTOs;
using TheCharityBLL.DTOs.UserDTOs;
using TheCharityBLL.Services.Abstraction;

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

        // ==============================
        // Helper Methods
        // ==============================

        private string? GetCurrentUserId()
            => User.FindFirstValue(ClaimTypes.NameIdentifier);

        private IActionResult HandleResponse<T>(
            ServiceResponse<T> response,
            bool notFoundOnFailure = false)
        {
            if (!response.Success)
            {
                if (notFoundOnFailure)
                    return NotFound(response);

                return BadRequest(response);
            }
            return Ok(response);
        }

        private string BuildFrontendLink(string path, string email, string token)
        {
            var frontendUrl = _configuration["FrontendUrl"];
            var encodedToken = Uri.EscapeDataString(token);
            return $"{frontendUrl}/{path}?email={email}&encodedToken={encodedToken}";
        }

        // ==============================
        // GET: api/user
        // ==============================

        /// <summary>
        /// Get all users with pagination
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParametersDto parametersDto, [FromQuery] bool includeDeleted = false)
        {
            var result = await _userService.GetAllUsersAsync(parametersDto, includeDeleted);
            return HandleResponse(result);
        }

        // ==============================
        // GET: api/user/{id}
        // ==============================

        /// <summary>
        /// Get user by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _userService.GetUserByIdAsync(id);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        // ==============================
        // POST: api/user/register
        // ==============================

        /// <summary>
        /// Register a new user
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] CreateUserDTO responseDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ServiceResponse<ModelStateDictionary>
                {
                    Success = false,
                    Data = ModelState,
                    Message = "Invalid credentials."
                });

            var result = await _userService.CreateUserAsync(responseDto);

            if (!result.Success)
                return BadRequest(result);

            // Generate and send confirmation email
            var token = await _userService.GenerateEmailConfirmationTokenAsync(responseDto.Email);
            if (token.Success)
            {
                var confirmationLink = BuildFrontendLink("api/User/confirm-email", responseDto.Email, token.Data);
                await _emailService.SendEmailConfirmationAsync(responseDto.Email, confirmationLink);
            }

            return Ok(new ServiceResponse<object?>
            {
                Success = true,
                Message = "Registration successful. Please check your email to confirm your account."
            });
        }

        // ==============================
        // POST: api/user/login
        // ==============================

        /// <summary>
        /// Login user and return JWT token
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginResponseDto responseDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ServiceResponse<ModelStateDictionary>
                {
                    Success = false,
                    Data = ModelState,
                    Message = "Invalid credentials."
                });

            var result = await _userService.LoginAsync(responseDto.UserName, responseDto.Password);

            if (result.Success)
            {
                var user = await _userService.GetUserByEmailAsync(responseDto.UserName);
                return HandleResponse(new ServiceResponse<object?>
                {
                    Success = true,
                    Data = new
                    {
                        Token = result.Data,
                        User = user.Data
                    },
                    Message = "Login successful."
                });
            }

            return HandleResponse(result);
        }

        // ==============================
        // POST: api/user/resend-confirmation
        // ==============================

        /// <summary>
        /// Resend email confirmation
        /// </summary>
        [HttpPost("resend-confirmation")]
        [AllowAnonymous]
        public async Task<IActionResult> ResendEmailConfirmation([FromBody] string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest(new ServiceResponse<object?>
                {
                    Success = false,
                    Message = "Email is required."
                });

            var user = await _userService.GetUserByEmailAsync(email);
            if (!user.Success || user.Data == null)
                return NotFound(user);

            if (user.Data.EmailConfirmed)
                return BadRequest(new ServiceResponse<object?>
                {
                    Success = false,
                    Message = "Email is already confirmed."
                });

            var token = await _userService.GenerateEmailConfirmationTokenAsync(email);
            if (token.Success)
            {
                var confirmationLink = BuildFrontendLink("api/User/confirm-email", email, token.Data);
                await _emailService.SendEmailConfirmationAsync(email, confirmationLink);
            }

            return Ok(new ServiceResponse<object?>
            {
                Success = true,
                Message = "If the email exists, a confirmation link has been sent."
            });
        }

        // ==============================
        // GET: api/user/confirm-email
        // ==============================

        [HttpGet("confirm-email")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string encodedToken)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(encodedToken))
                return BadRequest(new ServiceResponse<object?>
                {
                    Success = false,
                    Message = "Email and token are required."
                });

            var result = await _userService.ConfirmEmailAsync(email, encodedToken);
            return HandleResponse(result);
        }

        // ==============================
        // POST: api/user/forgot-password
        // ==============================

        /// <summary>
        /// Request password reset
        /// </summary>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest(new ServiceResponse<object?>
                {
                    Success = false,
                    Message = "Email is required."
                });

            var user = await _userService.GetUserByEmailAsync(email);
            if (user.Success && user.Data != null)
            {
                var token = await _userService.GeneratePasswordResetTokenAsync(user.Data.Id);
                if (token.Success)
                {
                    var resetLink = BuildFrontendLink("reset-password", email, token.Data);
                    await _emailService.SendPasswordResetAsync(email, resetLink);
                }
            }

            // Always return Ok to avoid email enumeration
            return Ok(new ServiceResponse<object?>
            {
                Success = true,
                Message = "If your email is registered, you will receive a password reset link shortly."
            });
        }

        // ==============================
        // POST: api/user/reset-password
        // ==============================

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordResponseDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ServiceResponse<ModelStateDictionary>
                {
                    Success = false,
                    Data = ModelState,
                    Message = "Invalid credentials."
                });

            var user = await _userService.GetUserByEmailAsync(model.Email);
            if (!user.Success || user.Data == null)
                return Ok(new ServiceResponse<object?>
                {
                    Success = true,
                    Message = "Password has been reset successfully."
                });

            var result = await _userService.ResetPasswordAsync(user.Data.Id, model.Token, model.Password);

            if (result.Success)
            {
                await _emailService.SendPasswordChangedNotificationAsync(user.Data.Email);
                return Ok(new ServiceResponse<object?>
                {
                    Success = true,
                    Message = "Password has been reset successfully."
                });
            }

            return BadRequest(result);
        }

        // ==============================
        // PUT: api/user/{id}
        // ==============================

        /// <summary>
        /// Update user information
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateUserDTO responseDto)
        {
            if (id != responseDto.Id)
                return BadRequest(new ServiceResponse<object?>
                {
                    Success = false,
                    Message = "ID mismatch."
                });

            if (!ModelState.IsValid)
                return BadRequest(new ServiceResponse<ModelStateDictionary>
                {
                    Success = false,
                    Data = ModelState,
                    Message = "Invalid credentials."
                });

            var result = await _userService.UpdateUserAsync(responseDto);

            return HandleResponse(result);
        }

        // ==============================
        // PUT: api/user/{id}/change-password
        // ==============================

        /// <summary>
        /// Change user password
        /// </summary>
        [HttpPut("{id}/change-password")]
        public async Task<IActionResult> ChangePassword(string id, [FromBody] ChangePasswordDTO responseDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ServiceResponse<ModelStateDictionary>
                {
                    Success = false,
                    Data = ModelState,
                    Message = "Invalid credentials."
                });

            var result = await _userService.ChangeUserPasswordAsync(id, responseDto);

            if (result.Success)
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user.Success && user.Data != null)
                {
                    await _emailService.SendPasswordChangedNotificationAsync(user.Data.Email);
                }
            }

            return HandleResponse(result);
        }

        // ==============================
        // DELETE: api/user/{id}
        // ==============================

        /// <summary>
        /// Soft delete user
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _userService.DeleteUserAsync(id);
            return HandleResponse(result);
        }

        // ==============================
        // POST: api/user/restore/{id}
        // ==============================

        /// <summary>
        /// Restore deleted user
        /// </summary>
        [HttpPost("restore/{id}")]
        public async Task<IActionResult> Restore(string id)
        {
            var result = await _userService.RestoreUserAsync(id);
            return HandleResponse(result);
        }

        // ==============================
        // Role Management
        // ==============================

        /// <summary>
        /// Assign a role to a user (SuperAdmin only)
        /// </summary>
        [HttpPost("{userId}/roles")]
        [IsSuperAdmin]
        public async Task<IActionResult> AssignRole(string userId, [FromBody] AssignRoleRequest request)
        {
            var result = await _userService.AddToRoleAsync(userId, request.Role);
            return HandleResponse(result);
        }

        /// <summary>
        /// Remove a role from a user (SuperAdmin only)
        /// </summary>
        [HttpDelete("{userId}/roles/{role}")]
        [IsSuperAdmin]
        public async Task<IActionResult> RemoveRole(string userId, string role)
        {
            var result = await _userService.RemoveFromRoleAsync(userId, role);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get all roles for a user
        /// </summary>
        [HttpGet("{userId}/roles")]
        [IsSuperAdmin]
        public async Task<IActionResult> GetUserRoles(string userId)
        {
            var result = await _userService.GetUserRolesAsync(userId);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get all available roles in the system
        /// </summary>
        [HttpGet("roles/all")]
        [IsSuperAdmin]
        public IActionResult GetAllRoles()
        {
            var roles = new List<string> { "SuperAdmin", "User" };
            return Ok(roles);
        }

        // ==============================
        // Seed SuperAdmin
        // ==============================

        /// <summary>
        /// Seed the first SuperAdmin (should be disabled in production)
        /// </summary>
        [HttpPost("seed-superadmin")]
        [AllowAnonymous]
        public async Task<IActionResult> SeedSuperAdmin([FromBody] CreateUserDTO request)
        {
            try
            {
                // Check if any SuperAdmin exists
                var allUsers = await _userService.GetAllUsersAsync(new PaginationParametersDto { PageNumber = 1, PageSize = int.MaxValue });
                if (allUsers.Success && allUsers.Data != null)
                {
                    foreach (var userDto in allUsers.Data.Items)
                    {
                        var roles = await _userService.GetUserRolesAsync(userDto.Id);
                        if (roles.Success && roles.Data.Contains("SuperAdmin"))
                        {
                            return BadRequest(new ServiceResponse<object?>
                            {
                                Success = false,
                                Message = "SuperAdmin already exists."
                            });
                        }
                    }
                }

                // Create the user
                var createResult = await _userService.CreateUserAsync(new CreateUserDTO
                {
                    Email = request.Email,
                    UserName = request.UserName,
                    FullName = request.FullName,
                    Password = request.Password,
                    PhoneNumber = request.PhoneNumber
                });

                if (!createResult.Success)
                    return BadRequest(createResult);

                // Get the created user
                var user = await _userService.GetUserByEmailAsync(request.Email);
                if (!user.Success || user.Data == null)
                    return BadRequest(new ServiceResponse<object?>
                    {
                        Success = false,
                        Message = "Failed to create user."
                    });

                // Assign SuperAdmin role
                var roleResult = await _userService.AddToRoleAsync(user.Data.Id, "SuperAdmin");
                if (!roleResult.Success)
                    return BadRequest(roleResult);

                return Ok(new ServiceResponse<object?>
                {
                    Success = true,
                    Message = $"SuperAdmin created successfully for user: {user.Data.Email}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding SuperAdmin");
                return StatusCode(500, new ServiceResponse<object?>
                {
                    Success = false,
                    Message = $"An error occurred while seeding SuperAdmin: {ex.Message}"
                });
            }
        }
    }
}