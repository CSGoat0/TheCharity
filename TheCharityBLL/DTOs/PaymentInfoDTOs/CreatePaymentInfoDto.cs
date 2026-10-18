using System.ComponentModel.DataAnnotations;

namespace TheCharityBLL.DTOs.PaymentInfoDTOs
{
    public class CreatePaymentInfoDto
    {
        [Required(ErrorMessage = "ApiKey is required.")]
        [MaxLength(300, ErrorMessage = "ApiKey cannot exceed 300 characters.")]
        public string ApiKey { get; set; } = null!;

        [Required(ErrorMessage = "IntegrationId is required.")]
        [MaxLength(300, ErrorMessage = "IntegrationId cannot exceed 300 characters.")]
        public string IntegrationId { get; set; } = null!;

        [Required(ErrorMessage = "IframeId is required.")]
        [MaxLength(300, ErrorMessage = "IframeId cannot exceed 300 characters.")]
        public string IframeId { get; set; } = null!;

        [Required(ErrorMessage = "HmacKey is required.")]
        [MaxLength(300, ErrorMessage = "HmacKey cannot exceed 300 characters.")]
        public string HmacKey { get; set; } = null!;

        [Range(1, int.MaxValue, ErrorMessage = "OrganizationId must be a positive integer.")]
        public int OrganizationId { get; set; }
    }
}