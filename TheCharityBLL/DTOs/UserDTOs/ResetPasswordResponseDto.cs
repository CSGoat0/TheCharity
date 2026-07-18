namespace TheCharityBLL.DTOs.UserDTOs
{
    public class ResetPasswordResponseDto
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Token { get; set; }
    }
}
