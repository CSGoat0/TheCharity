using System.ComponentModel.DataAnnotations.Schema;
using TheCharityDAL.Enums;

namespace TheCharityDAL.Entities
{
    public class OrganizationRole
    {
        public int Id { get; private set; }
        public int OrganizationId { get; private set; }
        public string UserId { get; private set; }
        public OrganizationRoleType Role { get; private set; }
        public bool IsDeleted { get; private set; } = false;
        public DateTime? DeletedOn { get; private set; }
        public DateTime RegistrationDate { get; private set; } = DateTime.UtcNow;
        public DateTime? UpdatedOn { get; private set; }

        [ForeignKey(nameof(OrganizationId))]
        public Organization? Organization { get; private set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; private set; }

        public OrganizationRole(int organizationId, string userId, OrganizationRoleType role)
        {
            OrganizationId = organizationId;
            UserId = userId;
            Role = role;
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
