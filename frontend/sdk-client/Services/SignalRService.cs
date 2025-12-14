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
		public event EventHandler<SeatHeldEvent>? SeatHeld;
		public event EventHandler<string>? ConnectionStateChanged;

		public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

		public SignalRService(string signalRUrl = "http://127.0.0.1:5001")
		{
			if (!Uri.TryCreate(signalRUrl, UriKind.Absolute, out var uri) ||
			    (uri.Scheme != "http" && uri.Scheme != "https"))
			{
				throw new ArgumentException("Invalid SignalR URL format. Must be http:// or https://",
					nameof(signalRUrl));
			}

			_hubUrl = signalRUrl.TrimEnd('/') + "/bookingHub";
		}

		public async Task StartAsync()
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(nameof(SignalRService));
			}

			if (_hubConnection is { State: HubConnectionState.Connected })
			{
				return;
			}

			_hubConnection = new HubConnectionBuilder()
				.WithUrl(_hubUrl)
				.WithAutomaticReconnect([
					TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10)
				])
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

			_hubConnection.On<SeatHeldEvent>("SeatHeld", (eventData) =>
			{
				SeatHeld?.Invoke(this, eventData);
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

		/// <summary>
		/// Asynchronously disposes the SignalR connection and releases all resources.
		/// This is the preferred disposal method for proper async cleanup.
		/// </summary>
		public async ValueTask DisposeAsync()
		{
			await DisposeAsyncCore().ConfigureAwait(false);

			Dispose(disposing: false);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Core async disposal logic for cleaning up the HubConnection.
		/// </summary>
		protected virtual async ValueTask DisposeAsyncCore()
		{
			if (_disposed)
			{
				return;
			}

			if (_hubConnection != null)
			{
				try
				{
					if (_hubConnection.State == HubConnectionState.Connected)
					{
						await _hubConnection.StopAsync().ConfigureAwait(false);
						OnConnectionStateChanged("Disconnected");
					}
				}
				catch
				{
					// Ignore exceptions during disposal
				}

				await _hubConnection.DisposeAsync().ConfigureAwait(false);
				_hubConnection = null;
			}

			_disposed = true;
		}

		/// <summary>
		/// Synchronously disposes the SignalR connection.
		/// Note: This is a fallback method. Use DisposeAsync() for proper async disposal.
		/// </summary>
		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Protected disposal method for synchronous cleanup.
		/// </summary>
		/// <param name="disposing">True if called from Dispose(), false if called from DisposeAsync()</param>
		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			if (disposing)
			{
				// Synchronous disposal fallback - not ideal but necessary for IDisposable compatibility
				if (_hubConnection != null)
				{
					try
					{
						if (_hubConnection.State == HubConnectionState.Connected)
						{
							_hubConnection.StopAsync().GetAwaiter().GetResult();
							OnConnectionStateChanged("Disconnected");
						}

						_hubConnection.DisposeAsync().GetAwaiter().GetResult();
						_hubConnection = null;
					}
					catch
					{
						// Ignore exceptions during disposal
					}
				}

				_disposed = true;
			}
		}
	}
}