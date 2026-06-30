using TheCharityDAL.Entities;

namespace TheCharityBLL.Events.Events.InvitationEvents
{
    public class InviteAcceptedEvent
    {
        public SharedCampaignInvite Invite { get; set; } = null!;
        public SharedCampaign Campaign { get; set; } = null!;
        public Organization InvitedOrganization { get; set; } = null!;
        public User AcceptedByUser { get; set; } = null!;
    }
}
