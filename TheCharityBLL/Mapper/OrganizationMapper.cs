using Riok.Mapperly.Abstractions;
using TheCharityBLL.DTOs.OrganizationContactMethodDTOs;
using TheCharityBLL.DTOs.OrganizationDTOs;
using TheCharityBLL.DTOs.PaymentInfoDTOs;
using TheCharityDAL.Entities;

namespace TheCharityBLL.Mapper
{
    [Mapper]
    public partial class OrganizationMapper
    {
        public partial Organization MapToOrganization(CreateOrganizationDto createOrganizationDto);
        public partial IEnumerable<OrganizationDropDownListDto> MapToOrganizationDropDownListDtos(IEnumerable<Organization> organizations);
        public partial OrganizationContactMethod MapToOrganizationContactMethod(CreateOrgContactMethodDto contactMethod);
        public partial OrgContactMethodResponseDto MapToOrganizationContactMethodResponseDto(OrganizationContactMethod contactMethod);
        public partial IEnumerable<OrgContactMethodResponseDto> MapToOrganizationContactMethodResponseDtos(IEnumerable<OrganizationContactMethod> contactMethods);
        public partial PaymentInfo MapToPaymentInfo(CreatePaymentInfoDto paymentInfo);
        public partial PaymentInfoResponseDto MapToPaymentInfoResponseDto(PaymentInfo paymentInfo);
        public partial IEnumerable<PaymentInfoResponseDto> MapToPaymentInfoResponseDto(IEnumerable<PaymentInfo> paymentInfo);
        public OrganizationResponseDto MapToOrganizationResponseDto(Organization organization)
        {
            if (organization == null) return null!;

            // Manually map ALL properties
            var dto = new OrganizationResponseDto
            {
                Id = organization.Id,
                Name = organization.Name,
                Address = organization.Address,
                PaymentId = organization.PaymentId,
                IsDeleted = organization.IsDeleted,
                RegistrationDate = organization.RegistrationDate.Value,
                UpdatedOn = organization.UpdatedOn,

                // ===== Map Admin properties =====
                AdminUserId = organization.AdminUserId,
                AdminUserName = organization.AdminUser?.UserName ?? organization.AdminUser?.Email ?? string.Empty,
                AdminUserFullName = organization.AdminUser?.FullName ?? string.Empty,
                AdminUserEmail = organization.AdminUser?.Email ?? string.Empty,
                // ===== END =====

                // Map ContactMethods
                ContactMethods = organization.ContactMethods?
                    .Where(cm => !cm.IsDeleted)
                    .Select(cm => new OrgContactMethodResponseDto
                    {
                        Id = cm.Id,
                        Value = cm.Value,
                        Type = cm.Type.Value
                    }).ToList() ?? new List<OrgContactMethodResponseDto>()
            };

            return dto;
        }

        public OrganizationDetailsDto MapToOrganizationDetailsDto(Organization organization)
        {
            if (organization == null) return null!;

            // Manually map ALL properties
            var dto = new OrganizationDetailsDto
            {
                Id = organization.Id,
                Name = organization.Name,
                Address = organization.Address,
                PaymentId = organization.PaymentId,
                IsDeleted = organization.IsDeleted,
                RegistrationDate = organization.RegistrationDate.Value,
                UpdatedOn = organization.UpdatedOn,

                // ===== Map Admin properties =====
                AdminUserId = organization.AdminUserId,
                AdminUserName = organization.AdminUser?.UserName ?? organization.AdminUser?.Email ?? string.Empty,
                AdminUserFullName = organization.AdminUser?.FullName ?? string.Empty,
                AdminUserEmail = organization.AdminUser?.Email ?? string.Empty,
                // ===== END =====

                // Map ContactMethods
                ContactMethods = organization.ContactMethods?
                    .Where(cm => !cm.IsDeleted)
                    .Select(cm => new OrgContactMethodResponseDto
                    {
                        Id = cm.Id,
                        Value = cm.Value,
                        Type = cm.Type.Value
                    }).ToList() ?? new List<OrgContactMethodResponseDto>(),

                // ===== Map campaign statistics =====
                SoloCampaignsCount = organization.SoloCampaigns?.Count(c => !c.IsDeleted) ?? 0,
                SharedCampaignsCount = organization.SharedCampaigns?.Count(c => !c.IsDeleted) ?? 0,
                TotalCampaignsCount = (organization.SoloCampaigns?.Count(c => !c.IsDeleted) ?? 0) +
                                      (organization.SharedCampaigns?.Count(c => !c.IsDeleted) ?? 0)
                // ===== END =====
            };

            return dto;
        }

        public IEnumerable<OrganizationResponseDto> MapToOrganizationResponseDtos(IEnumerable<Organization> organizations)
        {
            if (organizations == null)
                return new List<OrganizationResponseDto>();

            return organizations.Select(org => MapToOrganizationResponseDto(org)).ToList();
        }
    }
}
