using System.ComponentModel.DataAnnotations;

namespace TheCharityBLL.DTOs.UserDTOs
{
    public class CreateUserDTO
    {
        [Required(ErrorMessage = "Username is required.")]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters.")]
        [MaxLength(50, ErrorMessage = "Username cannot exceed 50 characters.")]
        [RegularExpression(@"^[A-Za-z0-9._-]+$",
            ErrorMessage = "Username can only contain letters, digits, dots, underscores, and hyphens.")]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "Full name is required.")]
        [MaxLength(200, ErrorMessage = "Full name cannot exceed 200 characters.")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [MaxLength(256, ErrorMessage = "Email cannot exceed 256 characters.")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters.")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters.")]
        public string? Password { get; set; }

        [MaxLength(500, ErrorMessage = "Image path cannot exceed 500 characters.")]
        public string? ImgPath { get; set; }

        [MaxLength(500, ErrorMessage = "Address cannot exceed 500 characters.")]
        public string? Address { get; set; }
    }
}