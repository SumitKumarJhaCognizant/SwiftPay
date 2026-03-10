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
		public Guid UserId { get; set; }

		public string Name { get; set; } = string.Empty;

		public UserRole Role { get; set; }

		public string Email { get; set; } = string.Empty;

		public string Phone { get; set; } = string.Empty;

		public UserStatus Status { get; set; }
	}
}
