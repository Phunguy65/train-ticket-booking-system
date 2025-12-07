using System.Configuration;

namespace client.Configuration
{
	public static class ApiConfig
	{
		private const string DefaultHost = "127.0.0.1";
		private const int DefaultPort = 5000;
		private const int DefaultConnectionTimeout = 30;
		private const int DefaultRequestTimeout = 30;

		public static string Host
		{
			get
			{
				var host = ConfigurationManager.AppSettings["ApiHost"];
				return string.IsNullOrWhiteSpace(host) ? DefaultHost : host;
			}
		}

		public static int Port
		{
			get
			{
				var portString = ConfigurationManager.AppSettings["ApiPort"];
				if (int.TryParse(portString, out int port))
				{
					return port;
				}

				return DefaultPort;
			}
		}

		public static int ConnectionTimeout
		{
			get
			{
				var timeoutString = ConfigurationManager.AppSettings["ConnectionTimeout"];
				if (int.TryParse(timeoutString, out int timeout))
				{
					return timeout;
				}

				return DefaultConnectionTimeout;
			}
		}

		public static int RequestTimeout
		{
			get
			{
				var timeoutString = ConfigurationManager.AppSettings["RequestTimeout"];
				if (int.TryParse(timeoutString, out int timeout))
				{
					return timeout;
				}

				return DefaultRequestTimeout;
			}
		}
	}
}