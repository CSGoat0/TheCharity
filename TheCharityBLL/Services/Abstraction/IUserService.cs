using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheCharityBLL.DTOs.PaginationDTOs;
using TheCharityBLL.DTOs.UserDTOs;
using TheCharityDAL.Entities;
using TheCharityDAL.Enums;

namespace TheCharityBLL.Services.Abstraction
{
    public interface IUserService
    {
        // Queries
        Task<IEnumerable<UserResponseDTO>> GetAllUsersAsync();
        Task<PagedResultDto<UserResponseDTO>> GetAllUsersAsync(PaginationParametersDto paginationDto);
        Task<UserResponseDTO?> GetUserByIdAsync(string userId);
        Task<UserResponseDTO?> GetUserByEmailAsync(string email);
        Task<bool> UserExistsAsync(string userId);
        Task<bool> IsUserDeletedAsync(string userId);
        Task<string?> LoginAsync(string usernameOrEmail, string password);
        public  Task<bool> IsExternalLoginLinkedAsync(string providerKey, string loginProvider, UserResponseDTO userDto);
        public Task<string> GenerateJwtTokenAsync(UserResponseDTO UserDTO);
        // CRUD
        Task<IdentityResult> CreateUserAsync(CreateUserDTO createUserDTO);
        Task<IdentityResult> UpdateUserAsync(UpdateUserDTO updateUserDTO);
        Task<IdentityResult> DeleteUserAsync(string userId);
        Task<IdentityResult> RestoreUserAsync(string id);
        Task<IdentityResult> CreateExternalUserAsync(string email);
        public Task AddLoginAsync(UserResponseDTO UserDTO , UserLoginInfo loginInfo);
        // Password
        Task<bool> ValidatePasswordAsync(string userId, string password);
        Task<bool> CheckPasswordAsync(string userId, string password);
        Task<IdentityResult> ChangeUserPasswordAsync(string userId, ChangePasswordDTO changePasswordDTO);
        Task<IdentityResult> ResetPasswordAsync(string userId, string token, string newPassword);
        Task<string> GeneratePasswordResetTokenAsync(string userId);

        // Email
        Task<IdentityResult> ConfirmEmailAsync(string email, string token);
        Task<string> GenerateEmailConfirmationTokenAsync(string email);

        // Roles
        Task<IList<string>> GetUserRolesAsync(string userId);
        Task<bool> IsInRoleAsync(string userId, string role);
        Task<IdentityResult> AddToRoleAsync(string userId, string role);
        Task<IdentityResult> RemoveFromRoleAsync(string userId, string role);
    }
}
