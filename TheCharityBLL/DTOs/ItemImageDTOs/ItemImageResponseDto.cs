using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheCharityBLL.DTOs.ItemImageDTOs
{
    public class ItemImageResponseDto
    {
        /// <example>8</example>
        public int Id { get; set; }

        /// <example>DonatedItems/Images/wheelchair-01.jpg</example>
        public string Path { get; set; } = null!;

        /// <example>15</example>
        public int DonatedItemId { get; set; }

        /// <example>true</example>
        public bool IsMain { get; set; }

        /// <example>2025-08-01T10:30:00</example>
        public DateTime RegistrationDate { get; set; }

        /// <example>2025-08-05T14:20:00</example>
        public DateTime? UpdatedOn { get; set; }

        /// <example>false</example>
        public bool IsDeleted { get; set; }
    }
}
