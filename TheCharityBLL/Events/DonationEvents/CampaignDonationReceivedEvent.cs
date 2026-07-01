namespace TheCharityBLL.Events.DonationEvents
{
    public class CampaignDonationReceivedEvent
    {
        public int CampaignId { get; set; }
        public double Amount { get; set; }
    }
}
