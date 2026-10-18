using System.ComponentModel.DataAnnotations;

namespace TheCharityBLL.DTOs.CampaignDTOs
{
    public class SendInviteRequestDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "OrganizationId must be a positive integer.")]
        public int OrganizationId { get; set; }

        [Range(1, 365, ErrorMessage = "ExpiresInDays must be between 1 and 365.")]
        public int ExpiresInDays { get; set; } = 7; // Default 7 days
    }
}