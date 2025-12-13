namespace backend.Business.Models;

/// <summary>
/// Defines valid status values for train schedules.
/// Used to filter and manage train availability.
/// </summary>
public enum TrainStatus
{
	/// <summary>
	/// Train is active and available for booking.
	/// </summary>
	Active,

	/// <summary>
	/// Train has been cancelled and is not available for booking.
	/// </summary>
	Cancelled,

	/// <summary>
	/// Train journey has been completed.
	/// </summary>
	Completed
}

/// <summary>
/// Extension methods for TrainStatus enum.
/// </summary>
public static class TrainStatusExtensions
{
	/// <summary>
	/// Converts TrainStatus enum to database string value.
	/// </summary>
	public static string ToDbString(this TrainStatus status)
	{
		return status.ToString();
	}

	/// <summary>
	/// Parses database string value to TrainStatus enum.
	/// Returns null if the value is invalid.
	/// </summary>
	public static TrainStatus? FromDbString(string? value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return null;
		}

		return Enum.TryParse<TrainStatus>(value, true, out var result) ? result : null;
	}

	/// <summary>
	/// Validates if a string is a valid TrainStatus value.
	/// </summary>
	public static bool IsValidStatus(string? value)
	{
		return FromDbString(value).HasValue;
	}
}