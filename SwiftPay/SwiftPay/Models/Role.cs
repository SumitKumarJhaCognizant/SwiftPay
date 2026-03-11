using System;
using SwiftPay.Constants.Enums;

namespace Model
{
    public class Role
    {
        public int RoleId { get; set; }

        public RoleType RoleType { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Navigation
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
