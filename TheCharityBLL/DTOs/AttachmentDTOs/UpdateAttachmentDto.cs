using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheCharityBLL.DTOs.AttachmentDTOs
{
    public class UpdateAttachmentDto
    {

        /// <example>Updated National ID.pdf</example>
        [MaxLength(255, ErrorMessage = "Name cannot exceed 255 characters.")]
        public string? Name { get; set; }

        /// <example>DonatedItems/Attachments/updated-national-id.pdf</example>
        [MaxLength(1000, ErrorMessage = "Path cannot exceed 1000 characters.")]
        public string? Path { get; set; }

        /// <example>312450</example>
        [Range(1, long.MaxValue, ErrorMessage = "FileSize must be a positive value.")]
        public long? FileSize { get; set; }

        /// <example>application/pdf</example>
        [MaxLength(100, ErrorMessage = "ContentType cannot exceed 100 characters.")]
        public string? ContentType { get; set; }

    }
}
