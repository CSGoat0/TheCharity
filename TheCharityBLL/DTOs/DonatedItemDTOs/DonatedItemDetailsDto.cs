
using TheCharityBLL.DTOs.AttachmentDTOs;
using TheCharityBLL.DTOs.DonorDtos;
using TheCharityBLL.DTOs.ItemImageDTOs;
using TheCharityBLL.DTOs.OrganizationDTOs;
using TheCharityDAL.Enums;

namespace TheCharityBLL.DTOs.DonatedItemDTOs
{
    public class DonatedItemDetailsDto
    {
        /// <example>15</example>
        public int Id { get; set; }

        /// <example>Winter Jacket</example>
        public string Name { get; set; } = null!;

        /// <example>Lightweight winter jacket in excellent condition.</example>
        public string? Description { get; set; }

        /// <example>Clothes</example>
        public ItemCategory ItemCategory { get; set; }

        /// <example>true</example>
        public bool IsAvailable { get; set; }

        /// <example>1</example>
        public int OrganizationId { get; set; }

        public OrganizationResponseDto? Organization { get; set; }

        /// <example>c3f8d6b4-8a21-4c4f-9d8d-7d5f8b1c2a34</example>
        public string DonorId { get; set; } = null!;

        public DonorResponseDto? Donor { get; set; }

        public List<ItemImageResponseDto>? Images { get; set; }

        public List<AttachmentResponseDto>? ItemAttachments { get; set; }

        public List<AttachmentResponseDto>? RecipientAttachments { get; set; }

        /// <example>2025-08-01T10:30:00</example>
        public DateTime RegistrationDate { get; set; }

        /// <example>2025-08-05T15:20:00</example>
        public DateTime? UpdatedOn { get; set; }

        /// <example>false</example>
        public bool IsDeleted { get; set; }
    }
}
