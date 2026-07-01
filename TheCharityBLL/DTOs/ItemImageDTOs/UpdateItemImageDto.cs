using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheCharityBLL.DTOs.ItemImageDTOs
{
    public class UpdateItemImageDto
    {
        /// <example>DonatedItems/Images/wheelchair-updated.jpg</example>
        [Required(ErrorMessage = "Path is required.")]
        [MaxLength(1000, ErrorMessage = "Path cannot exceed 1000 characters.")]
        public string Path { get; set; } = null!;
    }
}
