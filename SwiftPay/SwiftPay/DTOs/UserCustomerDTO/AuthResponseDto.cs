using System;
using System.Collections.Generic;
using SwiftPay.DTOs.UserRoleDTO;

namespace SwiftPay.DTOs.UserCustomerDTO
{
    public class AuthResponseDto
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public ICollection<UserRoleDto> Roles { get; set; } = new List<UserRoleDto>();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
