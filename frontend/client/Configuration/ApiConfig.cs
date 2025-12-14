using System;
using System.Configuration;

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

				host = ConfigurationManager.AppSettings["ApiHost"];
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

				var portConfig = ConfigurationManager.AppSettings["ApiPort"];
				return int.Parse(portConfig ?? "5000");
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

				var timeoutConfig = ConfigurationManager.AppSettings["ApiConnectionTimeout"];
				return int.Parse(timeoutConfig ?? "30");
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

				var timeoutConfig = ConfigurationManager.AppSettings["ApiRequestTimeout"];
				return int.Parse(timeoutConfig ?? "30");
			}
		}

		public static string SignalRUrl
		{
			get
			{
				var url = Environment.GetEnvironmentVariable("SIGNALR_URL");
				if (!string.IsNullOrWhiteSpace(url))
				{
					return url;
				}

				url = ConfigurationManager.AppSettings["SignalRUrl"];
				return string.IsNullOrWhiteSpace(url) ? "http://127.0.0.1:5001" : url;
			}
		}
	}
}