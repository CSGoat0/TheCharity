using TheCharityDAL.Enums;

namespace TheCharityBLL.DTOs.CampaignDTOs
{
    public class UpdateCampaignDto
    {
        /// <example>1</example>
        public int Id { get; set; }
        /// <example>Updated Campaign Title</example>
        public string? Title { get; set; }
        /// <example>Updated description for the campaign</example>
        public string? Description { get; set; }
        /// <example>/images/updated_campaign.jpg</example>
        public string? ImgPath { get; set; }
        /// <example>150000</example>
        public double? Target { get; set; }
    }
}
