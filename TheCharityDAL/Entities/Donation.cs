using System.ComponentModel.DataAnnotations.Schema;

namespace TheCharityDAL.Entities
{
    public class Donation
    {
        public int Id { get; private set; }
        public double? Amount { get; private set; }
        public string UserId { get; private set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; private set; }
        public int CampaignId { get; private set; }

        [ForeignKey(nameof(CampaignId))]
        public Campaign? Campaign { get; private set; }
        public bool IsDeleted { get; private set; } = false;
        public DateTime? DeletedOn { get; private set; }
        public DateTime RegistrationDate { get; private set; } = DateTime.Now;
        public DateTime? UpdatedOn { get; private set; }
        public Donation(double? amount, string userId, int campaignId)
        {
            this.Amount = amount;
            this.UserId = userId;
            this.CampaignId = campaignId;
        }
        public void EditAmount(double? amount)
        {
            if (amount.HasValue)
            {
                this.Amount = amount;
                this.UpdatedOn = DateTime.Now;
            }
        }
        public void EditCampaign(int campaignId)
        {
            this.CampaignId = campaignId;
            this.UpdatedOn = DateTime.Now;
        }
        public void Delete()
        {
            this.IsDeleted = true;
            this.DeletedOn = DateTime.Now;
        }
        public void Restore()
        {
            this.IsDeleted = false;
            this.UpdatedOn = DateTime.Now;
        }
    }
}
