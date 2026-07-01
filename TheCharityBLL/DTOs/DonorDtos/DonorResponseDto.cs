
namespace TheCharityBLL.DTOs.DonorDtos
{
    public class DonorResponseDto
    {
        /// <example>c3f8d6b4-8a21-4c4f-9d8d-7d5f8b1c2a34</example>
        public string? Id { get; set; }

        /// <example>abdallah_ayman</example>
        public string? UserName { get; set; }

        /// <example>abdallah.ayman@example.com</example>
        public string? Email { get; set; }

        /// <example>+201012345678</example>
        public string? PhoneNumber { get; set; }

        /// <example>Nasr City, Cairo, Egypt</example>
        public string? Address { get; set; }
    }
}
