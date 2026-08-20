using TheCharityDAL.Enums;

namespace TheCharityBLL.DTOs.CampaignDTOs
{
    public class CreateCampaignDto
    {
        /// <example>Help the Children</example>
        public string? Title { get; set; }
        /// <example>Providing education for underprivileged children</example>
        public string? Description { get; set; }
        /// <example>50000</example>
        public double Target { get; set; }
        /// <example>2024-03-01T00:00:00</example>
        public DateTime? StartDate { get; set; }
        /// <example>2024-05-01T00:00:00</example>
        public DateTime? Deadline { get; set; }
    }
}
