using System;

namespace Model
{
	public enum UserRole
	{
		Customer,
		Agent,
		Compliance,
		Ops,
		Treasury,
		Admin
	}

	public enum UserStatus
	{
		Active,
		Locked,
		Disabled
	}

	public class User
	{
		public int UserId { get; set; }

		public string? Name { get; set; }

		public UserRole Role { get; set; }

		public string? Email { get; set; }

		public string? Phone { get; set; }

		public UserStatus Status { get; set; }
	}
}
