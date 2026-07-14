using TheCharityDAL.Enums;

namespace TheCharityBLL.DTOs.CampaignDTOs
{
    public class InviteResponseDto
    {
        public int Id { get; set; }
        public int SharedCampaignId { get; set; }
        public string? CampaignTitle { get; set; }
        public int OrganizationId { get; set; }
        public string? OrganizationName { get; set; }
        public string? InvitedByUserName { get; set; }
        public InviteStatus Status { get; set; }
        public DateTime? RespondedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime RegistrationDate { get; set; }
    }
}
