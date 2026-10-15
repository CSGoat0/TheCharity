using Microsoft.AspNetCore.Mvc;
using TheCharityBLL.Authorization.Filters;

namespace TheCharityBLL.Authorization.Attributes
{
    /// <summary>
    /// Allows access to users who can manage sub-admins for the target organization.
    /// This includes SuperAdmin and OrganizationAdmin (NOT SubAdmin).
    ///
    /// Implemented as a filter because the organization id may only be available
    /// in the request body (or must be resolved from a related resource).
    /// </summary>
    public class CanManageSubAdminsAttribute : ServiceFilterAttribute
    {
        public CanManageSubAdminsAttribute()
            : base(typeof(CanManageSubAdminsFilter))
        {
        }
    }
}