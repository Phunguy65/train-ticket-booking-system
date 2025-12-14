using admin.Services;
using admin.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace admin.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
	public MainWindow()
	{
		// Initialize SessionManager before any UI setup
		InitializeSessionManager();

		this.WhenActivated(disposables => { });
		InitializeComponent();
	}

	/// <summary>
	/// Initializes the SessionManager with API client configuration.
	/// </summary>
	private void InitializeSessionManager()
	{
		// TODO: Load from configuration file (appsettings.json)
		// For now, use hardcoded values
		SessionManager.Instance.Initialize(
			host: "127.0.0.1",
			port: 5000,
			connectionTimeout: 30,
			requestTimeout: 30
		);
	}
}