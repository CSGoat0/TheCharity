using TheCharityDAL.Entities;

namespace TheCharityBLL.Events.Events.CampaignEvents
{
    public class CampaignCompletedEvent
    {
        public Campaign Campaign { get; set; } = null!;
    }
}
