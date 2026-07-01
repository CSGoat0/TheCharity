using Microsoft.AspNetCore.Authorization;

namespace TheCharityBLL.Authorization.Attributes
{
    /// <summary>
    /// Allows access to users who are the creator organization's Admin or SubAdmin of a shared campaign.
    /// This includes SuperAdmin as well.
    /// </summary>
    public class IsSharedCampaignCreatorAttribute : AuthorizeAttribute
    {
        public IsSharedCampaignCreatorAttribute()
        {
            Policy = "IsSharedCampaignCreator";
        }
    }
}
