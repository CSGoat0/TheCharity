using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheCharityBLL.DTOs.UserResponseDTOs
{
    public class UserListResponseDto
    {
        public string? Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public bool EmailConfirmed { get; set; }
    }
}
