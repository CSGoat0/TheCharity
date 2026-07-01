using TheCharityDAL.Entities;
using TheCharityDAL.Enums;

namespace TheCharityBLL.Events.CampaignEvents
{
    public class CampaignStatusChangedEvent
    {
        public Campaign Campaign { get; set; } = null!;
        public CampaignStatus OldStatus { get; set; }
        public CampaignStatus NewStatus { get; set; }
    }
}
