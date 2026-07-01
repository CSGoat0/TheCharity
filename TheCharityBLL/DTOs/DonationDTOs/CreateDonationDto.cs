using System.ComponentModel.DataAnnotations;

namespace TheCharityBLL.DTOs.DonationDTOs
{
    public class CreateDonationDto
    {
        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public double? Amount { get; set; }

        [Required(ErrorMessage = "UserId is required.")]
        public string UserId { get; set; } = null!;

        [Required(ErrorMessage = "CampaignId is required.")]
        public int? CampaignId { get; set; }
    }
}
