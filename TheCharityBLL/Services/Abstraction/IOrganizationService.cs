
using TheCharityBLL.DTOs;
using TheCharityBLL.DTOs.OrganizationContactMethodDTOs;
using TheCharityBLL.DTOs.OrganizationDTOs;
using TheCharityBLL.DTOs.PaymentInfoDTOs;
using TheCharityBLL.DTOs.UserDTOs;
using TheCharityDAL.Entities;
using TheCharityDAL.Enums;

namespace TheCharityBLL.Services.Abstraction
{
    public interface IOrganizationService
    {
        Task<ServiceResponse<IEnumerable<OrganizationResponseDto>>> GetAllOrganizations(bool includeDeleted = false);
        Task<ServiceResponse<OrganizationResponseDto>> GetOrganizationById(int id);
        Task<ServiceResponse<OrganizationResponseDto>> CreateOrganization(CreateOrganizationDto organization);
        Task<ServiceResponse<OrganizationResponseDto>> UpdateOrganization(int id,UpdateOrganizationDto organization);
        Task<ServiceResponse<bool>> DeleteOrganization(int id);
        Task<ServiceResponse<bool>> RestoreOrganization(int id);

        Task<ServiceResponse<OrganizationResponseDto>> GetOrganizationByName(string name);
        Task<ServiceResponse<IEnumerable<OrganizationResponseDto>>> SearchOrganizations(string searchTerm);
        Task<ServiceResponse<IEnumerable<OrganizationResponseDto>>> GetDeletedOrganizations();
        Task<ServiceResponse<IEnumerable<OrganizationDropDownListDto>>> GetOrganizationsDropDown();
        Task<ServiceResponse<IEnumerable<OrganizationResponseDto>>> GetOrganizationsByAddress(string address);

        Task<ServiceResponse<int>> GetTotalOrganizationsCount();
        Task<ServiceResponse<int>> GetActiveOrganizationsCount();

        Task<ServiceResponse<IEnumerable<OrgContactMethodResponseDto>>> GetOrganizationContactMethods(int organizationId);
        Task<ServiceResponse<OrgContactMethodResponseDto>> GetContactMethodById(int contactMethodId);
        Task<ServiceResponse<OrgContactMethodResponseDto>> CreateContactMethod(CreateOrgContactMethodDto contactMethod);
        Task<ServiceResponse<OrgContactMethodResponseDto>> UpdateContactMethod(int id,UpdateOrgContactMethodDto contactMethod);
        Task<ServiceResponse<bool>> DeleteContactMethod(int contactMethodId);
        Task<ServiceResponse<bool>> RestoreContactMethod(int contactMethodId);
        Task<ServiceResponse<IEnumerable<OrgContactMethodResponseDto>>> GetContactMethodsByType(int organizationId, ContactType type);

        //Task<ServiceResponse<PaymentInfoResponseDto>> GetPaymentInfoByOrganizationId(int organizationId);
        //Task<ServiceResponse<PaymentInfoResponseDto>> GetPaymentInfoById(int paymentInfoId);
        //Task<ServiceResponse<PaymentInfoResponseDto>> CreatePaymentInfo(CreatePaymentInfoDto paymentInfo);
        //Task<ServiceResponse<PaymentInfoResponseDto>> UpdatePaymentInfo(int id,UpdatePaymentInfoDto paymentInfo);
        //Task<ServiceResponse<bool>> DeletePaymentInfo(int paymentInfoId);
        //Task<ServiceResponse<bool>> RestorePaymentInfo(int paymentInfoId);
        //Task<ServiceResponse<bool>> HasPaymentInfo(int organizationId);

        Task<ServiceResponse<IEnumerable<OrganizationResponseDto>>> GetOrganizationsByCampaignCount(int minCampaigns = 1);

        //Task<ServiceResponse<bool> OrganizationExists(int id);
        //Task<ServiceResponse<bool> OrganizationNameExists(string name);

        Task<ServiceResponse<OrganizationDetailsDto>> GetOrganizationDetails(int id);

        Task<ServiceResponse<IEnumerable<OrganizationResponseDto>>> GetRecentlyRegisteredOrganizations(int days = 7);

        Task<ServiceResponse<IEnumerable<OrganizationResponseDto>>> GetOrganizationsWithoutCampaigns();
        Task<ServiceResponse<IEnumerable<OrganizationResponseDto>>> GetOrganizationsWithoutPaymentInfo();
        Task<ServiceResponse<IEnumerable<OrganizationResponseDto>>> GetOrganizationsWithActiveCampaigns();
        Task<ServiceResponse<IEnumerable<OrganizationResponseDto>>> GetOrganizationsWithCompletedCampaigns();

        //Task<ServiceResponse<bool> ContactMethodExists(int organizationId, ContactType type, string value);
        Task<ServiceResponse<int>> GetContactMethodCountByType(int organizationId, ContactType type);
        Task<ServiceResponse<IEnumerable<OrganizationResponseDto>>> GetOrganizationsByContactType(ContactType type);

        //Task<ServiceResponse<bool> ValidatePaymentInfo(int organizationId);
        Task<ServiceResponse<IEnumerable<OrganizationResponseDto>>> GetOrganizationsWithValidPaymentInfo();
        Task<ServiceResponse<Dictionary<int, DateTime>>> GetOrganizationLastPaymentUpdate();

        // ===== Sub-Admin Management =====
        Task<ServiceResponse<OrganizationRoleResponseDto>> AddSubAdminAsync(int organizationId, string userId);
        Task<ServiceResponse<bool>> RemoveSubAdminAsync(int organizationId, string userId);
        Task<ServiceResponse<IEnumerable<UserResponseDTO>>> GetOrganizationSubAdminsAsync(int organizationId);
        Task<ServiceResponse<bool>> IsUserSubAdminAsync(int organizationId, string userId);

        // ===== Organization Admin Management =====
        Task<ServiceResponse<OrganizationResponseDto>> AssignOrganizationAdminAsync(int organizationId, string adminUserId);
        Task<ServiceResponse<OrganizationResponseDto>> RemoveOrganizationAdminAsync(int organizationId);
        Task<ServiceResponse<OrganizationResponseDto>> TransferOrganizationAdminAsync(int organizationId, string newAdminUserId);
        Task<ServiceResponse<UserResponseDTO?>> GetOrganizationAdminAsync(int organizationId);
    }
}
