using TheCharityDAL.Enums;

namespace TheCharityBLL.DTOs.CampaignDTOs
{
    public class UpdateInviteStatusRequestDto
    {
        public InviteStatus Status { get; set; } // Accept or Reject
    }
}
