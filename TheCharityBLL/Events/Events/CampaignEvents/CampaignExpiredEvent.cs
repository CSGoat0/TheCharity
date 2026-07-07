using TheCharityDAL.Entities;

namespace TheCharityBLL.Events.Events.CampaignEvents
{
    public class CampaignExpiredEvent
    {
        public Campaign Campaign { get; set; } = null!;
    }
}
