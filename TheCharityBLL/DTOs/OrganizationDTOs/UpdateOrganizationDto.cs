using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheCharityBLL.DTOs.OrganizationDTOs
{
    public class UpdateOrganizationDto
    {
        /// <example>Resala Charity Association</example>
        [MaxLength(200, ErrorMessage = "Name cannot exceed 200 characters.")]
        public string? Name { get; set; }

        /// <example>Nasr City, Cairo</example>
        [MaxLength(500, ErrorMessage = "Address cannot exceed 500 characters.")]
        public string? Address { get; set; }

    }
}
