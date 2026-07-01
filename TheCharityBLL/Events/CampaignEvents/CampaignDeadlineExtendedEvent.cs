using TheCharityDAL.Entities;

namespace TheCharityBLL.Events.CampaignEvents
{
    public class CampaignDeadlineExtendedEvent
    {
        public Campaign Campaign { get; set; } = null!;
        public DateTime NewDeadline { get; set; }
    }
}
