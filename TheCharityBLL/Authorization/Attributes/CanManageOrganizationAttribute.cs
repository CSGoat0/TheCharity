using Microsoft.AspNetCore.Authorization;

namespace TheCharityBLL.Authorization.Attributes
{
    /// <summary>
    /// Allows access to users who can manage the specified organization.
    /// This includes SuperAdmin and OrganizationAdmin of that organization.
    /// </summary>
    public class CanManageOrganizationAttribute : AuthorizeAttribute
    {
        public CanManageOrganizationAttribute()
        {
            Policy = "CanManageOrganization";
        }
    }
}
