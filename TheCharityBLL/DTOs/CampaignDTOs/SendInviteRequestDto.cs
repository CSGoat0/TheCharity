namespace TheCharityBLL.DTOs.CampaignDTOs
{
    public class SendInviteRequestDto
    {
        public int OrganizationId { get; set; }
        public int ExpiresInDays { get; set; } = 7; // Default 7 days
    }
}
