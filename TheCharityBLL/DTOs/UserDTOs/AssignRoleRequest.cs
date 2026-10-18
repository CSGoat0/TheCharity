using System.ComponentModel.DataAnnotations;

namespace TheCharityBLL.DTOs.UserDTOs
{
    public class AssignRoleRequest
    {
        [Required(ErrorMessage = "Role is required.")]
        [MaxLength(100, ErrorMessage = "Role name cannot exceed 100 characters.")]
        public string Role { get; set; } = string.Empty;
    }
}