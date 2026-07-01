using Microsoft.AspNetCore.Authorization;

namespace TheCharityBLL.Authorization.Attributes
{
    /// <summary>
    /// Allows access only to SuperAdmin users for bulk operations.
    /// </summary>
    public class CanPerformBulkOperationsAttribute : AuthorizeAttribute
    {
        public CanPerformBulkOperationsAttribute()
        {
            Policy = "CanPerformBulkOperations";
        }
    }
}
