using TheCharityDAL.Entities;

namespace TheCharityBLL.Events.CampaignEvents
{
    public class CampaignDismissedEvent
    {
        public Campaign Campaign { get; set; } = null!;
    }
}
