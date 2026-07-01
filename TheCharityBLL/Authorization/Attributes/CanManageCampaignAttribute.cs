using Microsoft.AspNetCore.Authorization;

namespace TheCharityBLL.Authorization.Attributes
{
    /// <summary>
    /// Allows access to users who can manage the specified campaign.
    /// This includes SuperAdmin, OrganizationAdmin, and SubAdmin of the campaign's organization.
    /// </summary>
    public class CanManageCampaignAttribute : AuthorizeAttribute
    {
        public CanManageCampaignAttribute()
        {
            Policy = "CanManageCampaign";
        }
    }
}
