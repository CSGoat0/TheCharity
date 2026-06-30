using TheCharityDAL.Entities;

namespace TheCharityBLL.Events.Events.CampaignEvents
{
    public class CampaignCreatedEvent
    {
        public Campaign Campaign { get; set; } = null!;
    }
}
