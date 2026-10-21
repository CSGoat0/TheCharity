using System.ComponentModel.DataAnnotations;

namespace TheCharityBLL.DTOs.CampaignDTOs
{
    public class CreateSharedCampaignDto : CreateCampaignDto
    {
        /// <example>[1, 2, 3]</example>
        [Required(ErrorMessage = "OrganizationIds is required.")]
        [MinLength(1, ErrorMessage = "At least one participating organization is required.")]
        public List<int> OrganizationIds { get; set; } = new List<int>();

        /// <example>https://example.com/campaigns/shared.jpg</example>
        [Url(ErrorMessage = "ImgPath must be a valid URL.")]
        [MaxLength(500, ErrorMessage = "ImgPath cannot exceed 500 characters.")]
        public string? ImgPath { get; set; }

        /// <example>100000</example>
        [Range(1, 1_000_000_000, ErrorMessage = "Target must be between 1 and 1,000,000,000.")]
        public int? Target { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "CreatorOrganizationId must be a positive integer.")]
        public int CreatorOrganizationId { get; set; }
    }
}