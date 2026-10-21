using System.ComponentModel.DataAnnotations;

namespace TheCharityBLL.DTOs.OrganizationDTOs
{
    public class UpdateOrganizationDto
    {
        /// <example>Resala Charity Association</example>
        [MaxLength(200, ErrorMessage = "Name cannot exceed 200 characters.")]
        public string? Name { get; set; }

        /// <example>Nasr City, Cairo</example>
        [MaxLength(500, ErrorMessage = "Address cannot exceed 500 characters.")]
        public string? Address { get; set; }

        /// <example>https://example.com/logos/foodbank.png</example>
        [Url(ErrorMessage = "ImgPath must be a valid URL.")]
        [MaxLength(500, ErrorMessage = "ImgPath cannot exceed 500 characters.")]
        public string? ImgPath { get; set; }
    }
}
