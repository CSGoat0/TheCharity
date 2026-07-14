using Microsoft.EntityFrameworkCore;
using TheCharityDAL.Database;
using TheCharityDAL.Entities;
using TheCharityDAL.Enums;
using TheCharityDAL.Extensions;
using TheCharityDAL.Repositories.Abstraction;

namespace TheCharityDAL.Repositories.Implementation
{
    public class OrganizationRepository : IOrganizationRepository
    {
        private readonly TheCharityDbContext _context;

        public OrganizationRepository(TheCharityDbContext context)
        {
            _context = context;
        }

        // ===== Organization CRUD Operations =====
        public async Task<(IEnumerable<Organization> Data, int TotalCount)> GetAllOrganizationsAsync(int pageNumber, int pageSize, bool includeDeleted = false)
        {
            var query = _context.Organizations.Include(o => o.ContactMethods
                .Where(cm => cm.IsDeleted == false))
                .Include(o => o.PaymentInfo).AsQueryable();

            if (!includeDeleted)
                query = query.Where(o => o.IsDeleted == false);

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        //excluding campaigns, if you want campaigns try using GetOrganizationWithDetailsAsync()
        public async Task<Organization?> GetOrganizationByIdAsync(int id)
        {
            return await _context.Organizations
                .Where(o => o.Id == id && (o.IsDeleted == false))
                .Include(o => o.ContactMethods.Where(cm => cm.IsDeleted == false))
                .Include(o => o.PaymentInfo)
                .FirstOrDefaultAsync();
        }

        public async Task<Organization> AddOrganizationAsync(Organization organization)
        {
            _context.Organizations.Add(organization);
            await _context.SaveChangesAsync();
            return organization;
        }

        public async Task<Organization> UpdateOrganizationAsync(Organization organization)
        {
            _context.Entry(organization).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return organization;
        }

        public async Task DeleteOrganizationAsync(int id)
        {
            var organization = await GetOrganizationByIdAsync(id);
            if (organization != null)
            {
                organization.Delete();
                await _context.SaveChangesAsync();
            }
        }

        public async Task RestoreOrganizationAsync(int id)
        {
            var organization = await _context.Organizations
                .Where(o => o.Id == id && o.IsDeleted == true)
                .FirstOrDefaultAsync();

            if (organization != null)
            {
                organization.Restore();
                await _context.SaveChangesAsync();
            }
        }

        // ===== Organization Filtering & Search =====
        public async Task<Organization?> GetOrganizationByNameAsync(string name)
        {
            return await _context.Organizations
                .Where(o => o.Name == name && (o.IsDeleted == false))
                .FirstOrDefaultAsync();
        }
        public async Task<Organization?> GetOrganizationByPaymentInfoIdAsync(int PaymentInfoId)
        {
            return await _context.Organizations
               .Where(o => o.PaymentId == PaymentInfoId && (o.IsDeleted == false))
               .FirstOrDefaultAsync();
        }

        public async Task<(IEnumerable<Organization> Data, int TotalCount)> SearchOrganizationsAsync(int pageNumber, int pageSize, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllOrganizationsAsync(pageNumber, pageSize);

            var query = _context.Organizations
                .Where(o => (o.IsDeleted == false) &&
                           (o.Name != null && o.Name.Contains(searchTerm)) ||
                           (o.Address != null && o.Address.Contains(searchTerm))).AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<(IEnumerable<Organization> Data, int TotalCount)> GetDeletedOrganizationsAsync(int pageNumber, int pageSize)
        {
            var query = _context.Organizations
                .Where(o => o.IsDeleted == true).AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<(IEnumerable<Organization> Data, int TotalCount)> GetOrganizationsDropDownAsync(int pageNumber, int pageSize)
        {
            var query = _context.Organizations
                .Where(o => o.IsDeleted == false)
                .OrderBy(o => o.Name)
                .AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<(IEnumerable<Organization> Data, int TotalCount)> GetOrganizationsByAddressAsync(int pageNumber, int pageSize, string address)
        {
            var query = _context.Organizations
                .Where(o => o.Address != null && o.Address.Contains(address) &&
                           (o.IsDeleted == false)).AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        // ===== Organization Statistics =====
        public async Task<int> GetTotalOrganizationsCountAsync()
        {
            return await _context.Organizations
                .Where(o => o.IsDeleted == false)
                .CountAsync();
        }

        public async Task<int> GetActiveOrganizationsCountAsync()
        {
            return await _context.Organizations
                .Where(o => (o.IsDeleted == false) &&
                           (o.Campaigns != null && o.Campaigns.Any(c => c.Status == CampaignStatus.Active)))
                .CountAsync();
        }

        // ===== Organization Contact Methods =====
        public async Task<(IEnumerable<OrganizationContactMethod> Data, int TotalCount)> GetOrganizationContactMethodsAsync(int pageNumber, int pageSize, int organizationId)
        {
            var query = _context.OrganizationContactMethods
                .Where(cm => cm.CompanyId == organizationId &&
                           (cm.IsDeleted == false)).AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<OrganizationContactMethod?> GetContactMethodByIdAsync(int contactMethodId)
        {
            return await _context.OrganizationContactMethods
                .Where(cm => cm.Id == contactMethodId &&
                           (cm.IsDeleted == false))
                .FirstOrDefaultAsync();
        }

        public async Task<OrganizationContactMethod> AddContactMethodAsync(OrganizationContactMethod contactMethod)
        {
            _context.OrganizationContactMethods.Add(contactMethod);
            await _context.SaveChangesAsync();
            return contactMethod;
        }

        public async Task<OrganizationContactMethod> UpdateContactMethodAsync(OrganizationContactMethod contactMethod)
        {
            _context.Entry(contactMethod).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return contactMethod;
        }

        public async Task DeleteContactMethodAsync(int contactMethodId)
        {
            var contactMethod = await GetContactMethodByIdAsync(contactMethodId);
            if (contactMethod != null)
            {
                contactMethod.Delete();
                await _context.SaveChangesAsync();
            }
        }

        public async Task RestoreContactMethodAsync(int contactMethodId)
        {
            var contactMethod = await _context.OrganizationContactMethods
                .Where(cm => cm.Id == contactMethodId && cm.IsDeleted == true)
                .FirstOrDefaultAsync();

            if (contactMethod != null)
            {
                contactMethod.Restore();
                await _context.SaveChangesAsync();
            }
        }

        public async Task<(IEnumerable<OrganizationContactMethod> Data, int TotalCount)> GetContactMethodsByTypeAsync(int pageNumber, int pageSize, int organizationId, ContactType type)
        {
            var query = _context.OrganizationContactMethods
                   .Where(cm => cm.CompanyId == organizationId &&
                        cm.Type == type &&
                        !cm.IsDeleted).AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        // ===== Payment Info Management =====
        public async Task<PaymentInfo?> GetPaymentInfoByOrganizationIdAsync(int organizationId)
        {
            var organization = await GetOrganizationByIdAsync(organizationId);
            if (organization?.PaymentId != null)
            {
                return await _context.PaymentsInfo
                    .Where(p => p.Id == organization.PaymentId.Value &&
                               (p.IsDeleted == false))
                    .FirstOrDefaultAsync();
            }
            return null;
        }

        public async Task<PaymentInfo?> GetPaymentInfoByIdAsync(int paymentInfoId)
        {
            return await _context.PaymentsInfo
                .Where(p => p.Id == paymentInfoId &&
                           (p.IsDeleted == false))
                .FirstOrDefaultAsync();
        }

        public async Task<PaymentInfo> AddPaymentInfoAsync(PaymentInfo paymentInfo)
        {
            _context.PaymentsInfo.Add(paymentInfo);
            await _context.SaveChangesAsync();
            return paymentInfo;
        }

        public async Task<PaymentInfo> UpdatePaymentInfoAsync(PaymentInfo paymentInfo)
        {
            _context.Entry(paymentInfo).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return paymentInfo;
        }

        public async Task DeletePaymentInfoAsync(int paymentInfoId)
        {
            var paymentInfo = await GetPaymentInfoByIdAsync(paymentInfoId);
            if (paymentInfo != null)
            {
                paymentInfo.Delete();
                await _context.SaveChangesAsync();
            }
        }

        public async Task RestorePaymentInfoAsync(int paymentInfoId)
        {
            var paymentInfo = await _context.PaymentsInfo
                .Where(p => p.Id == paymentInfoId && p.IsDeleted == true)
                .FirstOrDefaultAsync();

            if (paymentInfo != null)
            {
                paymentInfo.Restore();
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> HasPaymentInfoAsync(int organizationId)
        {
            var organization = await GetOrganizationByIdAsync(organizationId);
            return organization?.PaymentId != null;
        }

        // ===== Organization Performance =====
        public async Task<(IEnumerable<Organization> Data, int TotalCount)> GetOrganizationsByCampaignCountAsync(int pageNumber, int pageSize, int minCampaigns = 1)
        {
            var query = _context.Organizations
        .Where(o => !o.IsDeleted)
        .Select(o => new
        {
            Organization = o,
            CampaignCount = o.SoloCampaigns.Count() + o.SharedCampaigns.Count()
        })
        .Where(x => x.CampaignCount >= minCampaigns)
        .Select(x => x.Organization)
        .Include(o => o.SoloCampaigns.Where(c => !c.IsDeleted))
        .Include(o => o.SharedCampaigns.Where(c => !c.IsDeleted))
        .AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        // ===== Validation & Checks =====
        public async Task<bool> OrganizationExistsAsync(int id)
        {
            return await _context.Organizations.AnyAsync(o => o.Id == id);
        }

        public async Task<bool> OrganizationNameExistsAsync(string name)
        {
            return await _context.Organizations
                .AnyAsync(o => o.Name == name && (o.IsDeleted == false));
        }

        // ===== Eager Loading =====
        public async Task<Organization?> GetOrganizationWithDetailsAsync(int id)
        {
            return await _context.Organizations
                .Where(o => o.Id == id && (o.IsDeleted == false))
                .Include(o => o.ContactMethods.Where(cm => cm.IsDeleted == false))
                .Include(o => o.PaymentInfo)
                .Include(o => o.Campaigns.Where(c => c.IsDeleted == false))
                .FirstOrDefaultAsync();
        }

        // ===== Dashboard & Reporting =====
        public async Task<(IEnumerable<Organization> Data, int TotalCount)> GetRecentlyRegisteredOrganizationsAsync(int pageNumber, int pageSize, int days = 30)
        {
            var cutoffDate = DateTime.Now.AddDays(-days);

            var query = _context.Organizations
        .Where(o => !o.IsDeleted &&
                    o.RegistrationDate >= cutoffDate)
        .OrderByDescending(o => o.RegistrationDate).AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        // ===== Advanced Queries =====
        public async Task<(IEnumerable<Organization> Data, int TotalCount)> GetOrganizationsWithoutCampaignsAsync(int pageNumber, int pageSize)
        {
            var query = _context.Organizations
                .Where(o => !o.IsDeleted &&
                    (o.Campaigns == null || !o.Campaigns.Any())).AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<(IEnumerable<Organization> Data, int TotalCount)> GetOrganizationsWithoutPaymentInfoAsync(int pageNumber, int pageSize)
        {
            var query = _context.Organizations
       .Where(o => !o.IsDeleted &&
                   o.PaymentId == null).AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<(IEnumerable<Organization> Data, int TotalCount)> GetOrganizationsWithActiveCampaignsAsync(int pageNumber, int pageSize)
        {
            var query = _context.Organizations
         .Where(o => !o.IsDeleted &&
                     o.Campaigns != null &&
                     o.Campaigns.Any(c => c.Status == CampaignStatus.Active)).AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<(IEnumerable<Organization> Data, int TotalCount)> GetOrganizationsWithCompletedCampaignsAsync(int pageNumber, int pageSize)
        {
            var query = _context.Organizations
        .Where(o => !o.IsDeleted &&
                    o.Campaigns != null &&
                    o.Campaigns.Any(c => c.Status == CampaignStatus.Completed)).AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        // ===== Contact Method Utilities =====
        public async Task<bool> ContactMethodExistsAsync(int organizationId, ContactType type, string value)
        {
            return await _context.OrganizationContactMethods
                .AnyAsync(cm => cm.CompanyId == organizationId &&
                               cm.Type == type &&
                               cm.Value == value &&
                               (cm.IsDeleted == false));
        }

        public async Task<int> GetContactMethodCountByTypeAsync(int organizationId, ContactType type)
        {
            return await _context.OrganizationContactMethods
                .Where(cm => cm.CompanyId == organizationId &&
                           cm.Type == type &&
                           (cm.IsDeleted == false))
                .CountAsync();
        }

        public async Task<(IEnumerable<Organization> Data, int TotalCount)> GetOrganizationsByContactTypeAsync(int pageNumber, int pageSize, ContactType type)
        {
            var query = _context.Organizations
        .Where(o => !o.IsDeleted &&
                    o.ContactMethods != null &&
                    o.ContactMethods.Any(cm => cm.Type == type)).AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        // ===== Payment Info Utilities =====
        public async Task<bool> ValidatePaymentInfoAsync(int organizationId)
        {
            var paymentInfo = await GetPaymentInfoByOrganizationIdAsync(organizationId);
            return paymentInfo != null &&
                   !string.IsNullOrEmpty(paymentInfo.ApiKey) &&
                   !string.IsNullOrEmpty(paymentInfo.HmacKey)
                   &&
                   !string.IsNullOrEmpty(paymentInfo.IntegrationId)
                   &&
                   !string.IsNullOrEmpty(paymentInfo.IframeId);
        }

        public async Task<(IEnumerable<Organization> Data, int TotalCount)> GetOrganizationsWithValidPaymentInfoAsync(int pageNumber, int pageSize)
        {
            // First get IDs of organizations with valid payment info
            var validOrgIds = await _context.Organizations
                .Where(o => !o.IsDeleted &&
                            o.PaymentId != null &&
                            _context.PaymentsInfo.Any(p => p.Id == o.PaymentId &&
                                                           !p.IsDeleted &&
                                                           !string.IsNullOrEmpty(p.ApiKey) &&
                                                           !string.IsNullOrEmpty(p.HmacKey) &&
                                                           !string.IsNullOrEmpty(p.IntegrationId) &&
                                                           !string.IsNullOrEmpty(p.IframeId)))
                .Select(o => o.Id)
                .ToListAsync();

            var totalCount = validOrgIds.Count;

            // Then get the paginated organizations
            var items = await _context.Organizations
                .Where(o => validOrgIds.Contains(o.Id))
                .OrderBy(o => o.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(o => o.ContactMethods.Where(cm => !cm.IsDeleted))
                .Include(o => o.PaymentInfo)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Dictionary<int, DateTime>> GetOrganizationLastPaymentUpdateAsync()
        {
            // Use a single query with JOIN instead of N+1 queries
            var result = await _context.Organizations
                .Where(o => !o.IsDeleted &&
                            o.PaymentId != null)
                .Join(_context.PaymentsInfo,
                      org => org.PaymentId,
                      payment => payment.Id,
                      (org, payment) => new
                      {
                          OrganizationId = org.Id,
                          PaymentUpdatedOn = payment.UpdatedOn
                      })
                .Where(x => x.PaymentUpdatedOn.HasValue)
                .ToDictionaryAsync(
                    x => x.OrganizationId,
                    x => x.PaymentUpdatedOn.Value
                );

            return result;
        }

        // Admin Management
        public async Task<Organization> AssignOrganizationAdminAsync(int organizationId, string adminUserId)
        {
            var organization = await GetOrganizationByIdAsync(organizationId);
            if (organization == null)
                throw new Exception("Organization not found");

            var user = await _context.Users.FindAsync(adminUserId);
            if (user == null)
                throw new Exception("User not found");

            organization.AssignAdmin(adminUserId);
            await _context.SaveChangesAsync();
            return organization;
        }

        public async Task<Organization> RemoveOrganizationAdminAsync(int organizationId)
        {
            var organization = await GetOrganizationByIdAsync(organizationId);
            if (organization == null)
                throw new Exception("Organization not found");

            organization.RemoveAdmin();
            await _context.SaveChangesAsync();
            return organization;
        }

        public async Task<Organization> TransferOrganizationAdminAsync(int organizationId, string newAdminUserId)
        {
            var organization = await GetOrganizationByIdAsync(organizationId);
            if (organization == null)
                throw new Exception("Organization not found");

            var user = await _context.Users.FindAsync(newAdminUserId);
            if (user == null)
                throw new Exception("User not found");

            organization.RemoveAdmin();
            organization.AssignAdmin(newAdminUserId);
            await _context.SaveChangesAsync();
            return organization;
        }

        public async Task<User?> GetOrganizationAdminAsync(int organizationId)
        {
            var organization = await GetOrganizationByIdAsync(organizationId);
            if (organization == null || string.IsNullOrEmpty(organization.AdminUserId))
                return null;

            return await _context.Users
                .Where(u => u.Id == organization.AdminUserId && !u.IsDeleted)
                .FirstOrDefaultAsync();
        }

        // SubAdmin Management
        public async Task<IEnumerable<User>> GetOrganizationSubAdminsAsync(int organizationId)
        {
            var roleIds = await _context.OrganizationRoles
                .Where(r => r.OrganizationId == organizationId &&
                           r.Role == OrganizationRoleType.SubAdmin &&
                           !r.IsDeleted)
                .Select(r => r.UserId)
                .ToListAsync();

            return await _context.Users
                .Where(u => roleIds.Contains(u.Id) && !u.IsDeleted)
                .ToListAsync();
        }

        public async Task<OrganizationRole> AddSubAdminAsync(int organizationId, string userId)
        {
            // Check if user is already an admin
            var isAdmin = await _context.Organizations
                .AnyAsync(o => o.Id == organizationId && o.AdminUserId == userId);

            if (isAdmin)
                throw new Exception("User is already an Organization Admin");

            // Check if already a sub-admin
            var existingRole = await _context.OrganizationRoles
                .Where(r => r.OrganizationId == organizationId &&
                           r.UserId == userId &&
                           !r.IsDeleted)
                .FirstOrDefaultAsync();

            if (existingRole != null)
            {
                existingRole = new OrganizationRole(organizationId, userId, OrganizationRoleType.SubAdmin);
                _context.OrganizationRoles.Update(existingRole);
            }
            else
            {
                var role = new OrganizationRole(organizationId, userId, OrganizationRoleType.SubAdmin);
                _context.OrganizationRoles.Add(role);
            }

            await _context.SaveChangesAsync();

            return await _context.OrganizationRoles
                .Where(r => r.OrganizationId == organizationId && r.UserId == userId)
                .FirstOrDefaultAsync()!;
        }

        public async Task RemoveSubAdminAsync(int organizationId, string userId)
        {
            var role = await _context.OrganizationRoles
                .Where(r => r.OrganizationId == organizationId &&
                           r.UserId == userId &&
                           r.Role == OrganizationRoleType.SubAdmin &&
                           !r.IsDeleted)
                .FirstOrDefaultAsync();

            if (role != null)
            {
                role.Delete();
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsUserSubAdminAsync(int organizationId, string userId)
        {
            return await _context.OrganizationRoles
                .AnyAsync(r => r.OrganizationId == organizationId &&
                              r.UserId == userId &&
                              r.Role == OrganizationRoleType.SubAdmin &&
                              !r.IsDeleted);
        }

        public async Task<IEnumerable<OrganizationRole>> GetOrganizationRolesAsync(int organizationId)
        {
            return await _context.OrganizationRoles
                .Where(r => r.OrganizationId == organizationId && !r.IsDeleted)
                .Include(r => r.User)
                .ToListAsync();
        }

        public async Task<OrganizationRole> AddOrganizationRoleAsync(int organizationId, string userId, OrganizationRoleType role)
        {
            // Check if user already has a role in this organization
            var existingRole = await _context.OrganizationRoles
                .Where(r => r.OrganizationId == organizationId &&
                           r.UserId == userId &&
                           !r.IsDeleted)
                .FirstOrDefaultAsync();

            if (existingRole != null)
            {
                // Update existing role instead of creating new one
                existingRole = new OrganizationRole(organizationId, userId, role);
                _context.OrganizationRoles.Update(existingRole);
            }
            else
            {
                var organizationRole = new OrganizationRole(organizationId, userId, role);
                _context.OrganizationRoles.Add(organizationRole);
            }

            await _context.SaveChangesAsync();

            return await _context.OrganizationRoles
                .Where(r => r.OrganizationId == organizationId && r.UserId == userId)
                .FirstOrDefaultAsync()!;
        }

        public async Task RemoveOrganizationRoleAsync(int organizationId, string userId)
        {
            var role = await _context.OrganizationRoles
                .Where(r => r.OrganizationId == organizationId &&
                           r.UserId == userId &&
                           !r.IsDeleted)
                .FirstOrDefaultAsync();

            if (role != null)
            {
                role.Delete();
                await _context.SaveChangesAsync();
            }
        }
    }
}
