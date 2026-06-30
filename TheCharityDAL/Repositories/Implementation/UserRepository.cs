using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TheCharityDAL.Database;
using TheCharityDAL.Entities;
using TheCharityDAL.Repositories.Abstraction;

namespace TheCharityDAL.Repositories.Implementation
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<User> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;
        public UserRepository(
             UserManager<User> userManager,
             RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

        public async Task<IdentityResult> CreateExternalUserAsync(string email)
        {
            var user = new User
            {
                Email = email,
                UserName = email,
                EmailConfirmed = true // Google/Facebook already verified it
            };

            return await _userManager.CreateAsync(user); // ✅ no password
        }
        public async Task<IdentityResult> AddToRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });
            }

            // Create role if it doesn't exist
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }

            return await _userManager.AddToRoleAsync(user, role);
        }

        public async Task<IdentityResult> ChangePasswordAsync(User user, string currentPassword, string newPassword)
        {
            return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        }

        public async Task<bool> CheckPasswordAsync(User user, string password)
        {
            return await _userManager.CheckPasswordAsync(user, password);
        }

        public async Task<bool> CheckPasswordAsync(string userId, string password)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            return await _userManager.CheckPasswordAsync(user, password);
        }

        public async Task<IdentityResult> CreateUserAsync(User user, string password)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            return await _userManager.CreateAsync(user, password);
        }

        public async Task<IdentityResult> DeleteUserAsync(User user)
        {
            user.Delete();
            return await _userManager.UpdateAsync(user);
        }

        public async Task<IEnumerable<User>?> GetAllUsersAsync()
        {
            return await _userManager.Users
                .ToListAsync();
        }

        //pagination
        public async Task<(IEnumerable<User>? Data,int TotalCount)> GetAllUsersAsync(int pageNumber, int pageSize)
        {
            var users = await _userManager.Users
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalCount = await _userManager.Users.CountAsync();

            return (users, totalCount);
        }

        public async Task<User?> GetUserByIdAsync(string id)
        {
            return await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<IList<string>> GetUserRolesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new List<string>();
            }

            return await _userManager.GetRolesAsync(user);
        }

        public async Task<bool> IsInRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            return userRoles.Any(r => r.Equals(role, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<bool> IsUserDeletedAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user?.IsDeleted ?? true;
        }






        public async Task<IdentityResult> RemoveFromRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });
            }

            return await _userManager.RemoveFromRoleAsync(user, role);
        }

        public async Task<IdentityResult> RestoreUserAsync(string id)
        {
            var user = await GetUserByIdAsync(id);
            if (user != null)
            {
                user.Restore();
                await _userManager.UpdateAsync(user);
                return IdentityResult.Success;
            }
            return IdentityResult.Failed(new IdentityError { Description = "User not found" });
        }

        public async Task<IdentityResult> UpdateUserAsync(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            return await _userManager.UpdateAsync(user);
        }

        public async Task<bool> UserExistsAsync(string userId)
        {
            return await _userManager.Users.AnyAsync(u => u.Id == userId && u.IsDeleted == false);
        }


        public async Task<string> GenerateEmailConfirmationTokenAsync(User user)
        {
            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }
        public async Task<string> GeneratePasswordResetTokenAsync(User user)
        {
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _userManager.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsDeleted == false);
        }
        public async Task<IdentityResult> ResetPasswordAsync(string userId, string token, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });
            }
            return await _userManager.ResetPasswordAsync(user, token, newPassword);
        }

        public async Task<IdentityResult> ConfirmEmailAsync(string email, string token)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });
            }
            return await _userManager.ConfirmEmailAsync(user, token);
        }
       
        ////////////new addations
        public async Task AccessFailedAsync(User user)
           => await _userManager.AccessFailedAsync(user);
        public async Task<IList<UserLoginInfo>> GetLoginsAsync(User user)
        {
            return await _userManager.GetLoginsAsync(user);
        }
        public async Task AddLoginAsync(User user, UserLoginInfo loginInfo)
        {
            // Re-fetch as TRACKED instance fresh from UserManager
            // to avoid conflict with any previously tracked instance
            var trackedUser = await _userManager.FindByIdAsync(user.Id);

            if (trackedUser == null)
                throw new Exception("User not found");

            await _userManager.AddLoginAsync(trackedUser, loginInfo);
        }
        public async Task ResetAccessFailedCountAsync(User user)
            => await _userManager.ResetAccessFailedCountAsync(user);

        public async Task<User?> FindByNameOrEmailAsync(string usernameOrEmail)
        {
            return await _userManager.FindByNameAsync(usernameOrEmail)
                   ?? await _userManager.FindByEmailAsync(usernameOrEmail);
        }
    }
}
