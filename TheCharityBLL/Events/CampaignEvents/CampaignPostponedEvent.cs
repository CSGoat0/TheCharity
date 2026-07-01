using TheCharityDAL.Entities;

namespace TheCharityBLL.Events.CampaignEvents
{
    public class CampaignPostponedEvent
    {
        public Campaign Campaign { get; set; } = null!;
    }
}
