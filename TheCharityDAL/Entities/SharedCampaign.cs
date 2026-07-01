using System.ComponentModel.DataAnnotations.Schema;
using TheCharityDAL.Enums;

namespace TheCharityDAL.Entities
{
    public class SharedCampaign: Campaign
    {
        public ICollection<Organization>? Organizations { get; private set; } = new List<Organization>();
        public int CreatorOrganizationId { get; private set; }

        [ForeignKey(nameof(CreatorOrganizationId))]
        public Organization? CreatorOrganization { get; private set; }
        public SharedCampaign(string? title, string? description, string? imgPath, int? target, int? achieved, CampaignStatus? status, CampaignType? type, DateTime deadline, int creatorOrganizationId) : base(title, description, imgPath, target, achieved, status, type, deadline)
        {
            CreatorOrganizationId = creatorOrganizationId;
        }
        private SharedCampaign() { }
        public void AddOrganization(Organization? organization)
        {
            if (!(organization == null))
            {
                this.Organizations?.Add(organization);
            }
        }
        public void RemoveOrganization(Organization? organization)
        {
            if (!(organization == null))
            {
                this.Organizations?.Remove(organization);
            }
        }
    }
}
