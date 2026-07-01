using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheCharityDAL.Enums;

namespace TheCharityBLL.DTOs.DonatedItemDTOs
{
    public class CreateDonatedItemDto
    {
        [Required(ErrorMessage = "DonorId is required.")]
        /// <example>c3f8d6b4-8a21-4c4f-9d8d-7d5f8b1c2a34</example>
        public string DonorId { get; set; } = null!;

        [Required(ErrorMessage = "OrganizationId is required.")]
        /// <example>1</example>
        public int OrganizationId { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(200, ErrorMessage = "Name cannot exceed 200 characters.")]
        /// <example>Winter Jacket</example>
        public string Name { get; set; } = null!;

        [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
        /// <example>Lightweight winter jacket in excellent condition.</example>
        public string? Description { get; set; }

        [Required(ErrorMessage = "Item category is required.")]
        [EnumDataType(typeof(ItemCategory), ErrorMessage = "Invalid item category.")]
        /// <example>Clothes</example>
        public ItemCategory ItemCategory { get; set; }

        [MaxLength(5, ErrorMessage = "Can not upload more than 5 images")]
        /// <example>[winter-jacket-front.jpg, winter-jacket-side.jpg]</example>
        public List<IFormFile>? ImageFiles { get; set; }

        [MaxLength(5, ErrorMessage = "Can not upload more than 5 attachment")]
        /// <example>[invoice.pdf, warranty.pdf]</example>
        public List<IFormFile>? AttachmentFiles { get; set; }

    }

}
