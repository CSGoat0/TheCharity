namespace TheCharityBLL.DTOs.CampaignDTOs
{
    public class SharedCampaignResponseDto : CampaignResponseDto
    {
        public List<OrganizationBasicDto>? Organizations { get; set; }
        /// <example>3</example>
        public int OrganizationsCount { get; set; }
        public int CreatorOrganizationId { get; set; }
        public string CreatorOrganizationName { get; set; }
    }
}
