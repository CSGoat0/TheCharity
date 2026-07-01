using TheCharityDAL.Entities;

namespace TheCharityBLL.Events.CampaignEvents
{
    public class CampaignCreatedEvent
    {
        public Campaign Campaign { get; set; } = null!;
    }
}
