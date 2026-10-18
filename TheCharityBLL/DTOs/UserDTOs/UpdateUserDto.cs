using System.ComponentModel.DataAnnotations;

namespace TheCharityBLL.DTOs.UserDTOs
{
    public class UpdateUserDTO
    {
        [Required(ErrorMessage = "User ID is required.")]
        public string? Id { get; set; }

        // Optional on update. If provided, must satisfy the same rules as CreateUserDTO.
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters.")]
        [MaxLength(50, ErrorMessage = "Username cannot exceed 50 characters.")]
        [RegularExpression(@"^[A-Za-z0-9._-]+$",
            ErrorMessage = "Username can only contain letters, digits, dots, underscores, and hyphens.")]
        public string? UserName { get; set; }

        [MaxLength(200, ErrorMessage = "Full name cannot exceed 200 characters.")]
        public string? FullName { get; set; }

        [MaxLength(500, ErrorMessage = "Image path cannot exceed 500 characters.")]
        public string? ImgPath { get; set; }

        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters.")]
        public string? PhoneNumber { get; set; }

        [MaxLength(500, ErrorMessage = "Address cannot exceed 500 characters.")]
        public string? Address { get; set; }
    }
}