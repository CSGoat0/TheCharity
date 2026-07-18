using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TheCharityBLL.DTOs;
using TheCharityBLL.DTOs.OrganizationDTOs;
using TheCharityBLL.DTOs.PaginationDTOs;
using TheCharityBLL.DTOs.UserDTOs;
using TheCharityBLL.Mapper;
using TheCharityBLL.Services.Abstraction;
using TheCharityDAL.Entities;
using TheCharityDAL.Repositories.Abstraction;

namespace TheCharityBLL.Services.Repository
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly UserMapper _userMapper;
        private readonly ILogger<UserService> _logger;
        private readonly IConfiguration _configuration;

        public UserService(
            IUserRepository userRepository,
            IConfiguration configuration,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _userMapper = new UserMapper();
        }

        // ===== Queries =====

        public async Task<ServiceResponse<PagedResultDto<UserResponseDTO>>> GetAllUsersAsync(PaginationParametersDto parametersDto, bool includeDeleted = false)
        {
            try
            {
                _logger.LogInformation("Getting all users");

                var (users, totalCount) = await _userRepository.GetAllUsersAsync(
                    parametersDto.PageNumber,
                    parametersDto.PageSize,
                    includeDeleted);

                var userDtos = _userMapper.MapToUserResponseDtos(users);

                var response = new PagedResultDto<UserResponseDTO>
                {
                    Items = userDtos,
                    TotalCount = totalCount,
                    PageNumber = parametersDto.PageNumber,
                    PageSize = parametersDto.PageSize
                };

                return new ServiceResponse<PagedResultDto<UserResponseDTO>>
                {
                    Success = true,
                    Data = response,
                    Message = "Users retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                return new ServiceResponse<PagedResultDto<UserResponseDTO>>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving users: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<UserResponseDTO>> GetUserByIdAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return new ServiceResponse<UserResponseDTO>
                {
                    Success = false,
                    Message = "User ID cannot be null or empty."
                };
            }

            try
            {
                _logger.LogInformation("Getting user with ID: {UserId}", userId);
                var user = await _userRepository.GetUserByIdAsync(userId);

                if (user == null)
                {
                    return new ServiceResponse<UserResponseDTO>
                    {
                        Success = false,
                        Message = $"User with ID '{userId}' not found."
                    };
                }

                var response = _userMapper.MapToUserResponseDto(user);

                return new ServiceResponse<UserResponseDTO>
                {
                    Success = true,
                    Data = response,
                    Message = "User retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user with ID: {UserId}", userId);
                return new ServiceResponse<UserResponseDTO>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving the user: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<PagedResultDto<UserResponseDTO>>> GetUsersInRoleAsync(PaginationParametersDto parametersDto, string role)
        {
            try
            {
                var (users, totalCount) = await _userRepository.GetUsersInRoleAsync(
                    parametersDto.PageNumber,
                    parametersDto.PageSize,
                    role);

                var userDtos = _userMapper.MapToUserResponseDtos(users);

                var response = new PagedResultDto<UserResponseDTO>
                {
                    Items = userDtos,
                    TotalCount = totalCount,
                    PageNumber = parametersDto.PageNumber,
                    PageSize = parametersDto.PageSize
                };

                return new ServiceResponse<PagedResultDto<UserResponseDTO>>
                {
                    Success = true,
                    Data = response,
                    Message = $"Users in role '{role}' retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users in role: {Role}", role);
                return new ServiceResponse<PagedResultDto<UserResponseDTO>>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving users in role '{role}': {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<UserResponseDTO>> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return new ServiceResponse<UserResponseDTO>
                {
                    Success = false,
                    Message = "Email cannot be null or empty."
                };
            }

            try
            {
                _logger.LogInformation("Getting user with email: {Email}", email);
                var user = await _userRepository.GetUserByEmailAsync(email);

                if (user == null)
                {
                    return new ServiceResponse<UserResponseDTO>
                    {
                        Success = false,
                        Message = $"User with email '{email}' not found."
                    };
                }

                var response = _userMapper.MapToUserResponseDto(user);

                return new ServiceResponse<UserResponseDTO>
                {
                    Success = true,
                    Data = response,
                    Message = "User retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user with email: {Email}", email);
                return new ServiceResponse<UserResponseDTO>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving the user: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<string?>> LoginAsync(string usernameOrEmail, string password)
        {
            try
            {
                var user = await _userRepository.FindByNameOrEmailAsync(usernameOrEmail);

                if (user == null || user.IsDeleted)
                {
                    return new ServiceResponse<string?>
                    {
                        Success = false,
                        Message = "Invalid credentials."
                    };
                }

                if (!user.EmailConfirmed)
                {
                    return new ServiceResponse<string?>
                    {
                        Success = false,
                        Message = "Please confirm your email before logging in."
                    };
                }

                var passwordValid = await _userRepository.CheckPasswordAsync(user, password);
                if (!passwordValid)
                {
                    await _userRepository.AccessFailedAsync(user);
                    return new ServiceResponse<string?>
                    {
                        Success = false,
                        Message = "Invalid credentials."
                    };
                }

                await _userRepository.ResetAccessFailedCountAsync(user);

                var userDto = _userMapper.MapToUserResponseDto(user);
                var token = await GenerateJwtTokenAsync(userDto);

                return new ServiceResponse<string?>
                {
                    Success = true,
                    Data = token,
                    Message = "Login successful."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for: {UsernameOrEmail}", usernameOrEmail);
                return new ServiceResponse<string?>
                {
                    Success = false,
                    Message = $"An error occurred during login: {ex.Message}"
                };
            }
        }

        public async Task<string> GenerateJwtTokenAsync(UserResponseDTO userDto)
        {
            var user = _userMapper.MapToUser(userDto);
            return await GenerateJwtTokenInternalAsync(user);
        }

        private async Task<string> GenerateJwtTokenInternalAsync(User user)
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

        public async Task<bool> IsExternalLoginLinkedAsync(string providerKey, string loginProvider, UserResponseDTO userDto)
        {
            var user = _userMapper.MapToUser(userDto);
            var userLogins = await _userRepository.GetLoginsAsync(user);
            var existingLogin = userLogins.FirstOrDefault(l =>
                l.LoginProvider == loginProvider && l.ProviderKey == providerKey);
            return existingLogin != null;
        }

        // ===== CRUD =====

        public async Task<ServiceResponse<IdentityResult>> CreateUserAsync(CreateUserDTO createUserDTO)
        {
            if (createUserDTO == null)
            {
                return new ServiceResponse<IdentityResult>
                {
                    Success = false,
                    Message = "User data cannot be null."
                };
            }

            try
            {
                _logger.LogInformation("Creating new user with email: {Email}", createUserDTO.Email);

                var user = _userMapper.MapToUser(createUserDTO);
                var result = await _userRepository.CreateUserAsync(user, createUserDTO.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created successfully with ID: {UserId}", user.Id);
                    return new ServiceResponse<IdentityResult>
                    {
                        Success = true,
                        Data = result,
                        Message = $"User created successfully with ID: {user.Id}"
                    };
                }

                _logger.LogWarning("User creation failed. Errors: {Errors}", string.Join(", ", result.Errors));
                return new ServiceResponse<IdentityResult>
                {
                    Success = false,
                    Data = result,
                    Message = "User creation failed."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user with email: {Email}", createUserDTO.Email);
                return new ServiceResponse<IdentityResult>
                {
                    Success = false,
                    Message = $"An error occurred while creating the user: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<IdentityResult>> UpdateUserAsync(UpdateUserDTO updateUserDTO)
        {
            if (updateUserDTO == null || string.IsNullOrWhiteSpace(updateUserDTO.Id))
            {
                return new ServiceResponse<IdentityResult>
                {
                    Success = false,
                    Message = "User data cannot be null."
                };
            }

            try
            {
                _logger.LogInformation("Updating user with ID: {UserId}", updateUserDTO.Id);

                var user = await _userRepository.GetUserByIdAsync(updateUserDTO.Id);
                if (user == null)
                {
                    return new ServiceResponse<IdentityResult>
                    {
                        Success = false,
                        Message = $"User with ID '{updateUserDTO.Id}' not found."
                    };
                }

                // Use mapper to update user
                user = _userMapper.MapToUser(updateUserDTO, user);

                var result = await _userRepository.UpdateUserAsync(user);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User updated successfully with ID: {UserId}", user.Id);
                    return new ServiceResponse<IdentityResult>
                    {
                        Success = true,
                        Data = result,
                        Message = "User updated successfully."
                    };
                }

                _logger.LogWarning("User update failed for ID: {UserId}. Errors: {Errors}", user.Id, string.Join(", ", result.Errors));
                return new ServiceResponse<IdentityResult>
                {
                    Success = false,
                    Data = result,
                    Message = "User update failed."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user with ID: {UserId}", updateUserDTO.Id);
                return new ServiceResponse<IdentityResult>
                {
                    Success = false,
                    Message = $"An error occurred while updating the user: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<IdentityResult>> DeleteUserAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return new ServiceResponse<IdentityResult>
                {
                    Success = false,
                    Message = "User ID cannot be null or empty."
                };
            }

            try
            {
                _logger.LogInformation("Deleting user with ID: {UserId}", userId);

                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return new ServiceResponse<IdentityResult>
                    {
                        Success = false,
                        Message = $"User with ID '{userId}' not found."
                    };
                }

                var result = await _userRepository.DeleteUserAsync(user);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User deleted successfully with ID: {UserId}", userId);
                    return new ServiceResponse<IdentityResult>
                    {
                        Success = true,
                        Data = result,
                        Message = "User deleted successfully."
                    };
                }

                return new ServiceResponse<IdentityResult>
                {
                    Success = false,
                    Data = result,
                    Message = "User deletion failed."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with ID: {UserId}", userId);
                return new ServiceResponse<IdentityResult>
                {
                    Success = false,
                    Message = $"An error occurred while deleting the user: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<IdentityResult>> RestoreUserAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return new ServiceResponse<IdentityResult>
                {
                    Success = false,
                    Message = "User ID cannot be null or empty."
                };
            }

            try
            {
                _logger.LogInformation("Restoring user with ID: {UserId}", id);

                var result = await _userRepository.RestoreUserAsync(id);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User restored successfully with ID: {UserId}", id);
                    return new ServiceResponse<IdentityResult>
                    {
                        Success = true,
                        Data = result,
                        Message = "User restored successfully."
                    };
                }

                return new ServiceResponse<IdentityResult>
                {
                    Success = false,
                    Data = result,
                    Message = "User restoration failed."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring user with ID: {UserId}", id);
                return new ServiceResponse<IdentityResult>
                {
                    Success = false,
                    Message = $"An error occurred while restoring the user: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<IdentityResult>> CreateExternalUserAsync(string email)
        {
            try
            {
                var result = await _userRepository.CreateExternalUserAsync(email);
                return new ServiceResponse<IdentityResult>
                {
                    Success = result.Succeeded,
                    Data = result,
                    Message = result.Succeeded ? "External user created successfully." : "Failed to create external user."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating external user with email: {Email}", email);
                return new ServiceResponse<IdentityResult>
                {
                    Success = false,
                    Message = $"An error occurred while creating the external user: {ex.Message}"
                };
            }
        }

        public async Task AddLoginAsync(UserResponseDTO userDto, UserLoginInfo loginInfo)
        {
            var user = _userMapper.MapToUser(userDto);
            await _userRepository.AddLoginAsync(user, loginInfo);
        }

        // ===== Password =====

        public async Task<ServiceResponse<bool>> ValidatePasswordAsync(string userId, string password)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = "User ID cannot be null or empty."
                };
            }

            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"User with ID '{userId}' not found."
                    };
                }

                var isValid = await _userRepository.CheckPasswordAsync(user, password);

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = isValid,
                    Message = isValid ? "Password is valid." : "Password is invalid."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating password for user: {UserId}", userId);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while validating the password: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<bool>> CheckPasswordAsync(string userId, string password)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = "User ID cannot be null or empty."
                };
            }

            try
            {
                var isValid = await _userRepository.CheckPasswordAsync(userId, password);

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = isValid,
                    Message = isValid ? "Password is valid." : "Password is invalid."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking password for user: {UserId}", userId);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while checking the password: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<IdentityResult>> ChangeUserPasswordAsync(string userId, ChangePasswordDTO changePasswordDTO)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return new ServiceResponse<IdentityResult>
                {
                    Success = false,
                    Message = "User ID cannot be null or empty."
                };
            }

            if (changePasswordDTO == null)
            {
                return new ServiceResponse<IdentityResult>
                {
                    Success = false,
                    Message = "Password data cannot be null."
                };
            }

            try
            {
                _logger.LogInformation("Changing password for user: {UserId}", userId);

                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return new ServiceResponse<IdentityResult>
                    {
                        Success = false,
                        Message = $"User with ID '{userId}' not found."
                    };
                }

                var result = await _userRepository.ChangePasswordAsync(user, changePasswordDTO.CurrentPassword, changePasswordDTO.NewPassword);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Password changed successfully for user: {UserId}", userId);
                    return new ServiceResponse<IdentityResult>
                    {
                        Success = true,
                        Data = result,
                        Message = "Password changed successfully."
                    };
                }

                _logger.LogWarning("Password change failed for user: {UserId}. Errors: {Errors}", userId, string.Join(", ", result.Errors));
                return new ServiceResponse<IdentityResult>
                {
                    Success = false,
                    Data = result,
                    Message = "Password change failed."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
                return new ServiceResponse<IdentityResult>
                {
                    Success = false,
                    Message = $"An error occurred while changing the password: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<IdentityResult>> ResetPasswordAsync(string userId, string token, string newPassword)
        {
            try
            {
                var result = await _userRepository.ResetPasswordAsync(userId, token, newPassword);

                return new ServiceResponse<IdentityResult>
                {
                    Success = result.Succeeded,
                    Data = result,
                    Message = result.Succeeded ? "Password reset successfully." : "Password reset failed."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for user: {UserId}", userId);
                return new ServiceResponse<IdentityResult>
                {
                    Success = false,
                    Message = $"An error occurred while resetting the password: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<string>> GeneratePasswordResetTokenAsync(string userId)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return new ServiceResponse<string>
                    {
                        Success = false,
                        Message = $"User with ID '{userId}' not found."
                    };
                }

                var token = await _userRepository.GeneratePasswordResetTokenAsync(user);

                return new ServiceResponse<string>
                {
                    Success = true,
                    Data = token,
                    Message = "Password reset token generated successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating password reset token for user: {UserId}", userId);
                return new ServiceResponse<string>
                {
                    Success = false,
                    Message = $"An error occurred while generating the password reset token: {ex.Message}"
                };
            }
        }

        // ===== Email =====

        public async Task<ServiceResponse<string>> GenerateEmailConfirmationTokenAsync(string email)
        {
            try
            {
                var user = await _userRepository.GetUserByEmailAsync(email);
                if (user == null)
                {
                    return new ServiceResponse<string>
                    {
                        Success = false,
                        Message = $"User with email '{email}' not found."
                    };
                }

                var token = await _userRepository.GenerateEmailConfirmationTokenAsync(user);

                return new ServiceResponse<string>
                {
                    Success = true,
                    Data = token,
                    Message = "Email confirmation token generated successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating email confirmation token for email: {Email}", email);
                return new ServiceResponse<string>
                {
                    Success = false,
                    Message = $"An error occurred while generating the email confirmation token: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<IdentityResult>> ConfirmEmailAsync(string email, string token)
        {
            try
            {
                var result = await _userRepository.ConfirmEmailAsync(email, token);

                return new ServiceResponse<IdentityResult>
                {
                    Success = result.Succeeded,
                    Data = result,
                    Message = result.Succeeded ? "Email confirmed successfully." : "Email confirmation failed."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming email: {Email}", email);
                return new ServiceResponse<IdentityResult>
                {
                    Success = false,
                    Message = $"An error occurred while confirming the email: {ex.Message}"
                };
            }
        }

        // ===== Roles =====

        public async Task<ServiceResponse<IList<string>>> GetUserRolesAsync(string userId)
        {
            try
            {
                var roles = await _userRepository.GetUserRolesAsync(userId);

                return new ServiceResponse<IList<string>>
                {
                    Success = true,
                    Data = roles,
                    Message = "User roles retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles for user: {UserId}", userId);
                return new ServiceResponse<IList<string>>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving user roles: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<bool>> IsInRoleAsync(string userId, string role)
        {
            try
            {
                var isInRole = await _userRepository.IsInRoleAsync(userId, role);

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = isInRole,
                    Message = isInRole ? "User is in role." : "User is not in role."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user is in role: {UserId}, {Role}", userId, role);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while checking user role: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<IdentityResult>> AddToRoleAsync(string userId, string role)
        {
            try
            {
                var result = await _userRepository.AddToRoleAsync(userId, role);

                return new ServiceResponse<IdentityResult>
                {
                    Success = result.Succeeded,
                    Data = result,
                    Message = result.Succeeded ? "Role added successfully." : "Failed to add role."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding role to user: {UserId}, {Role}", userId, role);
                return new ServiceResponse<IdentityResult>
                {
                    Success = false,
                    Message = $"An error occurred while adding the role: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<IdentityResult>> RemoveFromRoleAsync(string userId, string role)
        {
            try
            {
                var result = await _userRepository.RemoveFromRoleAsync(userId, role);

                return new ServiceResponse<IdentityResult>
                {
                    Success = result.Succeeded,
                    Data = result,
                    Message = result.Succeeded ? "Role removed successfully." : "Failed to remove role."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing role from user: {UserId}, {Role}", userId, role);
                return new ServiceResponse<IdentityResult>
                {
                    Success = false,
                    Message = $"An error occurred while removing the role: {ex.Message}"
                };
            }
        }

        // ===== Utility =====

        public async Task<ServiceResponse<bool>> UserExistsAsync(string userId)
        {
            try
            {
                var exists = await _userRepository.UserExistsAsync(userId);

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = exists,
                    Message = exists ? "User exists." : "User does not exist."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user exists: {UserId}", userId);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while checking if user exists: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<bool>> IsUserDeletedAsync(string userId)
        {
            try
            {
                var isDeleted = await _userRepository.IsUserDeletedAsync(userId);

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = isDeleted,
                    Message = isDeleted ? "User is deleted." : "User is not deleted."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user is deleted: {UserId}", userId);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while checking if user is deleted: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<PagedResultDto<OrganizationResponseDto>>> GetOrganizationsUserManagesAsync(
            PaginationParametersDto parametersDto,
            string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return new ServiceResponse<PagedResultDto<OrganizationResponseDto>>
                {
                    Success = false,
                    Message = "User ID cannot be null or empty."
                };
            }

            try
            {
                _logger.LogInformation("Getting organizations managed by user: {UserId}", userId);

                var (organizations, totalCount) = await _userRepository.GetOrganizationsUserManagesAsync(
                    parametersDto.PageNumber,
                    parametersDto.PageSize,
                    userId);

                // Map to DTO using the user mapper (or create a separate organization mapper)
                var organizationDtos = organizations.Select(o => new OrganizationResponseDto
                {
                    Id = o.Id,
                    Name = o.Name,
                    Address = o.Address,
                    IsDeleted = o.IsDeleted,
                    RegistrationDate = o.RegistrationDate,
                    UpdatedOn = o.UpdatedOn,
                    PaymentId = o.PaymentId,
                    AdminUserId = o.AdminUserId
                }).ToList();

                var response = new PagedResultDto<OrganizationResponseDto>
                {
                    Items = organizationDtos,
                    TotalCount = totalCount,
                    PageNumber = parametersDto.PageNumber,
                    PageSize = parametersDto.PageSize
                };

                return new ServiceResponse<PagedResultDto<OrganizationResponseDto>>
                {
                    Success = true,
                    Data = response,
                    Message = "Organizations managed by user retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting organizations managed by user: {UserId}", userId);
                return new ServiceResponse<PagedResultDto<OrganizationResponseDto>>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving organizations: {ex.Message}"
                };
            }
        }
    }
}