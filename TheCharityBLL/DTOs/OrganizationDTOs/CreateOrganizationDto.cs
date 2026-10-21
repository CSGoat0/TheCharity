using System.ComponentModel.DataAnnotations;

namespace TheCharityBLL.DTOs.OrganizationDTOs
{
    public class CreateOrganizationDto
    {
        /// <example>Egyptian Food Bank</example>
        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(200, ErrorMessage = "Name cannot exceed 200 characters.")]
        public string Name { get; set; } = null!;

        /// <example>Cairo, Egypt</example>
        [MaxLength(500, ErrorMessage = "Address cannot exceed 500 characters.")]
        public string? Address { get; set; }

        /// <example>https://example.com/logos/foodbank.png</example>
        [Url(ErrorMessage = "ImgPath must be a valid URL.")]
        [MaxLength(500, ErrorMessage = "ImgPath cannot exceed 500 characters.")]
        public string? ImgPath { get; set; }
    }
}