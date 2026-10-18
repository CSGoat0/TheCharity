using System.ComponentModel.DataAnnotations;

namespace TheCharityBLL.DTOs.CampaignDTOs
{
    public class UpdateSharedCampaignDto : UpdateCampaignDto
    {
        /// <example>[1, 2, 4]</example>
        [MinLength(1, ErrorMessage = "If provided, at least one participating organization is required.")]
        public List<int>? OrganizationIds { get; set; }
    }
}