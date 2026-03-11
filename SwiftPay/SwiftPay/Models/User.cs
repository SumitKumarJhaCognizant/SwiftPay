using System;
using SwiftPay.Constants.Enums;

namespace Model
{
	public class User
	{
		public int UserId { get; set; }

		public string Name { get; set; }

		public string Email { get; set; }

		public string Phone { get; set; }

		public string Password { get; set; }

		public UserStatus Status { get; set; }

		public bool IsDeleted { get; set; }

		public DateTime CreatedAt { get; set; }

		public DateTime UpdatedAt { get; set; }

		// Navigation
		public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
	}
}
