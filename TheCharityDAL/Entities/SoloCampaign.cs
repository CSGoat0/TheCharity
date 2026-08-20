using System.ComponentModel.DataAnnotations.Schema;
using TheCharityDAL.Enums;

namespace TheCharityDAL.Entities
{
    public class SoloCampaign: Campaign
    {
        public Organization? Organization { get; private set; }
        public SoloCampaign(string? title, string? description, string? imgPath, int? target, int? achieved, CampaignStatus? status, DateTime deadline, int? organizationId) : base(title, description, imgPath, target, achieved, status, deadline)
        {
            this.OrganizationId = organizationId;
        }
        private SoloCampaign() { }
    }
}
