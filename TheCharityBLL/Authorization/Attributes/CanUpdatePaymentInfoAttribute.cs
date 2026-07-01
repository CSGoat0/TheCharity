using Microsoft.AspNetCore.Authorization;

namespace TheCharityBLL.Authorization.Attributes
{
    /// <summary>
    /// Allows access to users who can update payment info for the organization.
    /// This includes SuperAdmin and OrganizationAdmin (NOT SubAdmin).
    /// </summary>
    public class CanUpdatePaymentInfoAttribute : AuthorizeAttribute
    {
        public CanUpdatePaymentInfoAttribute()
        {
            Policy = "CanUpdatePaymentInfo";
        }
    }
}
