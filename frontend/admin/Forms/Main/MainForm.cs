using admin.Forms.Authentication;
using admin.Forms.UserManagement;
using sdk_client;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace admin.Forms.Main
{
	public partial class MainForm : Form
	{
		private readonly ApiClient _apiClient;

		public MainForm(ApiClient apiClient)
		{
			InitializeComponent();
			_apiClient = apiClient;

			SetupUI();

			// Default mở Users
			//SetActiveButton(btnUsers);
			//OpenUserControl(new UserManagementControl());

			// Navigation
			btnDashboard.Click += (_, __) => SetActiveButton(btnDashboard);
			btnTrains.Click += (_, __) => SetActiveButton(btnTrains);

			btnUsers.Click += (_, __) =>
			{
				SetActiveButton(btnUsers);
				OpenUserControl(new UserManagementControl());
			};

			btnBookings.Click += (_, __) => SetActiveButton(btnBookings);
			btnAudit.Click += (_, __) => SetActiveButton(btnAudit);
			btnStatistics.Click += (_, __) => SetActiveButton(btnStatistics);

			btnLogout.Click += BtnLogout_Click;
		}

		// =========================================================
		// LOGOUT → QUAY VỀ LOGIN
		// =========================================================
		private void BtnLogout_Click(object sender, EventArgs e)
		{
			Application.Restart();
		}

		// =========================================================
		// UI SETUP
		// =========================================================
		private void SetupUI()
		{
			this.BackColor = Color.FromArgb(25, 28, 38);
			this.MinimumSize = new Size(1200, 800);

			// --- Sidebar ---
			sidebar.Dock = DockStyle.Left;
			sidebar.Width = 280;
			sidebar.BackColor = Color.FromArgb(32, 37, 52);

			// --- Content ---
			panelContent.Dock = DockStyle.Fill;
			panelContent.BackColor = Color.FromArgb(38, 43, 58);

			// FIX QUAN TRỌNG: đảm bảo panelContent không bị sidebar che
			this.Controls.SetChildIndex(sidebar, 1);
			this.Controls.SetChildIndex(panelContent, 0);

			// Title sidebar
			lblAppTitle.Text = "Hệ thống quản trị";
			lblAppTitle.Font = new Font("Segoe UI", 20, FontStyle.Bold);
			lblAppTitle.ForeColor = Color.FromArgb(0, 170, 255);
			lblAppTitle.Location = new Point(30, 30);

			// Buttons
			CreateSidebarBtn(btnDashboard, "Dashboard", "\uE80F", 120);
			CreateSidebarBtn(btnTrains, "Quản lý chuyến tàu", "\uE7C0", 182);
			CreateSidebarBtn(btnUsers, "Quản lý người dùng", "\uE716", 244);
			CreateSidebarBtn(btnBookings, "Quản lý đặt vé", "\uE897", 306);
			CreateSidebarBtn(btnAudit, "Nhật ký hoạt động", "\uE8D2", 368);
			CreateSidebarBtn(btnStatistics, "Báo cáo thống kê", "\uE8F1", 430);

			SetupLogoutButton();
		}


		// =========================================================
		// Sidebar Buttons
		// =========================================================
		private void CreateSidebarBtn(Button btn, string text, string iconUnicode, int y)
		{
			btn.Font = new Font("Segoe UI", 11F);
			btn.ForeColor = Color.FromArgb(220, 220, 220);
			btn.BackColor = Color.Transparent;
			btn.FlatStyle = FlatStyle.Flat;
			btn.FlatAppearance.BorderSize = 0;

			btn.Size = new Size(240, 48);
			btn.Location = new Point(20, y);
			btn.Text = text;
			btn.TextAlign = ContentAlignment.MiddleLeft;

			btn.Padding = new Padding(45, 0, 0, 0);
			btn.Tag = "normal";

			btn.Paint += (s, e) =>
			{
				using (Font f = new Font("Segoe MDL2 Assets", 19))
				using (Brush b = new SolidBrush(btn.ForeColor))
					e.Graphics.DrawString(iconUnicode, f, b, 10, 12);
			};

			btn.MouseEnter += (_, __) =>
			{
				if ((string)btn.Tag != "active")
					btn.BackColor = Color.FromArgb(50, 60, 80);
			};

			btn.MouseLeave += (_, __) =>
			{
				if ((string)btn.Tag != "active")
					btn.BackColor = Color.Transparent;
			};

			btn.Click += (_, __) => SetActiveButton(btn);
		}

		// =========================================================
		// Logout Button
		// =========================================================
		private void SetupLogoutButton()
		{
			btnLogout.Text = "   Đăng xuất";
			btnLogout.Font = new Font("Segoe UI", 11F);

			btnLogout.Size = new Size(240, 52);
			btnLogout.Location = new Point(20, 780);

			btnLogout.BackColor = Color.FromArgb(200, 60, 60);
			btnLogout.ForeColor = Color.White;
			btnLogout.FlatStyle = FlatStyle.Flat;
			btnLogout.FlatAppearance.BorderSize = 0;

			btnLogout.Paint += (s, e) =>
			{
				using (Font f = new Font("Segoe MDL2 Assets", 18))
					e.Graphics.DrawString("\uE8AC", f, Brushes.White, 10, 12);
			};

			btnLogout.MouseEnter += (_, __) => btnLogout.BackColor = Color.FromArgb(220, 80, 80);
			btnLogout.MouseLeave += (_, __) => btnLogout.BackColor = Color.FromArgb(200, 60, 60);
		}

		// =========================================================
		// Active Button Highlight
		// =========================================================
		private void SetActiveButton(Button btn)
		{
			foreach (Button b in new[] { btnDashboard, btnTrains, btnUsers, btnBookings, btnAudit, btnStatistics })
			{
				b.Tag = "normal";
				b.BackColor = Color.Transparent;
				b.ForeColor = Color.FromArgb(220, 220, 220);
			}

			btn.Tag = "active";
			btn.BackColor = Color.FromArgb(0, 130, 220);
			btn.ForeColor = Color.White;
		}

		// =========================================================
		// Load UserControl
		// =========================================================
		private void OpenUserControl(UserControl control)
		{
			panelContent.Controls.Clear();

			if (control != null)
			{
				control.Margin = new Padding(0);
				control.Padding = new Padding(0);
				control.AutoScaleMode = AutoScaleMode.None;
				control.Dock = DockStyle.Fill;

				panelContent.Controls.Add(control);
			}
		}
	}
}
