using System.ComponentModel.DataAnnotations;

namespace TheCharityBLL.DTOs.DonationDTOs
{
    public class UpdateDonationDto
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public double? Amount { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "CampaignId must be a positive integer.")]
        public int? CampaignId { get; set; }
    }
}