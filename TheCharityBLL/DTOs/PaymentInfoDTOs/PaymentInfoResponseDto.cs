namespace TheCharityBLL.DTOs.PaymentInfoDTOs
{
    public class PaymentInfoResponseDto
    {
        public int Id { get; set; }
        public string ApiKey { get; set; } 
        public string IntegrationId { get; set; } 
        public string IframeId { get; set; } 
        public string HmacKey { get; set; } 
        public DateTime? RegistrationDate { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
        public int? OrganizationId { get; set; }
    }
}
