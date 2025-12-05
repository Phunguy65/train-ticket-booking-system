using sdk_client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace admin.Forms.Authentication
{
	public partial class LoginForm : Form
	{
		private readonly ApiClient _apiClient;

		public LoginForm(ApiClient apiClient)
		{
			InitializeComponent();
			_apiClient = apiClient;
			String userName = this.txtUsername.Text.ToString();
			Console.Write("Hello Admin");

			this.Load += LoginForm_Load;
		}

		private void LoginForm_Load(object sender, EventArgs e)
		{
			panelCard.Left = (this.ClientSize.Width - panelCard.Width) / 2;
			panelCard.Top = (this.ClientSize.Height - panelCard.Height) / 2;
		}
		

		public LoginForm()
		{
			InitializeComponent();
		}

	}
}
