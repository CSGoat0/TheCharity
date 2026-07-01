namespace TheCharityBLL.DTOs.CampaignDTOs
{
    public class CreateSharedCampaignDto : CreateCampaignDto
    {
        /// <example>[1, 2, 3]</example>
        public List<int> OrganizationIds { get; set; } = new List<int>();
        /// <example>/images/shared_campaign.jpg</example>
        public string? ImgPath { get; set; }
        /// <example>100000</example>
        public int? Target { get; set; }
        public int CreatorOrganizationId { get; set; }
    }
}
