using System;
using System.Windows.Forms;
using sdk_client;
using admin.Forms.Authentication;
using admin.Forms.Main;

namespace admin
{
	internal static class Program
	{
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			// Khởi tạo ApiClient (kết nối TCP đến backend)
			var apiClient = new ApiClient("127.0.0.1", 5000);

			// Chạy LoginForm trước
			//Application.Run(new LoginForm(apiClient));
			Application.Run(new MainForm(apiClient));
		}
	}
}
