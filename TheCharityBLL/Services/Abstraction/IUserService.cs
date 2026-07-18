using Microsoft.AspNetCore.Identity;
using TheCharityBLL.DTOs;
using TheCharityBLL.DTOs.OrganizationDTOs;
using TheCharityBLL.DTOs.PaginationDTOs;
using TheCharityBLL.DTOs.UserDTOs;
using TheCharityBLL.DTOs.UserResponseDTOs;

namespace TheCharityBLL.Services.Abstraction
{
    public interface IUserService
    {
        // ===== Queries =====
        Task<ServiceResponse<PagedResultDto<UserResponseDTO>>> GetAllUsersAsync(PaginationParametersDto parametersDto, bool includeDeleted = false);
        Task<ServiceResponse<UserResponseDTO>> GetUserByIdAsync(string userId);
        Task<ServiceResponse<PagedResultDto<UserResponseDTO>>> GetUsersInRoleAsync(PaginationParametersDto parametersDto, string role);
        Task<ServiceResponse<UserResponseDTO>> GetUserByEmailAsync(string email);
        Task<ServiceResponse<bool>> UserExistsAsync(string userId);
        Task<ServiceResponse<bool>> IsUserDeletedAsync(string userId);
        Task<ServiceResponse<string?>> LoginAsync(string usernameOrEmail, string password);
        Task<bool> IsExternalLoginLinkedAsync(string providerKey, string loginProvider, UserResponseDTO userDto);
        Task<string> GenerateJwtTokenAsync(UserResponseDTO UserDTO);

        // ===== CRUD =====
        Task<ServiceResponse<IdentityResult>> CreateUserAsync(CreateUserDTO createUserDTO);
        Task<ServiceResponse<IdentityResult>> UpdateUserAsync(UpdateUserDTO updateUserDTO);
        Task<ServiceResponse<IdentityResult>> DeleteUserAsync(string userId);
        Task<ServiceResponse<IdentityResult>> RestoreUserAsync(string id);
        Task<ServiceResponse<IdentityResult>> CreateExternalUserAsync(string email);
        Task AddLoginAsync(UserResponseDTO UserDTO, UserLoginInfo loginInfo);

        // ===== Password =====
        Task<ServiceResponse<bool>> ValidatePasswordAsync(string userId, string password);
        Task<ServiceResponse<bool>> CheckPasswordAsync(string userId, string password);
        Task<ServiceResponse<IdentityResult>> ChangeUserPasswordAsync(string userId, ChangePasswordDTO changePasswordDTO);
        Task<ServiceResponse<IdentityResult>> ResetPasswordAsync(string userId, string token, string newPassword);
        Task<ServiceResponse<string>> GeneratePasswordResetTokenAsync(string userId);

        // ===== Email =====
        Task<ServiceResponse<IdentityResult>> ConfirmEmailAsync(string email, string token);
        Task<ServiceResponse<string>> GenerateEmailConfirmationTokenAsync(string email);

        // ===== Roles =====
        Task<ServiceResponse<IList<string>>> GetUserRolesAsync(string userId);
        Task<ServiceResponse<bool>> IsInRoleAsync(string userId, string role);
        Task<ServiceResponse<IdentityResult>> AddToRoleAsync(string userId, string role);
        Task<ServiceResponse<IdentityResult>> RemoveFromRoleAsync(string userId, string role);

        // ===== Organization Management Queries =====
        Task<ServiceResponse<PagedResultDto<OrganizationResponseDto>>> GetOrganizationsUserManagesAsync(PaginationParametersDto parametersDto, string userId);
    }
}