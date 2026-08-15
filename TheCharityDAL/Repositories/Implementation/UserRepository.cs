using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TheCharityDAL.Database;
using TheCharityDAL.Entities;
using TheCharityDAL.Enums;
using TheCharityDAL.Extensions;
using TheCharityDAL.Repositories.Abstraction;

namespace TheCharityDAL.Repositories.Implementation
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly TheCharityDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserRepository(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            TheCharityDbContext context)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _context = context;
        }

        // ===== CRUD =====

        public async Task<IdentityResult> CreateExternalUserAsync(string email)
        {
            var user = new User
            {
                Email = email,
                UserName = email,
                EmailConfirmed = true
            };

            return await _userManager.CreateAsync(user);
        }

        public async Task<IdentityResult> AddToRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });
            }

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

        public async Task<(IEnumerable<User> Data, int TotalCount)> GetAllUsersAsync(int pageNumber, int pageSize, bool includeDeleted = false)
        {
            var query = _userManager.Users.AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(u => !u.IsDeleted);
            }

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<User?> GetUserByIdAsync(string id)
        {
            return await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _userManager.Users
                .FirstOrDefaultAsync(u => u.UserName == username && !u.IsDeleted);
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
            return await _userManager.Users.AnyAsync(u => u.Id == userId && !u.IsDeleted);
        }

        // ===== Email =====

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
            return await _userManager.Users.FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
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

        // ===== Lockout =====

        public async Task AccessFailedAsync(User user)
            => await _userManager.AccessFailedAsync(user);

        public async Task<IList<UserLoginInfo>> GetLoginsAsync(User user)
        {
            return await _userManager.GetLoginsAsync(user);
        }

        public async Task AddLoginAsync(User user, UserLoginInfo loginInfo)
        {
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

        // ===== SuperAdmin Queries =====

        public async Task<bool> IsSuperAdminAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            return await _userManager.IsInRoleAsync(user, "SuperAdmin");
        }

        public async Task<(IEnumerable<User> Data, int TotalCount)> GetUsersInRoleAsync(
            int pageNumber,
            int pageSize,
            string role)
        {
            // Get the role ID first
            var roleId = await _context.Roles
                .Where(r => r.Name == role)
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(roleId))
                return (Enumerable.Empty<User>(), 0);

            // Single query using Join
            var query = _context.UserRoles
                .Where(ur => ur.RoleId == roleId)
                .Join(_context.Users,
                      ur => ur.UserId,
                      u => u.Id,
                      (ur, u) => u)
                .Where(u => !u.IsDeleted)
                .AsQueryable();

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        // ===== Organization Role Checks =====

        public async Task<bool> IsOrganizationAdminAsync(string userId, int organizationId)
        {
            return await _context.OrganizationRoles
                .AnyAsync(r => r.OrganizationId == organizationId &&
                              r.UserId == userId &&
                              r.Role == OrganizationRoleType.Admin &&
                              !r.IsDeleted);
        }

        public async Task<bool> IsOrganizationSubAdminAsync(string userId, int organizationId)
        {
            return await _context.OrganizationRoles
                .AnyAsync(r => r.OrganizationId == organizationId &&
                              r.UserId == userId &&
                              r.Role == OrganizationRoleType.SubAdmin &&
                              !r.IsDeleted);
        }

        public async Task<bool> IsOrganizationAdminOrSubAdminAsync(string userId, int organizationId)
        {
            return await IsOrganizationAdminAsync(userId, organizationId) ||
                   await IsOrganizationSubAdminAsync(userId, organizationId);
        }

        public async Task<OrganizationRoleType?> GetUserRoleInOrganizationAsync(string userId, int organizationId)
        {
            var role = await _context.OrganizationRoles
                .Where(r => r.OrganizationId == organizationId &&
                           r.UserId == userId &&
                           !r.IsDeleted)
                .FirstOrDefaultAsync();

            return role?.Role;
        }

        // ===== Organization Management Queries =====

        public async Task<(IEnumerable<Organization> Data, int TotalCount)> GetOrganizationsUserManagesAsync(
            int pageNumber,
            int pageSize,
            string userId)
        {
            // Get organization IDs where user is Admin
            var adminOrgIds = await _context.Organizations
                .Where(o => o.AdminUserId == userId && !o.IsDeleted)
                .Select(o => o.Id)
                .ToListAsync();

            // Get organization IDs where user is SubAdmin
            var subAdminOrgIds = await _context.OrganizationRoles
                .Where(r => r.UserId == userId &&
                           r.Role == OrganizationRoleType.SubAdmin &&
                           !r.IsDeleted)
                .Select(r => r.OrganizationId)
                .ToListAsync();

            // Combine all organization IDs
            var allOrgIds = adminOrgIds.Concat(subAdminOrgIds).Distinct().ToList();

            if (!allOrgIds.Any())
                return (Enumerable.Empty<Organization>(), 0);

            // Query organizations with pagination
            var query = _context.Organizations
                .Where(o => allOrgIds.Contains(o.Id) && !o.IsDeleted)
                .Include(o => o.ContactMethods.Where(cm => !cm.IsDeleted))
                .Include(o => o.PaymentInfo)
                .AsQueryable();

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<(IEnumerable<Organization> Data, int TotalCount)> GetOrganizationsUserIsSubAdminOfAsync(
            int pageNumber,
            int pageSize,
            string userId)
        {
            var subAdminOrgIds = await _context.OrganizationRoles
                .Where(r => r.UserId == userId &&
                           r.Role == OrganizationRoleType.SubAdmin &&
                           !r.IsDeleted)
                .Select(r => r.OrganizationId)
                .ToListAsync();

            if (!subAdminOrgIds.Any())
                return (Enumerable.Empty<Organization>(), 0);

            var query = _context.Organizations
                .Where(o => subAdminOrgIds.Contains(o.Id) && !o.IsDeleted)
                .Include(o => o.ContactMethods.Where(cm => !cm.IsDeleted))
                .Include(o => o.PaymentInfo)
                .AsQueryable();

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<(IEnumerable<Organization> Data, int TotalCount)> GetAllOrganizationsUserHasAccessToAsync(
            int pageNumber,
            int pageSize,
            string userId)
        {
            // Get organization IDs where user is Admin
            var adminOrgIds = await _context.Organizations
                .Where(o => o.AdminUserId == userId && !o.IsDeleted)
                .Select(o => o.Id)
                .ToListAsync();

            // Get organization IDs where user is SubAdmin
            var subAdminOrgIds = await _context.OrganizationRoles
                .Where(r => r.UserId == userId &&
                           r.Role == OrganizationRoleType.SubAdmin &&
                           !r.IsDeleted)
                .Select(r => r.OrganizationId)
                .ToListAsync();

            // Get organization IDs where user has donated
            var donatedOrgIds = await _context.Donations
                .Where(d => d.UserId == userId &&
                           d.Campaign != null &&
                           d.Campaign.OrganizationId.HasValue &&
                           !d.IsDeleted)
                .Select(d => d.Campaign.OrganizationId.Value)
                .Distinct()
                .ToListAsync();

            // Combine all organization IDs
            var allOrgIds = adminOrgIds.Concat(subAdminOrgIds).Concat(donatedOrgIds).Distinct().ToList();

            if (!allOrgIds.Any())
                return (Enumerable.Empty<Organization>(), 0);

            var query = _context.Organizations
                .Where(o => allOrgIds.Contains(o.Id) && !o.IsDeleted)
                .Include(o => o.ContactMethods.Where(cm => !cm.IsDeleted))
                .Include(o => o.PaymentInfo)
                .AsQueryable();

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<(IEnumerable<OrganizationRole> Data, int TotalCount)> GetUserOrganizationRolesAsync(
            int pageNumber,
            int pageSize,
            string userId)
        {
            var query = _context.OrganizationRoles
                .Where(r => r.UserId == userId && !r.IsDeleted)
                .Include(r => r.Organization)
                .AsQueryable();

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<bool> UserHasAnyManagementRoleAsync(string userId)
        {
            if (await IsSuperAdminAsync(userId))
                return true;

            var isAdmin = await _context.Organizations
                .AnyAsync(o => o.AdminUserId == userId && !o.IsDeleted);

            var isSubAdmin = await _context.OrganizationRoles
                .AnyAsync(r => r.UserId == userId &&
                              r.Role == OrganizationRoleType.SubAdmin &&
                              !r.IsDeleted);

            return isAdmin || isSubAdmin;
        }
    }
}