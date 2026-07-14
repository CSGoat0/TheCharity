using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheCharityBLL.DTOs.UserResponseDTOs
{
    public class ConfirmEmailResponseDto
    {
        public string email { get; set; }
        public string encodedToken { get; set; }
    }
}
