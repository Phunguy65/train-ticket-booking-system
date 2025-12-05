using System;

namespace admin.Models.Dto
{
	public class UserDto
	{
		public int UserId { get; set; }

		// Auth info
		public string Username { get; set; }
		public string PasswordHash { get; set; }

		// Profile
		public string FullName { get; set; }
		public string Email { get; set; }
		public string PhoneNumber { get; set; }

		// Role & status
		public string Role { get; set; }
		public bool IsActive { get; set; }

		// More
		public DateTime CreatedAt { get; set; }
	}
}
