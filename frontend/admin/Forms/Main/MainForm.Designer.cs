using System.Windows.Forms;
using System.Drawing;

namespace admin.Forms.Main
{
	partial class MainForm
	{
		private System.ComponentModel.IContainer components = null;

		private Panel sidebar;
		private Panel panelContent;
		private Label lblAppTitle;

		private Button btnDashboard;
		private Button btnTrains;
		private Button btnUsers;
		private Button btnBookings;
		private Button btnAudit;
		private Button btnStatistics;
		private Button btnLogout;

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
				components.Dispose();
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.sidebar = new Panel();
			this.panelContent = new Panel();
			this.lblAppTitle = new Label();

			this.btnDashboard = new Button();
			this.btnTrains = new Button();
			this.btnUsers = new Button();
			this.btnBookings = new Button();
			this.btnAudit = new Button();
			this.btnStatistics = new Button();
			this.btnLogout = new Button();

			this.SuspendLayout();

			// ==========================================================
			// FORM SETTINGS
			// ==========================================================
			this.ClientSize = new Size(1500, 850);
			this.StartPosition = FormStartPosition.CenterScreen;
			this.BackColor = Color.FromArgb(30, 34, 48);
			this.FormBorderStyle = FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "MainForm";
			this.Text = "Hệ thống quản trị vé tàu";

			// ==========================================================
			// SIDEBAR
			// ==========================================================
			sidebar.BackColor = Color.FromArgb(37, 43, 59);  // #252B3B
			sidebar.Size = new Size(250, 850);
			sidebar.Location = new Point(0, 0);
			sidebar.Padding = new Padding(15, 20, 15, 20);

			// App title
			lblAppTitle.Text = "Hệ thống quản trị";
			lblAppTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
			lblAppTitle.ForeColor = Color.White;
			lblAppTitle.AutoSize = true;
			lblAppTitle.Location = new Point(25, 25);

			// ==========================================================
			// SIDEBAR BUTTON STYLE
			// ==========================================================

			// Dashboard
			btnDashboard.Text = "Dashboard";
			btnDashboard.Location = new Point(15, 100);
			btnDashboard.Size = new Size(220, 45);

			// Trains
			btnTrains.Text = "Quản lý chuyến tàu";
			btnTrains.Location = new Point(15, 155);
			btnTrains.Size = new Size(220, 45);

			// Users
			btnUsers.Text = "Quản lý người dùng";
			btnUsers.Location = new Point(15, 210);
			btnUsers.Size = new Size(220, 45);

			// Bookings
			btnBookings.Text = "Quản lý đặt vé";
			btnBookings.Location = new Point(15, 265);
			btnBookings.Size = new Size(220, 45);

			// Audit
			btnAudit.Text = "Nhật ký hoạt động";
			btnAudit.Location = new Point(15, 320);
			btnAudit.Size = new Size(220, 45);

			// Statistics
			btnStatistics.Text = "Báo cáo - thống kê";
			btnStatistics.Location = new Point(15, 375);
			btnStatistics.Size = new Size(220, 45);

			// Logout
			btnLogout.Text = "Đăng xuất";
			btnLogout.Location = new Point(15, 720);
			btnLogout.Size = new Size(220, 45);


			// ==========================================================
			// PANEL CONTENT (right side)
			// ==========================================================
			panelContent.BackColor = Color.FromArgb(45, 51, 68); // sáng hơn sidebar
			panelContent.Location = new Point(250, 0);
			panelContent.Size = new Size(1250, 850);

			// ==========================================================
			// ADD CONTROLS
			// ==========================================================
			sidebar.Controls.Add(lblAppTitle);
			sidebar.Controls.AddRange(new Control[]
			{
				btnDashboard, btnTrains, btnUsers,
				btnBookings, btnAudit, btnStatistics, btnLogout
			});

			this.Controls.Add(sidebar);
			this.Controls.Add(panelContent);

			this.ResumeLayout(false);
		}
	}
}
