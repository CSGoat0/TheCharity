using Microsoft.AspNetCore.Authorization;

namespace TheCharityBLL.Authorization.Attributes
{
    /// <summary>
    /// Allows access to users who can manage sub-admins for the organization.
    /// This includes SuperAdmin and OrganizationAdmin (NOT SubAdmin).
    /// </summary>
    public class CanManageSubAdminsAttribute : AuthorizeAttribute
    {
        public CanManageSubAdminsAttribute()
        {
            Policy = "CanManageSubAdmins";
        }
    }
}
