using System.ComponentModel.DataAnnotations;

namespace TheCharityBLL.DTOs.CampaignDTOs
{
    public class UpdateSoloCampaignDto : UpdateCampaignDto
    {
        /// <example>5</example>
        [Range(1, int.MaxValue, ErrorMessage = "OrganizationId must be a positive integer.")]
        public int? OrganizationId { get; set; }
    }
}