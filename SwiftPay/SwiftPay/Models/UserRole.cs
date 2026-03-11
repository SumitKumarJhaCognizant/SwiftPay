using System;

namespace Model
{
    public class UserRole
    {
        public int UserRoleId { get; set; }

        public int UserId { get; set; }

        public int RoleId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public bool IsActive { get; set; }

        // Navigation
        public virtual User User { get; set; }

        public virtual Role Role { get; set; }
    }
}
