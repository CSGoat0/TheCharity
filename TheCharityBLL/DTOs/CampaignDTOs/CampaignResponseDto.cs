using TheCharityDAL.Enums;

namespace TheCharityBLL.DTOs.CampaignDTOs
{
    public class CampaignResponseDto
    {
        /// <example>1</example>
        public int Id { get; set; }
        /// <example>1</example>
        public int OrganizationId { get; set; }
        /// <example>Food for 1000 Families</example>
        public string? Title { get; set; }
        /// <example>Providing food packages to underprivileged families during Ramadan</example>
        public string? Description { get; set; }
        /// <example>/images/food_campaign.jpg</example>
        public string? ImgPath { get; set; }
        /// <example>100000</example>
        public double? Target { get; set; }
        /// <example>30000</example>
        public double? Achieved { get; set; }
        /// <example>Active</example>
        public CampaignStatus? Status { get; set; }
        /// <example>Solo</example>
        public CampaignType? Type { get; set; }
        /// <example>false</example>
        public bool IsDeleted { get; set; }
        /// <example>2024-02-01T08:00:00</example>
        public DateTime? RegistrationDate { get; set; }
        /// <example>2024-02-15T14:30:00</example>
        public DateTime? UpdatedOn { get; set; }
        /// <example>2024-04-30T23:59:59</example>
        public DateTime? Deadline { get; set; }
        /// <example>15</example>
        public int? DaysRemaining { get; set; }
    }
}
