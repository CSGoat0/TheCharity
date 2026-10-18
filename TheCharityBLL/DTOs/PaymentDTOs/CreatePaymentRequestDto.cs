using System.ComponentModel.DataAnnotations;

namespace TheCharityBLL.DTOs.PaymentDTOs
{
    public class CreatePaymentRequestDto
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "CampaignId must be a positive integer.")]
        public int CampaignId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "OrganizationId must be a positive integer.")]
        public int OrganizationId { get; set; }
    }
}