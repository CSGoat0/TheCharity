using System.ComponentModel.DataAnnotations;

namespace TheCharityBLL.DTOs.CampaignDTOs
{
    public class UpdateCampaignMoneyDto
    {
        /// <example>1000</example>
        [Range(0, double.MaxValue, ErrorMessage = "Amount must be non-negative.")]
        public double Amount { get; set; }
    }
}