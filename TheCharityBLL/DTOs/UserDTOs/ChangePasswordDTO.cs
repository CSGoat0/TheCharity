using System.ComponentModel.DataAnnotations;

namespace TheCharityBLL.DTOs.UserDTOs
{
    public class ChangePasswordDTO
    {
        [Required(ErrorMessage = "Current password is required.")]
        [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters.")]
        public string? CurrentPassword { get; set; }

        [Required(ErrorMessage = "New password is required.")]
        [MinLength(6, ErrorMessage = "New password must be at least 6 characters.")]
        [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters.")]
        public string? NewPassword { get; set; }
    }
}