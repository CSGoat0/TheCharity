using TheCharityDAL.Entities;
using TheCharityDAL.Enums;

namespace TheCharityDAL.Repositories.Abstraction
{
    public interface IOrganizationRepository
    {
        // ===== Organization CRUD Operations =====
        Task<(IEnumerable<Organization> Data, int TotalCount)> GetAllOrganizationsAsync(int pageNumber, int pageSize, bool includeDeleted = false);
        Task<Organization?> GetOrganizationByIdAsync(int id);
        Task<Organization> AddOrganizationAsync(Organization organization);
        Task<Organization> UpdateOrganizationAsync(Organization organization);
        Task DeleteOrganizationAsync(int id);
        Task RestoreOrganizationAsync(int id);

        // ===== Organization Filtering & Search =====
        Task<Organization?> GetOrganizationByNameAsync(string name);
        Task<Organization?> GetOrganizationByPaymentInfoIdAsync(int PaymentInfoId);
        Task<(IEnumerable<Organization> Data,int TotalCount)> SearchOrganizationsAsync(int pageNumber, int pageSize, string searchTerm);
        Task<(IEnumerable<Organization> Data, int TotalCount)> GetDeletedOrganizationsAsync(int pageNumber, int pageSize);
        Task<(IEnumerable<Organization> Data, int TotalCount)> GetOrganizationsDropDownAsync(int pageNumber, int pageSize);
        Task<(IEnumerable<Organization> Data, int TotalCount)> GetOrganizationsByAddressAsync(int pageNumber, int pageSize, string address);

        // ===== Organization Statistics =====
        Task<int> GetTotalOrganizationsCountAsync();
        Task<int> GetActiveOrganizationsCountAsync();

        // ===== Organization Contact Methods =====
        Task<(IEnumerable<OrganizationContactMethod> Data,int TotalCount)> GetOrganizationContactMethodsAsync(int pageNumber, int pageSize, int organizationId);
        Task<OrganizationContactMethod?> GetContactMethodByIdAsync(int contactMethodId);
        Task<OrganizationContactMethod> AddContactMethodAsync(OrganizationContactMethod contactMethod);
        Task<OrganizationContactMethod> UpdateContactMethodAsync(OrganizationContactMethod contactMethod);
        Task DeleteContactMethodAsync(int contactMethodId);
        Task RestoreContactMethodAsync(int contactMethodId);
        Task<(IEnumerable<OrganizationContactMethod> Data, int TotalCount)> GetContactMethodsByTypeAsync(int pageNumber, int pageSize, int organizationId, ContactType type);

        // ===== Payment Info Management =====
        Task<PaymentInfo?> GetPaymentInfoByOrganizationIdAsync(int organizationId);
        Task<PaymentInfo?> GetPaymentInfoByIdAsync(int paymentInfoId);
        Task<PaymentInfo> AddPaymentInfoAsync(PaymentInfo paymentInfo);
        Task<PaymentInfo> UpdatePaymentInfoAsync(PaymentInfo paymentInfo);
        Task DeletePaymentInfoAsync(int paymentInfoId);
        Task RestorePaymentInfoAsync(int paymentInfoId);
        Task<bool> HasPaymentInfoAsync(int organizationId);

        // ===== Organization Performance =====
        Task<(IEnumerable<Organization> Data, int TotalCount)> GetOrganizationsByCampaignCountAsync(int pageNumber, int pageSize, int minCampaigns = 1);

        // ===== Validation & Checks =====
        Task<bool> OrganizationExistsAsync(int id);
        Task<bool> OrganizationNameExistsAsync(string name);

        // ===== Eager Loading =====
        Task<Organization?> GetOrganizationWithDetailsAsync(int id);

        // ===== Dashboard & Reporting =====
        Task<(IEnumerable<Organization> Data, int TotalCount)> GetRecentlyRegisteredOrganizationsAsync(int pageNumber, int pageSize, int days = 30);

        // ===== Advanced Queries =====
        Task<(IEnumerable<Organization> Data, int TotalCount)> GetOrganizationsWithoutCampaignsAsync(int pageNumber, int pageSize);
        Task<(IEnumerable<Organization> Data, int TotalCount)> GetOrganizationsWithoutPaymentInfoAsync(int pageNumber, int pageSize);
        Task<(IEnumerable<Organization> Data, int TotalCount)> GetOrganizationsWithActiveCampaignsAsync(int pageNumber, int pageSize);
        Task<(IEnumerable<Organization> Data, int TotalCount)> GetOrganizationsWithCompletedCampaignsAsync(int pageNumber, int pageSize);

        // ===== Contact Method Utilities =====
        Task<bool> ContactMethodExistsAsync(int organizationId, ContactType type, string value);
        Task<int> GetContactMethodCountByTypeAsync(int organizationId, ContactType type);
        Task<(IEnumerable<Organization> Data, int TotalCount)> GetOrganizationsByContactTypeAsync(int pageNumber, int pageSize, ContactType type);
        
        // ===== Payment Info Utilities =====
        Task<bool> ValidatePaymentInfoAsync(int organizationId);
        Task<(IEnumerable<Organization> Data, int TotalCount)> GetOrganizationsWithValidPaymentInfoAsync(int pageNumber, int pageSize);
        Task<Dictionary<int, DateTime>> GetOrganizationLastPaymentUpdateAsync();

        // ===== Admin Management =====
        Task<Organization> AssignOrganizationAdminAsync(int organizationId, string adminUserId);
        Task<Organization> RemoveOrganizationAdminAsync(int organizationId);
        Task<Organization> TransferOrganizationAdminAsync(int organizationId, string newAdminUserId);
        Task<User?> GetOrganizationAdminAsync(int organizationId);

        // ===== SubAdmin Management =====
        Task<IEnumerable<User>> GetOrganizationSubAdminsAsync(int organizationId);
        Task<OrganizationRole> AddSubAdminAsync(int organizationId, string userId);
        Task RemoveSubAdminAsync(int organizationId, string userId);
        Task<bool> IsUserSubAdminAsync(int organizationId, string userId);

        // ===== Organization Role Utilities =====
        Task<IEnumerable<OrganizationRole>> GetOrganizationRolesAsync(int organizationId);
        Task<OrganizationRole> AddOrganizationRoleAsync(int organizationId, string userId, OrganizationRoleType role);
        Task RemoveOrganizationRoleAsync(int organizationId, string userId);
    }
}