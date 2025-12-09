using client.Forms.Authentication; // Sử dụng lại RoundedButton & ModernTextBox
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace client.Forms.Profile
{
	public partial class Profile : Form
	{
		// =========================================================
		// 1. CẤU HÌNH MÀU SẮC & THEME
		// =========================================================
		private readonly Color _clrBackground = Color.FromArgb(15, 23, 42);
		private readonly Color _clrHeader = Color.FromArgb(15, 23, 42);
		private readonly Color _clrTabActive = Color.FromArgb(37, 99, 235);
		private readonly Color _clrText = Color.White;
		private readonly Color _clrTextGray = Color.FromArgb(148, 163, 184);
		private readonly Color _clrItemBg = Color.FromArgb(30, 41, 59);

		private readonly Color ClrSuccess = Color.FromArgb(34, 197, 94);
		private readonly Color ClrWarning = Color.FromArgb(249, 115, 22);
		private readonly Color ClrError = Color.FromArgb(239, 68, 68);

		// Các biến UI Control
		private Panel pnlContent;
		private Label btnTabHistory, btnTabProfile;
		private Panel lineActiveTab;
		private bool isMaximized = false;

		// Cấu hình cột: [Mã vé, Tàu, Ngày đi, Trạng thái, Giá tiền]
		private readonly int[] colWidths = { 200, 350, 250, 250, 200 };

		public Profile()
		{
			InitializeComponent();
			SetupUI();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SwitchTab("HISTORY");
		}

		// =========================================================
		// 2. DỰNG GIAO DIỆN CHUNG
		// =========================================================
		private void SetupUI()
		{
			this.FormBorderStyle = FormBorderStyle.None;
			this.StartPosition = FormStartPosition.CenterScreen;
			this.Size = new Size(1500, 850);
			this.BackColor = _clrBackground;
			this.DoubleBuffered = true;

			// HEADER
			Panel pnlHeader = new Panel
			{
				Dock = DockStyle.Top, Height = 60, Padding = new Padding(30, 0, 30, 0), BackColor = _clrHeader
			};
			Label lblLogo = new Label
			{
				Text = "🚆 Vé Tàu Cao Tốc",
				Font = new Font("Segoe UI", 16, FontStyle.Bold),
				ForeColor = _clrTabActive,
				AutoSize = true,
				Location = new Point(30, 15),
				Cursor = Cursors.Hand
			};
			lblLogo.Click += (s, e) => this.Close();
			pnlHeader.Controls.Add(lblLogo);
			AddWindowControls(pnlHeader);
			SetupHeaderMenu(pnlHeader);
			this.Controls.Add(pnlHeader);

			// PAGE TITLE
			Panel pnlPageTitle = new Panel { Dock = DockStyle.Top, Height = 70, Padding = new Padding(50, 0, 50, 0) };
			Label lblPageTitle = new Label
			{
				Text = "Quản lý tài khoản",
				Font = new Font("Segoe UI", 24, FontStyle.Bold),
				ForeColor = Color.White,
				AutoSize = true,
				Location = new Point(50, 10)
			};
			pnlPageTitle.Controls.Add(lblPageTitle);

			// TABS
			Panel pnlTabs = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(50, 0, 50, 0) };
			btnTabHistory = CreateTabButton("Lịch sử đặt vé", 50);
			btnTabHistory.Click += (s, e) => SwitchTab("HISTORY");
			pnlTabs.Controls.Add(btnTabHistory);

			btnTabProfile = CreateTabButton("Hồ sơ cá nhân", 250);
			btnTabProfile.Click += (s, e) => SwitchTab("PROFILE");
			pnlTabs.Controls.Add(btnTabProfile);

			lineActiveTab = new Panel
			{
				Height = 4, BackColor = _clrTabActive, Location = new Point(50, 46), Size = new Size(100, 4)
			};
			pnlTabs.Controls.Add(lineActiveTab);

			// CONTENT
			pnlContent = new Panel { Dock = DockStyle.Fill, Padding = new Padding(50, 20, 50, 30) };

			// Thứ tự Add quan trọng cho Dock: Content trước -> Tabs -> Title -> Header
			this.Controls.Add(pnlContent);
			this.Controls.Add(pnlTabs);
			this.Controls.Add(pnlPageTitle);
			this.Controls.Add(pnlHeader);
		}

		private void SwitchTab(string tabName)
		{
			pnlContent.Controls.Clear();

			if (tabName == "HISTORY")
			{
				btnTabHistory.ForeColor = _clrText;
				btnTabProfile.ForeColor = _clrTextGray;
				lineActiveTab.Location = new Point(btnTabHistory.Location.X, 46);
				lineActiveTab.Width = btnTabHistory.Width;
				LoadHistoryContent();
			}
			else
			{
				btnTabHistory.ForeColor = _clrTextGray;
				btnTabProfile.ForeColor = _clrText;
				lineActiveTab.Location = new Point(btnTabProfile.Location.X, 46);
				lineActiveTab.Width = btnTabProfile.Width;
				LoadProfileContent();
			}
		}

		// =========================================================
		// 3. TAB LỊCH SỬ (ĐÃ SỬA LỖI CHỒNG LẤP)
		// =========================================================
		private void LoadHistoryContent()
		{
			// 1. Tạo Header Bảng
			Panel pnlTableHeader = new Panel
			{
				Dock = DockStyle.Top,
				Height = 50,
				BackColor = _clrBackground // Đổi màu nền trùng background để che chắn tốt hơn
			};

			string[] headers = { "MÃ VÉ", "THÔNG TIN TÀU", "NGÀY ĐI", "TRẠNG THÁI", "TỔNG TIỀN" };
			int curX = 20;
			for (int i = 0; i < headers.Length; i++)
			{
				Label lblH = new Label
				{
					Text = headers[i],
					ForeColor = _clrTextGray,
					Font = new Font("Segoe UI", 10, FontStyle.Bold),
					AutoSize = false,
					Size = new Size(colWidths[i], 40),
					Location = new Point(curX, 10),
					TextAlign = ContentAlignment.MiddleLeft
				};
				pnlTableHeader.Controls.Add(lblH);
				curX += colWidths[i];
			}

			// 2. Container danh sách
			FlowLayoutPanel flowList = new FlowLayoutPanel
			{
				Dock = DockStyle.Fill,
				FlowDirection = FlowDirection.TopDown,
				WrapContents = false,
				AutoScroll = true,
				Padding = new Padding(0, 10, 0, 0), // Khoảng cách nhỏ giữa header và item đầu tiên
				BackColor = Color.Transparent
			};

			// --- [QUAN TRỌNG: SỬA LẠI THỨ TỰ ADD CONTROL] ---

			// Bước 1: Add Header vào trước
			pnlContent.Controls.Add(pnlTableHeader);

			// Bước 2: Add List vào sau
			pnlContent.Controls.Add(flowList);

			// Bước 3: Đảo ngược quyền ưu tiên Docking
			// SendToBack() -> Đẩy xuống đáy danh sách quản lý -> Được ưu tiên xếp Layout ĐẦU TIÊN
			// Giúp Header chiếm chỗ phần Top trước, sau đó List mới điền vào phần còn lại (Fill)
			pnlTableHeader.SendToBack();
			flowList.BringToFront();

			// 3. Thêm dữ liệu mẫu (Giữ nguyên)
			AddHistoryItem(flowList, "#VE12345", "Tàu SE1 - Toa 5 (Ghế 12A)", "15/08/2024", "Đã hoàn tất", ClrSuccess,
				"450,000đ");
			AddHistoryItem(flowList, "#VE67890", "Tàu TN2 - Toa 3 (Ghế 05B)", "22/09/2024", "Sắp tới", ClrWarning,
				"500,000đ");
			AddHistoryItem(flowList, "#VE13579", "Tàu SE7 - Toa 1 (Ghế 01C)", "01/07/2024", "Đã hủy", ClrError,
				"380,000đ");
			AddHistoryItem(flowList, "#VE99999", "Tàu HN1 - Toa VIP", "30/12/2024", "Sắp tới", ClrWarning,
				"1,200,000đ");
			AddHistoryItem(flowList, "#VE88888", "Tàu SE3 - Toa 2", "10/01/2025", "Sắp tới", ClrWarning, "600,000đ");
		}

		private void AddHistoryItem(FlowLayoutPanel parent, string code, string train, string date, string status,
			Color statusColor, string price)
		{
			int itemWidth = parent.ClientSize.Width - 20;
			if (itemWidth < 1200) itemWidth = 1200;

			Panel pnlItem = new Panel
			{
				Size = new Size(itemWidth, 70), Margin = new Padding(0, 0, 0, 15), BackColor = Color.Transparent
			};

			pnlItem.Paint += (s, e) =>
			{
				e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
				Rectangle rect = new Rectangle(0, 0, pnlItem.Width - 1, pnlItem.Height - 1);
				using (GraphicsPath path = GetRoundedPath(rect, 15))
				using (SolidBrush brush = new SolidBrush(_clrItemBg)) { e.Graphics.FillPath(brush, path); }
			};

			int curX = 20;
			pnlItem.Controls.Add(CreateLabel(code, 11, FontStyle.Bold, _clrText, curX, 25));
			curX += colWidths[0];
			pnlItem.Controls.Add(CreateLabel(train, 11, FontStyle.Regular, _clrText, curX, 25));
			curX += colWidths[1];
			pnlItem.Controls.Add(CreateLabel(date, 11, FontStyle.Regular, _clrTextGray, curX, 25));
			curX += colWidths[2];
			Label lblStatus = new Label
			{
				Text = status,
				ForeColor = statusColor,
				Font = new Font("Segoe UI", 10, FontStyle.Bold),
				AutoSize = true,
				Location = new Point(curX, 25)
			};
			pnlItem.Controls.Add(lblStatus);
			curX += colWidths[3];
			pnlItem.Controls.Add(CreateLabel(price, 12, FontStyle.Bold, _clrText, curX, 23));

			parent.Controls.Add(pnlItem);
		}

		// =========================================================
		// 4. TAB HỒ SƠ (GIỮ NGUYÊN)
		// =========================================================
		private void LoadProfileContent()
		{
			Panel pnlProfile = new Panel { Size = new Size(800, 500), Location = new Point(20, 20) };
			Label lblHeader = new Label
			{
				Text = "Thông tin cá nhân",
				Font = new Font("Segoe UI", 14, FontStyle.Bold),
				ForeColor = _clrText,
				AutoSize = true,
				Location = new Point(0, 0)
			};
			Label lblSub = new Label
			{
				Text = "Cập nhật thông tin định danh và liên hệ của bạn.",
				Font = new Font("Segoe UI", 10, FontStyle.Regular),
				ForeColor = _clrTextGray,
				AutoSize = true,
				Location = new Point(0, 35)
			};
			pnlProfile.Controls.Add(lblHeader);
			pnlProfile.Controls.Add(lblSub);

			int yPos = 80;
			pnlProfile.Controls.Add(CreateLabel("Họ và Tên", 10, FontStyle.Regular, _clrTextGray, 0, yPos));
			pnlProfile.Controls.Add(CreateLabel("Số điện thoại", 10, FontStyle.Regular, _clrTextGray, 420, yPos));
			yPos += 30;

			ModernTextBox txtName = new ModernTextBox
			{
				Location = new Point(0, yPos),
				Size = new Size(380, 50),
				PlaceholderText = "Nguyễn Văn A",
				BackColor = _clrItemBg,
				ForeColor = _clrText,
				IconText = "👤"
			};
			pnlProfile.Controls.Add(txtName);
			ModernTextBox txtPhone = new ModernTextBox
			{
				Location = new Point(420, yPos),
				Size = new Size(380, 50),
				PlaceholderText = "0909123456",
				BackColor = _clrItemBg,
				ForeColor = _clrText,
				IconText = "📞"
			};
			pnlProfile.Controls.Add(txtPhone);
			yPos += 70;

			pnlProfile.Controls.Add(CreateLabel("Địa chỉ Email", 10, FontStyle.Regular, _clrTextGray, 0, yPos));
			yPos += 30;
			ModernTextBox txtEmail = new ModernTextBox
			{
				Location = new Point(0, yPos),
				Size = new Size(800, 50),
				PlaceholderText = "example@email.com",
				BackColor = _clrItemBg,
				ForeColor = _clrText,
				IconText = "📧"
			};
			pnlProfile.Controls.Add(txtEmail);
			yPos += 90;

			RoundedButton btnUpdate = new RoundedButton
			{
				Text = "Lưu thay đổi",
				BackColor = _clrTabActive,
				ForeColor = Color.White,
				Size = new Size(200, 50),
				Location = new Point(0, yPos),
				Font = new Font("Segoe UI", 11, FontStyle.Bold),
				Cursor = Cursors.Hand,
				FlatStyle = FlatStyle.Flat
			};
			btnUpdate.FlatAppearance.BorderSize = 0;
			btnUpdate.Click += (s, e) => MessageBox.Show(@"Cập nhật thông tin thành công!", @"Hệ thống");
			pnlProfile.Controls.Add(btnUpdate);

			pnlContent.Controls.Add(pnlProfile);
		}

		// =========================================================
		// HELPER METHODS
		// =========================================================
		private void SetupHeaderMenu(Panel pnlHeader)
		{
			string[] menuItems = { "Đăng xuất", "Trang chủ" };
			int menuX = pnlHeader.Width - 180;
			foreach (var item in menuItems)
			{
				Label lblMenu = new Label
				{
					Text = item,
					Font = new Font("Segoe UI", 11, FontStyle.Regular),
					ForeColor = _clrTextGray,
					AutoSize = true,
					Cursor = Cursors.Hand,
					Anchor = AnchorStyles.Top | AnchorStyles.Right
				};
				lblMenu.Location = new Point(menuX - 80, 20);

				lblMenu.MouseEnter += (s, e) => lblMenu.ForeColor = Color.White;
				lblMenu.MouseLeave += (s, e) => lblMenu.ForeColor = _clrTextGray;

				// --- XỬ LÝ SỰ KIỆN CLICK TẠI ĐÂY ---
				if (item == "Đăng xuất")
				{
					// Đóng hết ứng dụng hoặc quay về Login tùy logic
					lblMenu.Click += (s, e) => Application.Exit();
				}
				else if (item == "Trang chủ")
				{
					// CHỈ CẦN ĐÓNG PROFILE LÀ TỰ QUAY VỀ MAINFORM
					lblMenu.Click += (s, e) => this.Close();
				}
				// ------------------------------------

				pnlHeader.Controls.Add(lblMenu);
				menuX -= 120;
			}
		}

		private void AddWindowControls(Panel parent)
		{
			int btnSize = 45;
			int startX = parent.Width - (btnSize * 3) - 10;
			Label btnClose = CreateWindowButton("✕", startX + (btnSize * 2), ClrError);
			btnClose.Click += (s, e) => this.Close();
			btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			parent.Controls.Add(btnClose);
			Label btnMax = CreateWindowButton("☐", startX + btnSize, _clrItemBg);
			btnMax.Click += (s, e) => ToggleMaximize();
			btnMax.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			parent.Controls.Add(btnMax);
			Label btnMin = CreateWindowButton("―", startX, _clrItemBg);
			btnMin.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
			btnMin.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			parent.Controls.Add(btnMin);
		}

		private Label CreateWindowButton(string text, int x, Color hoverColor)
		{
			Label lbl = new Label
			{
				Text = text,
				Font = new Font("Segoe UI", 12, FontStyle.Regular),
				ForeColor = Color.White,
				AutoSize = false,
				Size = new Size(45, 30),
				Location = new Point(x, 15),
				TextAlign = ContentAlignment.MiddleCenter,
				Cursor = Cursors.Hand
			};
			lbl.MouseEnter += (s, e) => lbl.BackColor = hoverColor;
			lbl.MouseLeave += (s, e) => lbl.BackColor = Color.Transparent;
			return lbl;
		}

		private void ToggleMaximize()
		{
			if (isMaximized)
			{
				this.WindowState = FormWindowState.Normal;
				this.Size = new Size(1500, 850);
				this.CenterToScreen();
			}
			else { this.WindowState = FormWindowState.Maximized; }

			isMaximized = !isMaximized;
		}

		private Label CreateTabButton(string text, int x)
		{
			return new Label
			{
				Text = text,
				Font = new Font("Segoe UI", 12, FontStyle.Bold),
				ForeColor = _clrTextGray,
				AutoSize = true,
				Location = new Point(x, 10),
				Cursor = Cursors.Hand
			};
		}

		private Label CreateLabel(string text, float size, FontStyle style, Color color, int x, int y)
		{
			return new Label
			{
				Text = text,
				Font = new Font("Segoe UI", size, style),
				ForeColor = color,
				AutoSize = true,
				Location = new Point(x, y)
			};
		}

		public static GraphicsPath GetRoundedPath(Rectangle rect, int radius)
		{
			GraphicsPath path = new GraphicsPath();
			float d = radius * 2F;
			path.AddArc(rect.X, rect.Y, d, d, 180, 90);
			path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
			path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
			path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
			path.CloseFigure();
			return path;
		}

		[System.Runtime.InteropServices.DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

		[System.Runtime.InteropServices.DllImport("user32.dll")]
		public static extern bool ReleaseCapture();

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				ReleaseCapture();
				SendMessage(Handle, 0xA1, 0x2, 0);
			}
		}
	}
}