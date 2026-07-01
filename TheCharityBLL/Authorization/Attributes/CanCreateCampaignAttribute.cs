using Microsoft.AspNetCore.Mvc;
using TheCharityBLL.Authorization.Filters;

namespace TheCharityBLL.Authorization.Attributes
{
    /// <summary>
    /// Allows access to users who can create campaigns for the specified organization.
    /// This includes SuperAdmin, OrganizationAdmin, and SubAdmin.
    /// </summary>
    public class CanCreateCampaignAttribute : ServiceFilterAttribute
    {
        public CanCreateCampaignAttribute()
            : base(typeof(CanCreateCampaignFilter))
        {
        }
    }
}
