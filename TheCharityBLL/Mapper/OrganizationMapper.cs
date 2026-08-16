using Riok.Mapperly.Abstractions;
using TheCharityBLL.DTOs.CampaignDTOs;
using TheCharityBLL.DTOs.OrganizationContactMethodDTOs;
using TheCharityBLL.DTOs.OrganizationDTOs;
using TheCharityBLL.DTOs.PaymentInfoDTOs;
using TheCharityDAL.Entities;
using TheCharityDAL.Enums;

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

            // Get campaigns and filter out deleted ones
            var soloCampaigns = organization.SoloCampaigns?.Where(c => !c.IsDeleted).ToList() ?? new List<SoloCampaign>();
            var sharedCampaigns = organization.SharedCampaigns?.Where(c => !c.IsDeleted).ToList() ?? new List<SharedCampaign>();

            // Map Solo Campaigns to CampaignResponseDto
            var soloCampaignDtos = soloCampaigns.Select(c => new CampaignResponseDto
            {
                Id = c.Id,
                OrganizationId = c.OrganizationId ?? 0,
                Title = c.Title,
                Description = c.Description,
                ImgPath = c.ImgPath,
                Target = c.Target,
                Achieved = c.Achieved,
                Status = c.Status,
                IsDeleted = c.IsDeleted,
                RegistrationDate = c.RegistrationDate,
                UpdatedOn = c.UpdatedOn,
                Deadline = c.Deadline,
                DaysRemaining = c.Deadline.HasValue
                    ? (int?)Math.Max(0, (c.Deadline.Value - DateTime.UtcNow).Days)
                    : null
            }).ToList();

            // Map Shared Campaigns to CampaignResponseDto
            var sharedCampaignDtos = sharedCampaigns.Select(c => new CampaignResponseDto
            {
                Id = c.Id,
                OrganizationId = c.OrganizationId ?? 0,
                Title = c.Title,
                Description = c.Description,
                ImgPath = c.ImgPath,
                Target = c.Target,
                Achieved = c.Achieved,
                Status = c.Status,
                IsDeleted = c.IsDeleted,
                RegistrationDate = c.RegistrationDate,
                UpdatedOn = c.UpdatedOn,
                Deadline = c.Deadline,
                DaysRemaining = c.Deadline.HasValue
                    ? (int?)Math.Max(0, (c.Deadline.Value - DateTime.UtcNow).Days)
                    : null
            }).ToList();

            var dto = new OrganizationDetailsDto
            {
                // Base properties
                Id = organization.Id,
                Name = organization.Name,
                Address = organization.Address,
                PaymentId = organization.PaymentId,
                IsDeleted = organization.IsDeleted,
                RegistrationDate = organization.RegistrationDate.Value,
                UpdatedOn = organization.UpdatedOn,

                // Admin properties
                AdminUserId = organization.AdminUserId,
                AdminUserName = organization.AdminUser?.UserName ?? organization.AdminUser?.Email ?? string.Empty,
                AdminUserFullName = organization.AdminUser?.FullName ?? string.Empty,
                AdminUserEmail = organization.AdminUser?.Email ?? string.Empty,

                // Contact Methods
                ContactMethods = organization.ContactMethods?
                    .Where(cm => !cm.IsDeleted)
                    .Select(cm => new OrgContactMethodResponseDto
                    {
                        Id = cm.Id,
                        Value = cm.Value,
                        Type = cm.Type.Value
                    }).ToList() ?? new List<OrgContactMethodResponseDto>(),

                // Solo Campaigns
                SoloCampaigns = soloCampaignDtos,

                // Shared Campaigns
                SharedCampaigns = sharedCampaignDtos,

                // Campaign Statistics
                SoloCampaignsCount = soloCampaigns.Count,
                SharedCampaignsCount = sharedCampaigns.Count,
                TotalCampaignsCount = soloCampaigns.Count + sharedCampaigns.Count
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
