using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TheCharityBLL.DTOs.PaginationDTOs;
using TheCharityBLL.DTOs.UserDTOs;
using TheCharityBLL.Extensions;
using TheCharityBLL.Services.Abstraction;
using TheCharityDAL.Entities;
using TheCharityDAL.Enums;
using TheCharityDAL.Repositories.Abstraction;

namespace TheCharityBLL.Services.Repository
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;
        private readonly IConfiguration _configuration;

        public UserService(
            IUserRepository userRepository,
            IMapper mapper,
            IConfiguration configuration,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }
        public async Task<string?> LoginAsync(string usernameOrEmail, string password)
        {
            // 1. Resolve user by username or email
            var user = await _userRepository.FindByNameOrEmailAsync(usernameOrEmail);

            if (user == null || user.IsDeleted)
                return null;

            // 2. Validate password (increment lockout counter on failure)
            var passwordValid = await _userRepository.CheckPasswordAsync(user, password);
            if (!passwordValid)
            {
                await _userRepository.AccessFailedAsync(user);
                return null;
            }

            // 3. Reset lockout on success
            await _userRepository.ResetAccessFailedCountAsync(user);

          
            return await GenerateJwtTokenAsync(user);
        }
        public async Task<string> GenerateJwtTokenAsync(UserResponseDTO UserDTO)
        {
            var user = _mapper.Map<User>(UserDTO);
            return await GenerateJwtTokenAsync(user);
        }


        private async Task<string> GenerateJwtTokenAsync(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            
            var roles = await _userRepository.GetUserRolesAsync(user.Id);

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["JWT:Key"]!));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                expires: DateTime.UtcNow.AddHours(3),
                claims: claims,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public async Task<IEnumerable<UserResponseDTO>> GetAllUsersAsync()
        {
            try
            {
                _logger.LogInformation("Getting all users");
                var users = await _userRepository.GetAllUsersAsync();
                return _mapper.Map<IEnumerable<UserResponseDTO>>(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                throw;
            }
        }

        //pagination
        public async Task<PagedResultDto<UserResponseDTO>> GetAllUsersAsync(PaginationParametersDto paginationDto)
        {
            try
            {
                _logger.LogInformation("Getting all users");
                var users = await _userRepository.GetAllUsersAsync(paginationDto.PageNumber,paginationDto.PageSize);
                var usersDto= _mapper.Map<IEnumerable<UserResponseDTO>>(users.Data);
                return users.ToPagedResult(usersDto, paginationDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                throw;
            }
        }

        public async Task<bool> ValidatePasswordAsync(string userId, string password)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            try
            {
                _logger.LogDebug("Validating password for user: {UserId}", userId);
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                    throw new KeyNotFoundException($"User with ID {userId} not found");

                return await _userRepository.CheckPasswordAsync(user, password);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating password for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<IdentityResult> ChangeUserPasswordAsync(string userId, ChangePasswordDTO changePasswordDTO)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            if (changePasswordDTO == null)
                throw new ArgumentNullException(nameof(changePasswordDTO));

            if (string.IsNullOrWhiteSpace(changePasswordDTO.CurrentPassword))
                throw new ArgumentException("Current password cannot be null or empty", nameof(changePasswordDTO.CurrentPassword));

            if (string.IsNullOrWhiteSpace(changePasswordDTO.NewPassword))
                throw new ArgumentException("New password cannot be null or empty", nameof(changePasswordDTO.NewPassword));

            if (changePasswordDTO.NewPassword.Equals(changePasswordDTO.ConfirmPassword))
                throw new ArgumentException("New password and confirmation password do not match");

            try
            {
                _logger.LogInformation("Changing password for user: {UserId}", userId);

                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                    throw new KeyNotFoundException($"User with ID {userId} not found");

                var result = await _userRepository.ChangePasswordAsync(user, changePasswordDTO.CurrentPassword, changePasswordDTO.NewPassword);

                if (result.Succeeded)
                    _logger.LogInformation("Password changed successfully for user: {UserId}", userId);
                else
                    _logger.LogWarning("Password change failed for user: {UserId}. Errors: {Errors}", userId, string.Join(", ", result.Errors));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<UserResponseDTO?> GetUserByIdAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            try
            {
                _logger.LogInformation("Getting user with ID: {UserId}", userId);
                var user = await _userRepository.GetUserByIdAsync(userId);
                return user != null ? _mapper.Map<UserResponseDTO>(user) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user with ID: {UserId}", userId);
                throw;
            }
        }
        public async Task<bool> IsExternalLoginLinkedAsync(string providerKey,string loginProvider,UserResponseDTO userDto)
        {
            var user = _mapper.Map<User>(userDto);
            var userLogins = await _userRepository.GetLoginsAsync(user);
            var existingLogin = userLogins.FirstOrDefault(l =>
                l.LoginProvider == loginProvider && l.ProviderKey == providerKey);
            if (existingLogin == null)
            {
               return false;
            }
            return true;
        }
        public async Task<UserResponseDTO?> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or empty", nameof(email));
            try
            {
                _logger.LogInformation("Getting user with email: {Email}", email);
                var user = await _userRepository.GetUserByEmailAsync(email);
                return user != null ? _mapper.Map<UserResponseDTO>(user) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user with email: {Email}", email);
                throw;
            }
        }
        public async Task<IdentityResult> CreateExternalUserAsync(string email)
        {
            return await _userRepository.CreateExternalUserAsync(email);
        }
        public async Task<IdentityResult> CreateUserAsync(CreateUserDTO createUserDTO)
        {
            if (createUserDTO == null)
                throw new ArgumentNullException(nameof(createUserDTO));

            if (string.IsNullOrWhiteSpace(createUserDTO.Password))
                throw new ArgumentException("Password cannot be null or empty", nameof(createUserDTO.Password));

            try
            {
                _logger.LogInformation("Creating new user with email: {Email}", createUserDTO.Email);

                // Map DTO to entity
                var user = _mapper.Map<User>(createUserDTO);

                var result = await _userRepository.CreateUserAsync(user, createUserDTO.Password);

                if (result.Succeeded)
                    _logger.LogInformation("User created successfully with ID: {UserId}", user.Id);
                else
                    _logger.LogWarning("User creation failed. Errors: {Errors}", string.Join(", ", result.Errors));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user with email: {Email}", createUserDTO.Email);
                throw;
            }
        }

        public async Task<IdentityResult> UpdateUserAsync(UpdateUserDTO updateUserDTO)
        {
            if (updateUserDTO == null || updateUserDTO.Id == null)
                throw new ArgumentNullException(nameof(updateUserDTO));

            try
            {
                _logger.LogInformation("Updating user with ID: {UserId}", updateUserDTO.Id);

                var user = await _userRepository.GetUserByIdAsync(updateUserDTO.Id);
                if (user == null)
                    throw new KeyNotFoundException($"User with ID {updateUserDTO.Id} not found");

                // Apply updates using entity methods
                if (!string.IsNullOrWhiteSpace(updateUserDTO.UserName))
                {
                    user.EditUsername(updateUserDTO.UserName);
                }

                if (!string.IsNullOrWhiteSpace(updateUserDTO.Address))
                {
                    user.EditAddress(updateUserDTO.Address);
                }

                // Update other properties directly
                if (!string.IsNullOrWhiteSpace(updateUserDTO.Email))
                    user.Email = updateUserDTO.Email;

                if (!string.IsNullOrWhiteSpace(updateUserDTO.PhoneNumber))
                    user.PhoneNumber = updateUserDTO.PhoneNumber;

                var result = await _userRepository.UpdateUserAsync(user);

                if (result.Succeeded)
                    _logger.LogInformation("User updated successfully with ID: {UserId}", user.Id);
                else
                    _logger.LogWarning("User update failed for ID: {UserId}. Errors: {Errors}", user.Id, string.Join(", ", result.Errors));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user with ID: {UserId}", updateUserDTO.Id);
                throw;
            }
        }

        public async Task<IdentityResult> DeleteUserAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            try
            {
                _logger.LogInformation("Deleting user with ID: {UserId}", userId);

                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                    throw new KeyNotFoundException($"User with ID {userId} not found");

                var result = await _userRepository.DeleteUserAsync(user);

                if (result.Succeeded)
                    _logger.LogInformation("User deleted successfully with ID: {UserId}", userId);
                else
                    _logger.LogWarning("User deletion failed for ID: {UserId}. Errors: {Errors}", userId, string.Join(", ", result.Errors));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<IdentityResult> RestoreUserAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("User ID cannot be null or empty", nameof(id));

            try
            {
                _logger.LogInformation("Restoring user with ID: {UserId}", id);
                var result = await _userRepository.RestoreUserAsync(id);

                if (result.Succeeded)
                    _logger.LogInformation("User restored successfully with ID: {UserId}", id);
                else
                    _logger.LogWarning("User restoration failed for ID: {UserId}. Errors: {Errors}", id, string.Join(", ", result.Errors));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring user with ID: {UserId}", id);
                throw;
            }
        }

        public async Task<bool> UserExistsAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            try
            {
                _logger.LogDebug("Checking if user exists with ID: {UserId}", userId);
                return await _userRepository.UserExistsAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user exists with ID: {UserId}", userId);
                throw;
            }
        }

      

        public Task<bool> IsUserDeletedAsync(string userId)
        {
            return _userRepository.IsUserDeletedAsync(userId);
        }

        public Task<bool> CheckPasswordAsync(string userId, string password)
        {
            return _userRepository.CheckPasswordAsync(userId, password);
        }

        public Task<IdentityResult> AddToRoleAsync(string userId, string role)
        {
            return _userRepository.AddToRoleAsync(userId, role);
        }

        public Task<IdentityResult> RemoveFromRoleAsync(string userId, string role)
        {
            return _userRepository.RemoveFromRoleAsync(userId, role);
        }

        public Task<IList<string>> GetUserRolesAsync(string userId)
        {
            return _userRepository.GetUserRolesAsync(userId);
        }

        public Task<bool> IsInRoleAsync(string userId, string role)
        {
            return _userRepository.IsInRoleAsync(userId, role);
        }


        public Task<string> GenerateEmailConfirmationTokenAsync(string email)
        {
            var user = _userRepository.GetUserByEmailAsync(email).Result;
            if (user == null)
            {
                throw new KeyNotFoundException($"User with Email {email} not found");
            }
            return _userRepository.GenerateEmailConfirmationTokenAsync(user);
        }
        public Task<string> GeneratePasswordResetTokenAsync(string userId)
        {
            var user = _userRepository.GetUserByIdAsync(userId).Result;
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }
            return _userRepository.GeneratePasswordResetTokenAsync(user);
        }
        public Task<IdentityResult> ResetPasswordAsync(string userId, string token, string newPassword)
        {
            return _userRepository.ResetPasswordAsync(userId, token, newPassword);
        }

        public Task<IdentityResult> ConfirmEmailAsync(string email, string token)
        {
            return _userRepository.ConfirmEmailAsync(email, token);
        }
       

        public async Task AddLoginAsync(UserResponseDTO UserDTO, UserLoginInfo loginInfo)
        {
            var user = _mapper.Map<User>(UserDTO);

            await _userRepository.AddLoginAsync(user, loginInfo);
        }
    }
}
