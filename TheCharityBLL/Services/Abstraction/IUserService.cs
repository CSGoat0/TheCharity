using Microsoft.AspNetCore.Identity;
using TheCharityBLL.DTOs;
using TheCharityBLL.DTOs.UserDTOs;
using TheCharityDAL.Entities;


namespace TheCharityBLL.Services.Abstraction
{
    public interface IUserService
    {
        // Queries
        Task<IEnumerable<UserResponseDTO>> GetAllUsersAsync();
        Task<UserResponseDTO?> GetUserByIdAsync(string userId);
        Task<UserResponseDTO?> GetUserByEmailAsync(string email);
        Task<ServiceResponse<bool>> UserExistsAsync(string userId);
        Task<ServiceResponse<bool>> IsUserDeletedAsync(string userId);
        Task<ServiceResponse<string?>> LoginAsync(string usernameOrEmail, string password);
        public  Task<bool> IsExternalLoginLinkedAsync(string providerKey, string loginProvider, UserResponseDTO userDto);
        public Task<string> GenerateJwtTokenAsync(UserResponseDTO UserDTO);
        // CRUD
        Task<ServiceResponse<IdentityResult>> CreateUserAsync(CreateUserDTO createUserDTO);
        Task<IdentityResult> UpdateUserAsync(UpdateUserDTO updateUserDTO);
        Task<IdentityResult> DeleteUserAsync(string userId);
        Task<IdentityResult> RestoreUserAsync(string id);
        Task<IdentityResult> CreateExternalUserAsync(string email);
        public Task AddLoginAsync(UserResponseDTO UserDTO , UserLoginInfo loginInfo);
        // Password
        Task<ServiceResponse<bool>> ValidatePasswordAsync(string userId, string password);
        Task<ServiceResponse<bool>> CheckPasswordAsync(string userId, string password);
        Task<IdentityResult> ChangeUserPasswordAsync(string userId, ChangePasswordDTO changePasswordDTO);
        Task<IdentityResult> ResetPasswordAsync(string userId, string token, string newPassword);
        Task<string> GeneratePasswordResetTokenAsync(string userId);

        // Email
        Task<IdentityResult> ConfirmEmailAsync(string email, string token);
        Task<string> GenerateEmailConfirmationTokenAsync(string email);

        // Roles
        Task<ServiceResponse<IList<string>>> GetUserRolesAsync(string userId);
        Task<ServiceResponse<bool>> IsInRoleAsync(string userId, string role);
        Task<ServiceResponse<IdentityResult>> AddToRoleAsync(string userId, string role);
        Task<ServiceResponse<IdentityResult>> RemoveFromRoleAsync(string userId, string role);

        // Organization Management Queries
        Task<IEnumerable<Organization>> GetOrganizationsUserManagesAsync(string userId);
    }
}
