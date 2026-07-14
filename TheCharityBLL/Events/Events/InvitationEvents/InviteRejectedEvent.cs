using TheCharityDAL.Entities;

namespace TheCharityBLL.Events.Events.InvitationEvents
{
    public class InviteRejectedEvent
    {
        public SharedCampaignInvite Invite { get; set; } = null!;
        public SharedCampaign Campaign { get; set; } = null!;
        public Organization InvitedOrganization { get; set; } = null!;
        public User RejectedByUser { get; set; } = null!;
    }
}
