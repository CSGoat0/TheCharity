using TheCharityBLL.DTOs.CampaignDTOs;
using TheCharityBLL.DTOs.OrganizationContactMethodDTOs;

namespace TheCharityBLL.DTOs.OrganizationDTOs
{
    public class OrganizationDetailsDto
    {
        /// <example>1</example>
        public int Id { get; set; }

        /// <example>Egyptian Food Bank</example>
        public string Name { get; set; } = null!;

        /// <example>Cairo, Egypt</example>
        public string? Address { get; set; }

        public ICollection<OrgContactMethodResponseDto> ContactMethods { get; set; }
            = new List<OrgContactMethodResponseDto>();

        public ICollection<CampaignResponseDto> Campaigns { get; set; }
            = new List<CampaignResponseDto>();

        /// <example>false</example>
        public bool IsDeleted { get; set; }

        /// <example>2025-01-10T09:30:00</example>
        public DateTime RegistrationDate { get; set; }

        /// <example>2025-06-20T14:15:00</example>
        public DateTime? UpdatedOn { get; set; }
        public int? PaymentId { get; set; }
        public string? AdminUserId { get; set; }
        public string AdminUserName { get; set; }
        public string AdminUserFullName { get; set; }
        public string AdminUserEmail { get; set; }
        public int SoloCampaignsCount { get; set; }
        public int SharedCampaignsCount { get; set; }
        public int TotalCampaignsCount { get; set; }
    }
}
