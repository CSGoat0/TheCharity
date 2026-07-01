using System.ComponentModel.DataAnnotations.Schema;
using TheCharityDAL.Enums;

namespace TheCharityDAL.Entities
{
    public class SharedCampaignInvite
    {
        public int Id { get; private set; }
        public int SharedCampaignId { get; private set; }
        public int OrganizationId { get; private set; }
        public string InvitedByUserId { get; private set; }
        public InviteStatus Status { get; private set; } = InviteStatus.Pending;
        public DateTime? RespondedAt { get; private set; }
        public DateTime ExpiresAt { get; private set; }
        public bool IsDeleted { get; private set; } = false;
        public DateTime? DeletedOn { get; private set; }
        public DateTime RegistrationDate { get; private set; } = DateTime.UtcNow;
        public DateTime? UpdatedOn { get; private set; }

        [ForeignKey(nameof(SharedCampaignId))]
        public SharedCampaign? SharedCampaign { get; private set; }

        [ForeignKey(nameof(OrganizationId))]
        public Organization? Organization { get; private set; }

        [ForeignKey(nameof(InvitedByUserId))]
        public User? InvitedByUser { get; private set; }

        public SharedCampaignInvite(int sharedCampaignId, int organizationId, string invitedByUserId, DateTime expiresAt)
        {
            SharedCampaignId = sharedCampaignId;
            OrganizationId = organizationId;
            InvitedByUserId = invitedByUserId;
            ExpiresAt = expiresAt;
        }

        public void Accept()
        {
            Status = InviteStatus.Accepted;
            RespondedAt = DateTime.UtcNow;
            UpdatedOn = DateTime.UtcNow;
        }

        public void Reject()
        {
            Status = InviteStatus.Rejected;
            RespondedAt = DateTime.UtcNow;
            UpdatedOn = DateTime.UtcNow;
        }

        public void Expire()
        {
            Status = InviteStatus.Expired;
            UpdatedOn = DateTime.UtcNow;
        }

        public void Delete()
        {
            IsDeleted = true;
            DeletedOn = DateTime.UtcNow;
        }

        public void Restore()
        {
            IsDeleted = false;
            UpdatedOn = DateTime.UtcNow;
        }
    }
}
