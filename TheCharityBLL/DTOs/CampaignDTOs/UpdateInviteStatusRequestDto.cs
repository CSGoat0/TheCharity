using System.ComponentModel.DataAnnotations;
using TheCharityDAL.Enums;

namespace TheCharityBLL.DTOs.CampaignDTOs
{
    public class UpdateInviteStatusRequestDto
    {
        [EnumDataType(typeof(InviteStatus), ErrorMessage = "Invalid invite status.")]
        public InviteStatus Status { get; set; } // Accept or Reject
    }
}