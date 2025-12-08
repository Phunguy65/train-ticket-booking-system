using System;

namespace client.Configuration
{
	public static class ApiConfig
	{
		private const string DefaultHost = "127.0.0.1";

		public static string Host
		{
			get
			{
				var host = Environment.GetEnvironmentVariable("API_HOST");
				if (!string.IsNullOrWhiteSpace(host))
				{
					return host;
				}

				host = Properties.Settings.Default.ApiHost;
				return string.IsNullOrWhiteSpace(host) ? DefaultHost : host;
			}
		}

		public static int Port
		{
			get
			{
				var portEnv = Environment.GetEnvironmentVariable("API_PORT");
				if (!string.IsNullOrWhiteSpace(portEnv))
				{
					return int.Parse(portEnv);
				}

				var portConfig = Properties.Settings.Default.ApiPort;

				return portConfig;
			}
		}

		public static int ConnectionTimeout
		{
			get
			{
				var timeoutEnv = Environment.GetEnvironmentVariable("API_CONNECTION_TIMEOUT");
				if (!string.IsNullOrWhiteSpace(timeoutEnv))
				{
					return int.Parse(timeoutEnv);
				}

				var timeoutConfig = Properties.Settings.Default.ApiConnectionTimeout;

				return timeoutConfig;
			}
		}

		public static int RequestTimeout
		{
			get
			{
				var timeoutEnv = Environment.GetEnvironmentVariable("API_REQUEST_TIMEOUT");
				if (!string.IsNullOrWhiteSpace(timeoutEnv))
				{
					return int.Parse(timeoutEnv);
				}

				var timeoutConfig = Properties.Settings.Default.ApiRequestTimeout;

				return timeoutConfig;
			}
		}

		public static string SignalRHost
		{
			get
			{
				var host = Environment.GetEnvironmentVariable("SIGNALR_HOST");
				if (!string.IsNullOrWhiteSpace(host))
				{
					return host;
				}

				host = Properties.Settings.Default.SignalRHost;
				return string.IsNullOrWhiteSpace(host) ? DefaultHost : host;
			}
		}

		public static int SignalRPort
		{
			get
			{
				var portEnv = Environment.GetEnvironmentVariable("SIGNALR_PORT");
				if (!string.IsNullOrWhiteSpace(portEnv))
				{
					return int.Parse(portEnv);
				}

				return Properties.Settings.Default.SignalRPort;
			}
		}
	}
}