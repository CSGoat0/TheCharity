
using TheCharityDAL.Enums;

namespace TheCharityBLL.DTOs.OrganizationContactMethodDTOs
{
    public class OrgContactMethodResponseDto
    {
        /// <example>10</example>
        public int Id { get; set; }

        /// <example>info@egyptianfoodbank.org</example>
        public string Value { get; set; } = null!;

        /// <example>Email</example>
        public ContactType Type { get; set; }

        /// <example>1</example>
        public int CompanyId { get; set; }

        /// <example>false</example>
        public bool IsDeleted { get; set; }

        /// <example>2025-01-15T10:30:00</example>
        public DateTime RegistrationDate { get; set; }

        /// <example>2025-06-20T14:45:00</example>
        public DateTime? UpdatedOn { get; set; }
    }
}
