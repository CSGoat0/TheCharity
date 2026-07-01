using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheCharityBLL.DTOs.AttachmentDTOs
{
    public class CreateAttachmentDto
    {
        /// <example>8</example>
        [Required(ErrorMessage = "DonatedItem id is required.")]
        public int DonatedItemId { get; set; }

        /// <example>National ID.pdf</example>
        [MaxLength(255, ErrorMessage = "Name cannot exceed 255 characters.")]
        public string? Name { get; set; }

        /// <example>DonatedItems/Attachments/national-id.pdf</example>
        [Required(ErrorMessage = "Path is required.")]
        [MaxLength(1000, ErrorMessage = "Path cannot exceed 1000 characters.")]
        public string? Path { get; set; }

        /// <example>National ID.pdf</example>
        public IFormFile FileUrl { get; set; } = null!;

        /// <example>254800</example>
        [Range(1, long.MaxValue, ErrorMessage = "FileSize must be a positive value.")]
        public long? FileSize { get; set; }

        /// <example>application/pdf</example>
        [MaxLength(100, ErrorMessage = "ContentType cannot exceed 100 characters.")]
        public string? ContentType { get; set; }

        /// <example>true</example>
        [Required(ErrorMessage = "Is Item is required.")]
        public bool IsItemAttachment { get; set; } = true;
    }
}
