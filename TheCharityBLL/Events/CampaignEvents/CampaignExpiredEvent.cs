using TheCharityDAL.Entities;

namespace TheCharityBLL.Events.CampaignEvents
{
    public class CampaignExpiredEvent
    {
        public Campaign Campaign { get; set; } = null!;
    }
}
