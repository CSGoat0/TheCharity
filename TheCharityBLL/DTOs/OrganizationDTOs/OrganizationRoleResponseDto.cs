using TheCharityDAL.Enums;

namespace TheCharityBLL.DTOs.OrganizationDTOs
{
    public class OrganizationRoleResponseDto
    {
        public int Id { get; set; }
        public int OrganizationId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public OrganizationRoleType Role { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public string? UserFullName { get; set; }
    }
}
