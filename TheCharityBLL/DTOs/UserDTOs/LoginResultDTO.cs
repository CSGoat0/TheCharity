namespace TheCharityBLL.DTOs.UserDTOs
{
    public class LoginResultDto
    {
        public string Token { get; set; } = string.Empty;
        public UserResponseDTO User { get; set; } = null!;
    }
}
