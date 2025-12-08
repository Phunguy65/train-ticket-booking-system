using sdk_client.Protocol;
using System;
using System.Threading.Tasks;

namespace sdk_client.Services
{
	/// <summary>
	/// Interface for SignalR real-time communication service.
	/// Manages WebSocket connections to the backend SignalR hub for real-time seat availability updates.
	/// </summary>
	public interface ISignalRService : IDisposable
	{
		/// <summary>
		/// Event fired when seats are booked on a train.
		/// Subscribers receive real-time notifications when other users book seats.
		/// </summary>
		event EventHandler<SeatBookedEvent>? SeatBooked;

		/// <summary>
		/// Event fired when seats are released (booking cancelled) on a train.
		/// Subscribers receive real-time notifications when seats become available again.
		/// </summary>
		event EventHandler<SeatReleasedEvent>? SeatReleased;

		/// <summary>
		/// Event fired when the SignalR connection state changes.
		/// Useful for displaying connection status in the UI.
		/// </summary>
		event EventHandler<string>? ConnectionStateChanged;

		/// <summary>
		/// Gets whether the SignalR connection is currently active.
		/// </summary>
		bool IsConnected { get; }

		/// <summary>
		/// Starts the SignalR connection to the backend hub.
		/// Automatically attempts reconnection on failure.
		/// </summary>
		/// <returns>Task representing the async operation</returns>
		Task StartAsync();

		/// <summary>
		/// Stops the SignalR connection gracefully.
		/// </summary>
		/// <returns>Task representing the async operation</returns>
		Task StopAsync();

		/// <summary>
		/// Joins a train-specific SignalR group to receive seat updates for that train.
		/// Must be called after successful connection.
		/// </summary>
		/// <param name="trainId">Unique train identifier</param>
		/// <returns>Task representing the async operation</returns>
		Task JoinTrainGroupAsync(int trainId);

		/// <summary>
		/// Leaves a train-specific SignalR group to stop receiving updates for that train.
		/// Should be called when user navigates away from the booking page.
		/// </summary>
		/// <param name="trainId">Unique train identifier</param>
		/// <returns>Task representing the async operation</returns>
		Task LeaveTrainGroupAsync(int trainId);
	}
}