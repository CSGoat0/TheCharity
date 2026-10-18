using System.ComponentModel.DataAnnotations;

namespace TheCharityBLL.DTOs.OrganizationDTOs
{
    public class AssignAdminRequest
    {
        [Required(ErrorMessage = "UserId is required.")]
        [MaxLength(450, ErrorMessage = "UserId cannot exceed 450 characters.")]
        public string UserId { get; set; } = string.Empty;
    }
}