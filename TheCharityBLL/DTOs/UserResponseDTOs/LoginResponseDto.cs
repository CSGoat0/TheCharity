using System.ComponentModel.DataAnnotations;

namespace TheCharityBLL.DTOs.UserResponseDTOs
{
    public class LoginResponseDto
    {
        [Required(ErrorMessage = "Username is required")]
        [Display(Name = "Username")]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }
}
