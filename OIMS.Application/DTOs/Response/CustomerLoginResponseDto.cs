using System;
using System.Collections.Generic;
using System.Text;

namespace OIMS.Application.DTOs.Response
{
    public class CustomerLoginResponseDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "Customer";
    }
}
