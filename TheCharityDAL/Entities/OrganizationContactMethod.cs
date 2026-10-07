using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations.Schema;
using TheCharityDAL.Enums;

namespace TheCharityDAL.Entities
{
    public class OrganizationContactMethod
    {
        public int Id { get; private set; }
        public string? Value { get; private set; }
        public ContactType? Type { get; private set; }
        public bool IsDeleted { get; private set; } = false;
        public DateTime? DeletedOn { get; private set; }
        public DateTime? RegistrationDate { get; private set; } = DateTime.UtcNow;
        public DateTime? UpdatedOn { get; private set; }
        public int? OrganizationId { get; private set; }

        [ForeignKey(nameof(OrganizationId))]
        public Organization? organization { get; private set; }
        public OrganizationContactMethod(string? value, ContactType? type, int? organizationId) { 
            this.Value = value;
            this.Type = type;
            this.OrganizationId = organizationId;
        }
        public void EditValue(string? value) {
            if (!value.IsNullOrEmpty())
            {
                this.Value = value;
                this.UpdatedOn = DateTime.UtcNow;
            }
        }
        public void EditType(ContactType? type) {
            if (type.HasValue)
            {
                this.Type = type;
                this.UpdatedOn = DateTime.UtcNow;
            }
        }
        public void Delete() {
            this.IsDeleted = true;
            this.DeletedOn = DateTime.UtcNow;
        }
        public void Restore() { 
            this.IsDeleted = false;
            this.UpdatedOn = DateTime.UtcNow; ;
        }
    }
}
