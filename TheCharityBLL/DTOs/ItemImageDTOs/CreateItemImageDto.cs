using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheCharityBLL.DTOs.ItemImageDTOs
{
    public class CreateItemImageDto
    {
        /// <example>DonatedItems/Images/DonatedItem-15.jpg</example>
        [Required(ErrorMessage = "Path is required.")]
        [MaxLength(1000, ErrorMessage = "Path cannot exceed 1000 characters.")]
        public string Path { get; set; } = null!;

        public IFormFile ImageUrl { get; set; } = null!;

        /// <example>15</example>
        [Required(ErrorMessage = "DonatedItem id is required.")]
        public int DonatedItemId { get; set; }
    }
}
