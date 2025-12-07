using System.Net;
using System.Net.Sockets;
using backend.Presentation.Handlers;
using Microsoft.Extensions.Configuration;

namespace backend.Presentation;

/// <summary>
/// TCP server that listens for client connections and spawns ClientHandler instances.
/// Manages concurrent client connections and provides graceful shutdown.
/// </summary>
public class TcpServer
{
	private readonly IConfiguration _configuration;
	private readonly IServiceProvider _serviceProvider;
	private readonly ILogger<TcpServer> _logger;
	private TcpListener? _listener;
	private readonly List<Task> _clientTasks = new();
	private CancellationTokenSource? _cancellationTokenSource;

	public TcpServer(
		IConfiguration configuration,
		IServiceProvider serviceProvider,
		ILogger<TcpServer> logger)
	{
		_configuration = configuration;
		_serviceProvider = serviceProvider;
		_logger = logger;
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		var host = _configuration.GetValue<string>("TcpServer:Host") ?? "127.0.0.1";
		var port = _configuration.GetValue("TcpServer:Port", 5000);

		_listener = new TcpListener(IPAddress.Parse(host), port);
		_listener.Start();
		_cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

		_logger.LogInformation("TCP Server started on {Host}:{Port}", host, port);

		try
		{
			while (!_cancellationTokenSource.Token.IsCancellationRequested)
			{
				var client = await _listener.AcceptTcpClientAsync(_cancellationTokenSource.Token);
				_logger.LogInformation("Client connected from {RemoteEndPoint}", client.Client.RemoteEndPoint);

				var clientTask = Task.Run(async () =>
				{
					using var scope = _serviceProvider.CreateScope();
					var authenticationHandler = scope.ServiceProvider.GetRequiredService<AuthenticationHandler>();
					var trainHandler = scope.ServiceProvider.GetRequiredService<TrainHandler>();
					var bookingHandler = scope.ServiceProvider.GetRequiredService<BookingHandler>();
					var userHandler = scope.ServiceProvider.GetRequiredService<UserHandler>();
					var auditHandler = scope.ServiceProvider.GetRequiredService<AuditHandler>();
					var clientLogger = scope.ServiceProvider.GetRequiredService<ILogger<ClientHandler>>();

					var clientHandler = new ClientHandler(
						client,
						authenticationHandler,
						trainHandler,
						bookingHandler,
						userHandler,
						auditHandler,
						clientLogger);

					await clientHandler.HandleAsync();
				}, _cancellationTokenSource.Token);

				_clientTasks.Add(clientTask);
			}
		}
		catch (OperationCanceledException)
		{
			_logger.LogInformation("TCP Server is shutting down.");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in TCP Server.");
		}
	}

	public async Task StopAsync()
	{
		_logger.LogInformation("Stopping TCP Server...");

		_cancellationTokenSource?.Cancel();
		_listener?.Stop();

		await Task.WhenAll(_clientTasks);

		_logger.LogInformation("TCP Server stopped.");
	}
}