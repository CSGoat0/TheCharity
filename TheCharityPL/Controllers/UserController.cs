using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;
using TheCharityBLL.Authorization.Attributes;
using TheCharityBLL.DTOs;
using TheCharityBLL.DTOs.UserDTOs;
using TheCharityBLL.DTOs.UserResponseDTOs;
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
        public async Task<IActionResult> GetAll([FromQuery] bool showDeleted = false)
        {
            try
            {
                _logger.LogInformation("Loading all users");

                var users = await _userService.GetAllUsersAsync();

                if (!showDeleted)
                    users = users.Where(u => !u.IsDeleted);

                var result = users.Select(u => new UserListResponseDto
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    FullName = u.FullName,
                    PhoneNumber = u.PhoneNumber,
                    IsDeleted = u.IsDeleted,
                    RegistrationDate = u.RegistrationDate,
                    EmailConfirmed = u.EmailConfirmed
                }).OrderByDescending(u => u.RegistrationDate).ToList();

                var api_response=new ServiceResponse<List<UserListResponseDto>> 
                {
                    Data = result,
                    Success = true,
                    Message = "Users loaded successfully."
                };
                return Ok(api_response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users");
                return StatusCode(500, new ServiceResponse<object?> {Success = false, Message = "An error occurred while loading users." });
            }
        }
        /// <summary>
        /// get user information by user id 
        /// </summary>
        
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
                    return NotFound(new ServiceResponse<object?> { Success = false, Message = $"User with ID '{id}' not found." });
                }

                var ResponseDto = new UserDetailResponseDto
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
                var api_response= new ServiceResponse<UserDetailResponseDto>
                {
                    Data = ResponseDto,
                    Success = true,
                    Message = "User details loaded successfully."
                };
                return Ok(api_response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading details for user ID: {UserId}", id);
                return StatusCode(500, new ServiceResponse<object?> { Success = false, Message = "An error occurred while loading user details." });
            }
        }
        /// <summary>
        /// create account method 
        /// </summary>
        // ─── POST api/user/register ──────────────────────────────────────────────────

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] CreateUserResponseDto ResponseDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ServiceResponse<ModelStateDictionary> { Data = ModelState, Success = false, Message = "your credentials is invalid" });

            try
            {
                _logger.LogInformation("Creating new user with email: {Email}", ResponseDto.Email);

                var existingUsers = await _userService.GetAllUsersAsync();

                if (existingUsers.Any(u => u.Email == ResponseDto.Email))
                    return Conflict(new ServiceResponse<object?> {Success = false, Message = "This email is already registered." });

                // FIX: original used return View() which is MVC-only — replaced with proper API response
                if (existingUsers.Any(u => u.UserName == ResponseDto.UserName))
                    return Conflict(new ServiceResponse<object?> { Success = false, Message = "This username is already taken." });

                var createUserDTO = new CreateUserDTO
                {
                    Email = ResponseDto.Email,
                    UserName = ResponseDto.UserName,
                    FullName = ResponseDto.FullName,
                    PhoneNumber = ResponseDto.PhoneNumber,
                    Address = ResponseDto.Address,
                    Password = ResponseDto.Password
                };

                var result = await _userService.CreateUserAsync(createUserDTO);

                if (result.Data.Succeeded)
                {
                    var token = await _userService.GenerateEmailConfirmationTokenAsync(createUserDTO.Email);
                    var confirmationLink = BuildFrontendLink("api/User/confirm-email", createUserDTO.Email, token);
                    await _emailService.SendEmailConfirmationAsync(createUserDTO.Email, confirmationLink);

                    return Ok(new ServiceResponse<object?> {Success=true, Message = "Registration successful. Please check your email to confirm your account." });
                }

                return BadRequest(new ServiceResponse<IEnumerable< string>>{ Success=false,Message="your credentials is invalid", Data= result.Data.Errors.Select(e => e.Description) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, new ServiceResponse<object?> {Success = false, Message = "An error occurred while creating the user." });
            }
        }
        /// <summary>
        /// login by enter user name, password and jwt token
        /// </summary>
        
        // ─── POST api/user/login ─────────────────────────────────────────────────────

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginResponseDto ResponseDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ServiceResponse<ModelStateDictionary> { Data = ModelState, Success = false, Message = "your credentials is invalid" });


            try
            {
                _logger.LogInformation("Login attempt for: {UserName}", ResponseDto.UserName);

                // Check email confirmed before attempting login
                var user = await _userService.GetUserByEmailAsync(ResponseDto.UserName)
                           ?? (await _userService.GetAllUsersAsync())
                               .FirstOrDefault(u => u.UserName == ResponseDto.UserName && !u.IsDeleted);

                if (user == null)
                    return Unauthorized(new ServiceResponse<object?> {Success = false, Message = "Invalid credentials." });

                if (!user.EmailConfirmed)
                    return Unauthorized(new ServiceResponse<object?> {Success = false, Message = "Please confirm your email before logging in." });

                // AuthService handles password validation, lockout, and JWT generation
                var token = await _userService.LoginAsync(ResponseDto.UserName, ResponseDto.Password);

                if (token == null)
                {
                    _logger.LogWarning("Login failed for: {UserName}", ResponseDto.UserName);
                    return Unauthorized(new ServiceResponse<object?> { Success = false, Message = "Invalid credentials." });
                }

                _logger.LogInformation("User logged in successfully: {UserName}", ResponseDto.UserName);
                return Ok(new ServiceResponse<string>{ Data = token?.Data, Message= $"User logged in successfully: {ResponseDto.UserName}",Success=true});
                }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for: {UserName}", ResponseDto.UserName);
                return StatusCode(500, new ServiceResponse<object?> {Success = false, Message = "An error occurred during login." });
            }
        }
        /// <summary>
        /// resend email confirmation ,to check if the email valid or not 
        /// </summary>
        // ─── POST api/user/resend-confirmation ───────────────────────────────────────

        [HttpPost("resend-confirmation")]
        [AllowAnonymous]
        public async Task<IActionResult> ResendEmailConfirmation([FromBody] string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest(new ServiceResponse<object?> {Success = false, Message = "Email is required." });

            try
            {
                var user = await _userService.GetUserByEmailAsync(email);
                if (user == null)
                    return NotFound(new ServiceResponse<object?> {Success = false, Message = "User not found." });

                if (user.EmailConfirmed)
                    return BadRequest(new ServiceResponse<object?> {Success = false, Message = "Email is already confirmed." });

                var token = await _userService.GenerateEmailConfirmationTokenAsync(email);
                var confirmationLink = BuildFrontendLink("api/User/confirm-email", email, token);
                await _emailService.SendEmailConfirmationAsync(email, confirmationLink);

                return Ok(new ServiceResponse<object?> {Success = true, Message = "If the email exists, a confirmation link has been sent." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending confirmation email");
                return StatusCode(500, new ServiceResponse<object?> { Success = false, Message = "An error occurred while resending the confirmation email." });
            }
        }
        
        // ─── POST api/user/confirm-email ─────────────────────────────────────────────

        [HttpGet("confirm-email")]
        [AllowAnonymous]
        private async Task<IActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string encodedToken)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(encodedToken))
                return BadRequest(new ServiceResponse<object?> {Success = false, Message = "Email and token are required." });

            try
            {
                var result = await _userService.ConfirmEmailAsync(email, encodedToken);

                if (result.Succeeded)
                    return Ok(new ServiceResponse<object?> {Success = true, Message = "Email confirmed successfully." });

                return BadRequest(new ServiceResponse<IEnumerable<string>>{Success=false, Message = "Email confirmation failed.", Data = result.Errors.Select(e => e.Description) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming email");
                return StatusCode(500, new ServiceResponse<object?> { Success = false, Message = "An error occurred while confirming the email." });
            }
        }
        /// <summary>
        /// change password method
        /// </summary>
      
        // ─── POST api/user/forgot-password ───────────────────────────────────────────

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest(new ServiceResponse<object?> { Success = false, Message = "Email is required." });

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

                return Ok(new ServiceResponse<object?> { Success = true, Message = "If your email is registered, you will receive a password reset link shortly." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing forgot password request");
                return StatusCode(500, new ServiceResponse<object?> { Success = false, Message = "An error occurred while processing your request." });
            }
        }

        // ─── POST api/user/reset-password ────────────────────────────────────────────

        [HttpPost("reset-password")]
        [AllowAnonymous]
        private async Task<IActionResult> ResetPassword([FromBody] ResetPasswordResponseDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ServiceResponse<ModelStateDictionary> { Data = ModelState, Success = false, Message = "your credentials is invalid" });


            try
            {
                var user = await _userService.GetUserByEmailAsync(model.Email);

                // Return Ok to avoid email enumeration
                if (user == null)
                    return Ok(new ServiceResponse<object?> { Success = true, Message = "Password has been reset successfully." });

                var result = await _userService.ResetPasswordAsync(user.Id, model.Token, model.Password);

                if (result.Succeeded)
                {
                    try { await _emailService.SendPasswordChangedNotificationAsync(user.Email); }
                    catch (Exception ex) { _logger.LogError(ex, "Failed to send password change notification"); }

                    return Ok(new ServiceResponse<object?> { Success = true, Message = "Password has been reset successfully." });
                }

                return BadRequest(new ServiceResponse<IEnumerable<string>>{Message="your credentials is invalid",Success=false, Data = result.Errors.Select(e => e.Description) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password");
                return StatusCode(500, new ServiceResponse<object?> {Success = false, Message = "An error occurred while resetting the password." });
            }
        }
        /// <summary>
        /// update user info method by user id
        /// </summary>
        
        // ─── PUT api/user/{id} ───────────────────────────────────────────────────────

        [HttpPut("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Update(string id, [FromBody] EditUserResponseDto ResponseDto)
        {
            if (id != ResponseDto.Id)
                return BadRequest(new ServiceResponse<object?> {Success = false, Message = "ID mismatch." });

            if (!ModelState.IsValid)
                return BadRequest(new ServiceResponse<ModelStateDictionary> { Data = ModelState, Success = false, Message = "your credentials is invalid" });


            try
            {
                _logger.LogInformation("Updating user ID: {UserId}", id);

                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound(new ServiceResponse<object?> {Success=false, Message = $"User with ID '{id}' not found." });

                var existingUsers = await _userService.GetAllUsersAsync();

                if (existingUsers.Any(u => u.UserName == ResponseDto.UserName && u.Id != id))
                    return Conflict(new ServiceResponse<object?> {Success = false, Message = "This username is already taken." });

                if (existingUsers.Any(u => u.Email == ResponseDto.Email && u.Id != id))
                    return Conflict(new ServiceResponse<object?> { Success = false, Message = "This email is already registered." });

                var updateUserDTO = new UpdateUserDTO
                {
                    Id = id,
                    UserName = ResponseDto.UserName,
                    Email = ResponseDto.Email,
                    PhoneNumber = ResponseDto.PhoneNumber,
                    Address = ResponseDto.Address
                };

                var result = await _userService.UpdateUserAsync(updateUserDTO);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User updated successfully with ID: {UserId}", id);
                    return Ok(new ServiceResponse<object?> { Success = true, Message = $"User '{ResponseDto.UserName}' updated successfully." });
                }

                return BadRequest(new ServiceResponse<IEnumerable<string>>{Success=false,Message="your credentials is invalid", Data = result.Errors.Select(e => e.Description) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user ID: {UserId}", id);
                return StatusCode(500, new ServiceResponse<object?> { Success = false, Message = "An error occurred while updating the user." });
            }
        }
        /// <summary>
        /// change password method by user id
        /// </summary>
        // ─── PUT api/user/{id}/change-password ───────────────────────────────────────

        [HttpPut("{id}/change-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ChangePassword(string id, [FromBody] ChangePasswordResponseDto ResponseDto)
        {
            if (id != ResponseDto.UserId)
                return BadRequest(new ServiceResponse<object?> {Success = false, Message = "ID mismatch." });

            if (!ModelState.IsValid)
                return BadRequest(new ServiceResponse<ModelStateDictionary> { Data = ModelState, Success = false, Message = "your credentials is invalid" });


            try
            {
                _logger.LogInformation("Changing password for user ID: {UserId}", id);

                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound(new ServiceResponse<object?> { Success = false, Message = $"User with ID '{id}' not found." });

                var changePasswordDTO = new ChangePasswordDTO
                {
                    CurrentPassword = ResponseDto.CurrentPassword,
                    NewPassword = ResponseDto.NewPassword,
                    ConfirmPassword = ResponseDto.ConfirmPassword
                };

                var result = await _userService.ChangeUserPasswordAsync(user.Id, changePasswordDTO);

                if (result.Succeeded)
                {
                    await _emailService.SendPasswordChangedNotificationAsync(user.Email);
                    _logger.LogInformation("Password changed successfully for user ID: {UserId}", id);
                    return Ok(new ServiceResponse<object?> {Success = true, Message = "Password changed successfully." });
                }

                return BadRequest(new ServiceResponse<IEnumerable<string>> { Success = false, Message = "your credentials is invalid", Data = result.Errors.Select(e => e.Description) });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user ID: {UserId}", id);
                return StatusCode(500, new ServiceResponse<object?> { Success = false, Message = "An error occurred while changing the password." });
            }
        }
        /// <summary>
        /// delete user by user id
        /// </summary>
       
        // ─── DELETE api/user/{id} ────────────────────────────────────────────────────

        [HttpDelete("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                _logger.LogInformation("Deleting user ID: {UserId}", id);

                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound(new ServiceResponse<object?> {Success = false, Message = $"User with ID '{id}' not found." });

                var currentUserId = GetCurrentUserId();
                var result = await _userService.DeleteUserAsync(user.Id);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User deleted successfully with ID: {UserId}", id);

                    // FIX: removed LogoutAsync() call — JWT is stateless, client discards the token
                    if (currentUserId == user.Id)
                        return Ok(new ServiceResponse<object?> { Message = "Your account has been deleted. Please discard your token.", Success = true });

                    return Ok(new ServiceResponse<object?> { Success = true, Message = $"User '{user.UserName}' deleted successfully." });
                }

                return BadRequest(new ServiceResponse<object?> {Success = false, Message = "An error occurred while deleting the user." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user ID: {UserId}", id);
                return StatusCode(500, new ServiceResponse<object?> { Success = false, Message = "An error occurred while deleting the user." });
            }
        }
        /// <summary>
        /// restore user deleted by user id
        /// </summary>
      
        // ─── POST api/user/restore/{id} ──────────────────────────────────────────────

        [HttpPost("restore/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Restore(string id)  // FIX: removed [FromBody] — id comes from route
        {
            try
            {
                _logger.LogInformation("Restoring user ID: {UserId}", id);

                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound(new ServiceResponse<object?> {Success=false, Message = $"User with ID '{id}' not found." });

                var result = await _userService.RestoreUserAsync(id);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User restored successfully with ID: {UserId}", id);
                    return Ok(new ServiceResponse<object?> { Success = true, Message = "User restored successfully." });
                }

                return BadRequest(new ServiceResponse<object?> { Success = false, Message = "An error occurred while restoring the user." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring user ID: {UserId}", id);
                return StatusCode(500, new ServiceResponse<object?> {Success = false, Message = "An error occurred while restoring the user." });
            }
        }

        // ─── Role Management ─────────────────────────────────────────────────────────

        /// <summary>
        /// Assign a role to a user (SuperAdmin only)
        /// </summary>
        [HttpPost("{userId}/roles")]
        [IsSuperAdmin] // Only SuperAdmin can assign roles
        public async Task<IActionResult> AssignRole(string userId, [FromBody] AssignRoleRequest request)
        {
            try
            {
                // Check if user exists
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                    return NotFound(new ServiceResponse<object?> {Success = false, Message = $"User with ID '{userId}' not found." });

                // Check if role exists in Identity
                var roles = await _userService.GetUserRolesAsync(userId);
                if (roles.Data.Contains(request.Role))
                    return BadRequest(new ServiceResponse<object?> {Success=false, Message = $"User already has role '{request.Role}'." });

                var result = await _userService.AddToRoleAsync(userId, request.Role);

                if (result.Data.Succeeded)
                {
                    _logger.LogInformation($"Role '{request.Role}' assigned to user {userId}");
                    return Ok(new ServiceResponse<object?> { Success = false, Message = $"Role '{request.Role}' assigned successfully." });
                }

                return BadRequest(new ServiceResponse<IEnumerable<string>> {Data= result.Data.Errors.Select(e => e.Description) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error assigning role to user {userId}");
                return StatusCode(500, new ServiceResponse<object?> { Message = "An error occurred while assigning the role.", Success = false });
            }
        }

        /// <summary>
        /// Remove a role from a user (SuperAdmin only)
        /// </summary>
        [HttpDelete("{userId}/roles/{role}")]
        [IsSuperAdmin]
        public async Task<IActionResult> RemoveRole(string userId, string role)
        {
            try
            {
                // Check if user exists
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                    return NotFound(new ServiceResponse<object?> {Success = false, Message = $"User with ID '{userId}' not found." });

                var roles = await _userService.GetUserRolesAsync(userId);
                if (!roles.Data.Contains(role))
                    return BadRequest(new ServiceResponse<object?> {Success = false, Message = $"User does not have role '{role}'." });

                var result = await _userService.RemoveFromRoleAsync(userId, role);

                if (result.Data.Succeeded)
                {
                    _logger.LogInformation($"Role '{role}' removed from user {userId}");
                    return Ok(new ServiceResponse<object?> { Success = true, Message = $"Role '{role}' removed successfully." });
                }

                return BadRequest(new ServiceResponse<IEnumerable<string>>{Success=false,Message="your credentials invalid", Data = result.Data.Errors.Select(e => e.Description) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing role from user {userId}");
                return StatusCode(500, new ServiceResponse<object?> {Success = false, Message = "An error occurred while removing the role." });
            }
        }

        /// <summary>
        /// Get all roles for a user
        /// </summary>
        [HttpGet("{userId}/roles")]
        [IsSuperAdmin]
        public async Task<IActionResult> GetUserRoles(string userId)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                    return NotFound(new ServiceResponse<object?> { Success = false, Message = $"User with ID '{userId}' not found." });

                var roles = await _userService.GetUserRolesAsync(userId);
                return Ok(new { roles });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting roles for user {userId}");
                return StatusCode(500, new ServiceResponse<object?> {Success = false, Message = "An error occurred while getting user roles." });
            }
        }

        /// <summary>
        /// Get all available roles in the system
        /// </summary>
        [HttpGet("roles/all")]
        [IsSuperAdmin]
        public async Task<IActionResult> GetAllRoles()
        {
            try
            {
                // Get all roles from Identity
                var roles = new List<string> { "SuperAdmin", "User" }; // Add any other global roles
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all roles");
                return StatusCode(500, new ServiceResponse<object?> { Message = "An error occurred while getting roles.",Success=false });
            }
        }

        // ─── Seed SuperAdmin ─────────────────────────────────────────────────────────

        /// <summary>
        /// Seed the first SuperAdmin (should be disabled in production or protected)
        /// </summary>
        [HttpPost("seed-superadmin")]
        [AllowAnonymous] // Or use a secret key/token
        public async Task<IActionResult> SeedSuperAdmin([FromBody] CreateSuperAdminRequest request)
        {
            // WARNING: This should be protected with a secret key or environment check
            // Consider using: if (!Environment.IsDevelopment()) return NotFound();

            try
            {
                // Check if any SuperAdmin exists
                var allUsers = await _userService.GetAllUsersAsync();
                foreach (var user in allUsers)
                {
                    var roles = await _userService.GetUserRolesAsync(user.Id);
                    if (roles.Data.Contains("SuperAdmin"))
                        return BadRequest(new ServiceResponse<object?> {Success = false, Message = "SuperAdmin already exists." });
                }

                // Create the user
                var createUserDto = new CreateUserDTO
                {
                    Email = request.Email,
                    UserName = request.UserName,
                    FullName = request.FullName,
                    Password = request.Password,
                    PhoneNumber = request.PhoneNumber
                };

                var result = await _userService.CreateUserAsync(createUserDto);
                if (!result.Data.Succeeded)
                    return BadRequest(new ServiceResponse<IEnumerable<string>>{ Data = result.Data.Errors.Select(e => e.Description) ,Success=false,Message="faild to create user"});

                // Get the created user
                var createdUser = await _userService.GetUserByEmailAsync(request.Email);
                if (createdUser == null)
                    return BadRequest(new ServiceResponse<object?> { Success = false, Message = "Failed to create user." });

                // Assign SuperAdmin role
                var roleResult = await _userService.AddToRoleAsync(createdUser.Id, "SuperAdmin");
                if (!roleResult.Data.Succeeded)
                    return BadRequest(new ServiceResponse<IEnumerable<string>>{Data = roleResult.Data.Errors.Select(e => e.Description),Success=false,Message="faild to add role to the user." });

                return Ok(new ServiceResponse<object?>
                {
                    Message = $"SuperAdmin created for user id:{createdUser.Id} and Email:{createdUser.Email} successfully."
                   ,Success=true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding SuperAdmin");
                return StatusCode(500, new ServiceResponse<object?> { Message = "An error occurred while seeding SuperAdmin.",Success=false });
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
