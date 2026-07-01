using Microsoft.AspNetCore.Authorization;

namespace TheCharityBLL.Authorization.Attributes
{
    /// <summary>
    /// Allows access only to SuperAdmin users.
    /// </summary>
    public class IsSuperAdminAttribute : AuthorizeAttribute
    {
        public IsSuperAdminAttribute()
        {
            Policy = "IsSuperAdmin";
        }
    }
}
