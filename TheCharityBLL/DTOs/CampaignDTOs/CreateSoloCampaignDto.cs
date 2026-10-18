using System.ComponentModel.DataAnnotations;

namespace TheCharityBLL.DTOs.CampaignDTOs
{
    public class CreateSoloCampaignDto : CreateCampaignDto
    {
        /// <example>5</example>
        [Range(1, int.MaxValue, ErrorMessage = "OrganizationId must be a positive integer.")]
        public int OrganizationId { get; set; }

        /// <example>/images/solo_campaign.jpg</example>
        [MaxLength(500, ErrorMessage = "ImgPath cannot exceed 500 characters.")]
        public string? ImgPath { get; set; }

        /// <example>50000</example>
        [Range(1, 1_000_000_000, ErrorMessage = "Target must be between 1 and 1,000,000,000.")]
        public int? Target { get; set; }
    }
}