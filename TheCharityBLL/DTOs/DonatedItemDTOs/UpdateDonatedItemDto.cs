using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheCharityDAL.Enums;

namespace TheCharityBLL.DTOs.DonatedItemDTOs
{
    public class UpdateDonatedItemDto
    {

        [MaxLength(200, ErrorMessage = "Name cannot exceed 200 characters.")]
        /// <example>Winter Jacket</example>
        public string? Name { get; set; }

        [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
        /// <example>Lightweight winter jacket in excellent condition.</example>
        public string? Description { get; set; }

        [EnumDataType(typeof(ItemCategory), ErrorMessage = "Invalid item category.")]
        /// <example>Clothes</example>
        public ItemCategory? ItemCategory { get; set; }

        /// <example>true</example>
        public bool? IsAvailable { get; set; }

        /// <example>2</example>
        public int? OrganizationId { get; set; }

    }
}
