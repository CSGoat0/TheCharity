using Microsoft.AspNetCore.Identity;
using TheCharityDAL.Entities;
using TheCharityDAL.Enums;

namespace TheCharityDAL.Repositories.Abstraction
{
    public interface IUserRepository
    {
        // ===== CRUD =====
        Task<IdentityResult> CreateUserAsync(User user, string password);
        Task<IdentityResult> UpdateUserAsync(User user);
        Task<IdentityResult> DeleteUserAsync(User user);
        Task<IdentityResult> RestoreUserAsync(string id);
        public Task AddLoginAsync(User user, UserLoginInfo loginInfo);
        Task<IdentityResult> CreateExternalUserAsync(string email);
        // ===== Lookup =====
        Task<User?> GetUserByIdAsync(string id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> FindByNameOrEmailAsync(string usernameOrEmail);
        Task<IEnumerable<User>?> GetAllUsersAsync();
        Task<bool> UserExistsAsync(string userId);
        Task<bool> IsUserDeletedAsync(string userId);
        public Task<IList<UserLoginInfo>> GetLoginsAsync(User user);

        // ===== Password =====
        Task<bool> CheckPasswordAsync(User user, string password);
        Task<bool> CheckPasswordAsync(string userId, string password);
        Task<IdentityResult> ChangePasswordAsync(User user, string currentPassword, string newPassword);
        Task<IdentityResult> ResetPasswordAsync(string userId, string token, string newPassword);
        Task<string> GeneratePasswordResetTokenAsync(User user);

        // ===== Email =====
        Task<IdentityResult> ConfirmEmailAsync(string email, string token);
        Task<string> GenerateEmailConfirmationTokenAsync(User user);

        // ===== Roles =====
        Task<IList<string>> GetUserRolesAsync(string userId);
        Task<bool> IsInRoleAsync(string userId, string role);
        Task<IdentityResult> AddToRoleAsync(string userId, string role);
        Task<IdentityResult> RemoveFromRoleAsync(string userId, string role);

        // ===== Lockout =====
        Task AccessFailedAsync(User user);
        Task ResetAccessFailedCountAsync(User user);

        // ===== SuperAdmin Check (Identity Role) =====
        Task<bool> IsSuperAdminAsync(string userId);

        // ===== Organization Role Queries (OrganizationRole Entity) =====
        Task<bool> IsOrganizationAdminAsync(string userId, int organizationId);
        Task<bool> IsOrganizationSubAdminAsync(string userId, int organizationId);
        Task<bool> IsOrganizationAdminOrSubAdminAsync(string userId, int organizationId);
        Task<OrganizationRoleType?> GetUserRoleInOrganizationAsync(string userId, int organizationId);

        // ===== Organization Management Queries =====
        Task<IEnumerable<Organization>> GetOrganizationsUserManagesAsync(string userId);
        Task<IEnumerable<Organization>> GetOrganizationsUserIsSubAdminOfAsync(string userId);
        Task<IEnumerable<Organization>> GetAllOrganizationsUserHasAccessToAsync(string userId);
        Task<bool> UserHasAnyManagementRoleAsync(string userId);
        Task<IEnumerable<OrganizationRole>> GetUserOrganizationRolesAsync(string userId);
    }
}