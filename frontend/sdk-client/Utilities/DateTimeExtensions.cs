using System;

namespace sdk_client.Utilities
{
	/// <summary>
	/// Extension methods for DateTime timezone conversion.
	/// Provides safe conversion between UTC and local time with proper DateTimeKind handling.
	/// </summary>
	public static class DateTimeExtensions
	{
		/// <summary>
		/// Converts a DateTime to UTC, handling different DateTimeKind values safely.
		/// If the DateTime is already UTC, returns it unchanged.
		/// If the DateTime is Local, converts to UTC.
		/// If the DateTime is Unspecified, treats it as local time and converts to UTC.
		/// </summary>
		/// <param name="dateTime">The DateTime to convert</param>
		/// <returns>DateTime in UTC with DateTimeKind.Utc</returns>
		public static DateTime ToUtcSafe(this DateTime dateTime)
		{
			switch (dateTime.Kind)
			{
				case DateTimeKind.Utc:
					return dateTime;
				case DateTimeKind.Local:
					return dateTime.ToUniversalTime();
				case DateTimeKind.Unspecified:
					// Treat unspecified as local time and convert to UTC
					return DateTime.SpecifyKind(dateTime, DateTimeKind.Local).ToUniversalTime();
				default:
					return dateTime.ToUniversalTime();
			}
		}

		/// <summary>
		/// Converts a DateTime to local time, handling different DateTimeKind values safely.
		/// If the DateTime is already Local, returns it unchanged.
		/// If the DateTime is UTC, converts to local time.
		/// If the DateTime is Unspecified, treats it as UTC and converts to local time.
		/// </summary>
		/// <param name="dateTime">The DateTime to convert</param>
		/// <returns>DateTime in local time with DateTimeKind.Local</returns>
		public static DateTime ToLocalTimeSafe(this DateTime dateTime)
		{
			switch (dateTime.Kind)
			{
				case DateTimeKind.Local:
					return dateTime;
				case DateTimeKind.Utc:
					return dateTime.ToLocalTime();
				case DateTimeKind.Unspecified:
					// Treat unspecified as UTC and convert to local time
					return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc).ToLocalTime();
				default:
					return dateTime.ToLocalTime();
			}
		}

		/// <summary>
		/// Converts a nullable DateTime to UTC safely.
		/// Returns null if the input is null.
		/// </summary>
		/// <param name="dateTime">The nullable DateTime to convert</param>
		/// <returns>Nullable DateTime in UTC or null</returns>
		public static DateTime? ToUtcSafe(this DateTime? dateTime)
		{
			return dateTime?.ToUtcSafe();
		}

		/// <summary>
		/// Converts a nullable DateTime to local time safely.
		/// Returns null if the input is null.
		/// </summary>
		/// <param name="dateTime">The nullable DateTime to convert</param>
		/// <returns>Nullable DateTime in local time or null</returns>
		public static DateTime? ToLocalTimeSafe(this DateTime? dateTime)
		{
			return dateTime?.ToLocalTimeSafe();
		}
	}
}