using TheCharityDAL.Entities;

namespace TheCharityBLL.Events.Events.InvitationEvents
{
    public class InviteSentEvent
    {
        public SharedCampaignInvite Invite { get; set; } = null!;
        public SharedCampaign Campaign { get; set; } = null!;
        public Organization InvitedOrganization { get; set; } = null!;
        public User InvitedByUser { get; set; } = null!;
    }
}
