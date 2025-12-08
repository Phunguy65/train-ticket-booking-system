using Microsoft.AspNetCore.SignalR.Client;
using sdk_client.Protocol;
using System;
using System.Threading.Tasks;

namespace sdk_client.Services
{
	/// <summary>
	/// SignalR service implementation for real-time seat availability updates.
	/// Manages WebSocket connection lifecycle, automatic reconnection, and event broadcasting.
	/// </summary>
	public class SignalRService : ISignalRService
	{
		private HubConnection? _hubConnection;
		private readonly string _hubUrl;
		private bool _disposed;

		public event EventHandler<SeatBookedEvent>? SeatBooked;
		public event EventHandler<SeatReleasedEvent>? SeatReleased;
		public event EventHandler<string>? ConnectionStateChanged;

		public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

		/// <summary>
		/// Initializes a new instance of SignalRService with the SignalR hub URL.
		/// </summary>
		/// <param name="signalRHost">SignalR server hostname or IP address</param>
		/// <param name="signalRPort">SignalR server port number</param>
		public SignalRService(string signalRHost = "127.0.0.1", int signalRPort = 5001)
		{
			_hubUrl = $"http://{signalRHost}:{signalRPort}/bookingHub";
		}

		public async Task StartAsync()
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(nameof(SignalRService));
			}

			if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
			{
				return;
			}

			_hubConnection = new HubConnectionBuilder()
				.WithUrl(_hubUrl)
				.WithAutomaticReconnect(new[]
				{
					TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10)
				})
				.Build();

			RegisterEventHandlers();
			RegisterConnectionHandlers();

			await _hubConnection.StartAsync().ConfigureAwait(false);
			OnConnectionStateChanged("Connected");
		}

		public async Task StopAsync()
		{
			if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
			{
				await _hubConnection.StopAsync().ConfigureAwait(false);
				OnConnectionStateChanged("Disconnected");
			}
		}

		public async Task JoinTrainGroupAsync(int trainId)
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(nameof(SignalRService));
			}

			if (_hubConnection == null || _hubConnection.State != HubConnectionState.Connected)
			{
				throw new InvalidOperationException("SignalR connection is not active. Call StartAsync() first.");
			}

			await _hubConnection.InvokeAsync("JoinTrainGroup", trainId).ConfigureAwait(false);
		}

		public async Task LeaveTrainGroupAsync(int trainId)
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(nameof(SignalRService));
			}

			if (_hubConnection == null || _hubConnection.State != HubConnectionState.Connected)
			{
				return;
			}

			await _hubConnection.InvokeAsync("LeaveTrainGroup", trainId).ConfigureAwait(false);
		}

		private void RegisterEventHandlers()
		{
			if (_hubConnection == null) return;

			_hubConnection.On<SeatBookedEvent>("SeatBooked", (eventData) =>
			{
				SeatBooked?.Invoke(this, eventData);
			});

			_hubConnection.On<SeatReleasedEvent>("SeatReleased", (eventData) =>
			{
				SeatReleased?.Invoke(this, eventData);
			});
		}

		private void RegisterConnectionHandlers()
		{
			if (_hubConnection == null) return;

			_hubConnection.Closed += async (error) =>
			{
				OnConnectionStateChanged("Disconnected");
				if (error != null)
				{
					await Task.Delay(TimeSpan.FromSeconds(5));
				}
			};

			_hubConnection.Reconnecting += (_) =>
			{
				OnConnectionStateChanged("Reconnecting");
				return Task.CompletedTask;
			};

			_hubConnection.Reconnected += (_) =>
			{
				OnConnectionStateChanged("Reconnected");
				return Task.CompletedTask;
			};
		}

		private void OnConnectionStateChanged(string state)
		{
			ConnectionStateChanged?.Invoke(this, state);
		}

		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}

			if (_hubConnection != null)
			{
				_hubConnection.StopAsync().GetAwaiter().GetResult();
				_hubConnection.DisposeAsync().GetAwaiter().GetResult();
				_hubConnection = null;
			}

			_disposed = true;
		}
	}
}