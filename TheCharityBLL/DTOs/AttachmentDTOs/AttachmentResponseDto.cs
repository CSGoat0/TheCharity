using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheCharityBLL.DTOs.AttachmentDTOs
{
    public class AttachmentResponseDto
    {
        /// <example>15</example>
        public int Id { get; set; }

        /// <example>National ID.pdf</example>
        public string? Name { get; set; }

        /// <example>DonatedItems/Attachments/national-id.pdf</example>
        public string Path { get; set; }

        /// <example>254800</example>
        public long? FileSize { get; set; }

        /// <example>application/pdf</example>
        public string? ContentType { get; set; }

        /// <example>true</example>
        public bool IsItemAttachment { get; set; }

        /// <example>8</example>
        public int DonatedItemId { get; set; }

        /// <example>2025-08-10T09:30:00</example>
        public DateTime RegistrationDate { get; set; }

        /// <example>2025-08-12T14:15:00</example>
        public DateTime? UpdatedOn { get; set; }

        /// <example>false</example>
        public bool IsDeleted { get; set; }

    }
}
