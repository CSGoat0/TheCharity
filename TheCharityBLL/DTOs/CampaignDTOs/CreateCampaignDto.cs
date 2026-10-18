using System.ComponentModel.DataAnnotations;
using TheCharityDAL.Enums;

namespace TheCharityBLL.DTOs.CampaignDTOs
{
    public class CreateCampaignDto
    {
        /// <example>Help the Children</example>
        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string? Title { get; set; }

        /// <example>Providing education for underprivileged children</example>
        [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }

        /// <example>50000</example>
        [Range(1, 1_000_000_000, ErrorMessage = "Target must be between 1 and 1,000,000,000.")]
        public double Target { get; set; }

        /// <example>2024-03-01T00:00:00</example>
        public DateTime? StartDate { get; set; }

        /// <example>2024-05-01T00:00:00</example>
        [Required(ErrorMessage = "Deadline is required.")]
        public DateTime? Deadline { get; set; }
    }
}