
using TheCharityBLL.DTOs.AttachmentDTOs;
using TheCharityBLL.DTOs.ItemImageDTOs;
using TheCharityDAL.Enums;

namespace TheCharityBLL.DTOs.DonatedItemDTOs
{
    public class DonatedItemResponseDto
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

        /// <example>Egyptian Food Bank</example>
        public string OrganizationName { get; set; } = null!;

        /// <example>c3f8d6b4-8a21-4c4f-9d8d-7d5f8b1c2a34</example>
        public string DonorId { get; set; } = null!;

        /// <example>abdallah_ayman</example>
        public string DonorName { get; set; } = null!;

        public ICollection<ItemImageResponseDto>? Images { get; set; } = new List<ItemImageResponseDto>();

        public ICollection<AttachmentResponseDto>? ItemAttachments { get; set; } = new List<AttachmentResponseDto>();

        public ICollection<AttachmentResponseDto>? RecipientAttachments { get; set; } = new List<AttachmentResponseDto>();

        /// <example>2025-08-01T10:30:00</example>
        public DateTime RegistrationDate { get; set; }

        /// <example>2025-08-05T15:20:00</example>
        public DateTime? UpdatedOn { get; set; }

        /// <example>false</example>
        public bool IsDeleted { get; set; }

    }
}
