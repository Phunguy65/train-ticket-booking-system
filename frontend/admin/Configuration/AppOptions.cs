namespace admin.Configuration
{
	public sealed class AppOptions
	{
		public ApiConfig ApiConfig { get; set; } = new ApiConfig();
		public SignalRConfig SignalRConfig { get; set; } = new SignalRConfig();

		public int RequestTimeout { get; set; } = 30;

		public int ConnectTimeout { get; set; } = 30;
	}

	public sealed class ApiConfig
	{
		public string Host { get; set; } = "http://localhost:5000";
		public int Port { get; set; } = 5000;
	}

	public sealed class SignalRConfig
	{
		public string Host { get; set; } = "http://localhost:5001";
		public int Port { get; set; } = 5001;
	}
}