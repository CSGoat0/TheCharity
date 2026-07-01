using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheCharityDAL.Entities
{
    public class Organization
    {
        public int Id { get; private set; }
        public string? Name { get; private set; }
        public string? Address { get; private set; }
        public int? PaymentId { get; private set; }
        public string? AdminUserId { get; private set; }

        [ForeignKey(nameof(AdminUserId))]
        public User? AdminUser { get; private set; }
        public virtual ICollection<OrganizationRole> OrganizationRoles { get; private set; } = new List<OrganizationRole>();

        [ForeignKey(nameof(PaymentId))]
        public PaymentInfo? PaymentInfo { get; private set; }
        public virtual ICollection<SoloCampaign> SoloCampaigns { get; set; } = new List<SoloCampaign>();
        public virtual ICollection<SharedCampaign> SharedCampaigns { get; set; } = new List<SharedCampaign>();

        [NotMapped]
        public IEnumerable<Campaign> Campaigns =>
            SoloCampaigns.Cast<Campaign>().Concat(SharedCampaigns.Cast<Campaign>());
        public virtual ICollection<OrganizationContactMethod> ContactMethods { get; private set; } = new List<OrganizationContactMethod>();
        public bool IsDeleted { get; private set; } = false;
        public DateTime? DeletedOn { get; private set; }
        public DateTime? RegistrationDate { get; private set; } = DateTime.UtcNow;
        public DateTime? UpdatedOn { get; private set; }
        public Organization(string? name, string? address)
        {
            this.Name = name;
            this.Address = address;
            this.PaymentId = null;
        }
        public void EditName(string? name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                this.Name = name;
                this.UpdatedOn = DateTime.UtcNow;
            }
        }
        public void EditAddress(string? address)
        {
            if (!string.IsNullOrEmpty(address))
            {
                this.Address = address;
                this.UpdatedOn = DateTime.UtcNow;
            }
        }
        public void EditPaymentInfoId(int? paymentinfoId)
        {
            if (paymentinfoId>0 || paymentinfoId==null)
            {
                this.PaymentId = paymentinfoId;
                this.UpdatedOn = DateTime.UtcNow;
            }
        }
        public void AssignAdmin(string adminUserId)
        {
            if (!string.IsNullOrEmpty(adminUserId))
            {
                AdminUserId = adminUserId;
                UpdatedOn = DateTime.UtcNow;
            }
        }
        public void RemoveAdmin()
        {
            AdminUserId = null;
            UpdatedOn = DateTime.UtcNow;
        }
        public void Delete()
        {
            this.IsDeleted = true;
            this.DeletedOn = DateTime.UtcNow;
        }
        public void Restore()
        {
            this.IsDeleted = false;
            this.UpdatedOn = DateTime.UtcNow;
        }
    }
}
