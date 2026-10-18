using System.ComponentModel.DataAnnotations;
using TheCharityDAL.Enums;

namespace TheCharityBLL.DTOs.CampaignDTOs
{
    public class UpdateCampaignDto
    {
        /// <example>1</example>
        [Range(1, int.MaxValue, ErrorMessage = "Id must be a positive integer.")]
        public int Id { get; set; }

        /// <example>Updated Campaign Title</example>
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string? Title { get; set; }

        /// <example>Updated description for the campaign</example>
        [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }

        /// <example>/images/updated_campaign.jpg</example>
        [MaxLength(500, ErrorMessage = "ImgPath cannot exceed 500 characters.")]
        public string? ImgPath { get; set; }

        /// <example>150000</example>
        [Range(1, 1_000_000_000, ErrorMessage = "Target must be between 1 and 1,000,000,000.")]
        public double? Target { get; set; }
    }
}