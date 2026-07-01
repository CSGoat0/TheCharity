using TheCharityDAL.Entities;

namespace TheCharityBLL.Events.CampaignEvents
{
    public class CampaignCompletedEvent
    {
        public Campaign Campaign { get; set; } = null!;
    }
}
