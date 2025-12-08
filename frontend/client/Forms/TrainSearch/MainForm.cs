using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using client.Controls; // Import Custom Controls
using client.Helpers;  // Import Màu sắc chung

namespace client.Forms.TrainSearch
{
	public partial class MainForm : Form
	{
		// =========================================================
		// 1. CẤU HÌNH RIÊNG (Những màu đặc thù chỉ dùng ở Form này)
		// =========================================================
		private readonly Color ClrStatusGreen = Color.FromArgb(34, 197, 94);   // Còn nhiều vé
		private readonly Color ClrStatusOrange = Color.FromArgb(249, 115, 22); // Sắp hết
		private readonly Color ClrStatusRed = Color.FromArgb(239, 68, 68);     // Hết vé
		private readonly Color ClrSidebarBg = Color.FromArgb(30, 41, 59);      // Nền Sidebar riêng biệt

		// Biến toàn cục
		private FlowLayoutPanel flowResults;
		private ModernTextBox txtDepStation, txtArrStation, txtDate;
		private Label lblResultTitle;
		private bool isMaximized = false;

		public MainForm()
		{
			InitializeComponent();
			SetupForm();
			SetupHeader();
			SetupSidebar();
			SetupResultArea();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			LoadListTrain();
		}

		// =========================================================
		// 2. SETUP GIAO DIỆN (ĐÃ TÁCH NHỎ)
		// =========================================================

		private void SetupForm()
		{
			this.FormBorderStyle = FormBorderStyle.None;
			this.StartPosition = FormStartPosition.CenterScreen;
			this.Size = new Size(1500, 850);
			this.BackColor = AppColors.Background;
			this.DoubleBuffered = true;
		}

		private void SetupHeader()
		{
			Panel pnlHeader = new Panel
			{
				Dock = DockStyle.Top,
				Height = 60,
				BackColor = AppColors.CardBg, // Dùng màu Card cho Header
				Padding = new Padding(20, 0, 20, 0)
			};

			// Logo
			Label lblLogo = new Label
			{
				Text = "🚆 Vé Tàu Cao Tốc",
				Font = new Font("Segoe UI", 14, FontStyle.Bold),
				ForeColor = AppColors.Primary,
				AutoSize = true,
				Location = new Point(20, 15)
			};
			pnlHeader.Controls.Add(lblLogo);

			// Nút điều khiển Window (Close/Max/Min)
			AddWindowControls(pnlHeader);

			// Menu User (Đăng xuất / Tài khoản)
			SetupHeaderMenu(pnlHeader);

			this.Controls.Add(pnlHeader);
		}

		private void SetupHeaderMenu(Panel pnlHeader)
		{
			string[] menuItems = { "Đăng xuất", "Tài khoản" };
			int menuX = pnlHeader.Width - 160;

			foreach (var item in menuItems)
			{
				Label lblMenu = new Label
				{
					Text = item,
					Font = new Font("Segoe UI", 10, FontStyle.Regular),
					ForeColor = AppColors.TextMuted,
					AutoSize = true,
					Cursor = Cursors.Hand,
					Anchor = AnchorStyles.Top | AnchorStyles.Right
				};
				lblMenu.Location = new Point(menuX - 80, 20);

				// Hover Effect
				lblMenu.MouseEnter += (s, e) => lblMenu.ForeColor = AppColors.Primary;
				lblMenu.MouseLeave += (s, e) => lblMenu.ForeColor = AppColors.TextMuted;

				// Click Logic
				if (item == "Đăng xuất")
				{
					lblMenu.Click += (s, e) => Application.Exit();
				}
				else if (item == "Tài khoản")
				{
					lblMenu.Click += (s, e) => OpenProfile();
				}

				pnlHeader.Controls.Add(lblMenu);
				menuX -= 100;
			}
		}

		private void SetupSidebar()
		{
			int sidebarW = 320;
			Panel pnlSearch = new Panel
			{
				Size = new Size(sidebarW, 600),
				Location = new Point(30, 80),
				BackColor = Color.Transparent,
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom
			};

			// Vẽ nền bo góc cho Sidebar
			pnlSearch.Paint += (s, e) =>
			{
				e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
				using (GraphicsPath path = RoundedButton.GetRoundedPath(new Rectangle(0, 0, pnlSearch.Width, pnlSearch.Height - 20), 15))
				using (SolidBrush brush = new SolidBrush(ClrSidebarBg))
				{
					e.Graphics.FillPath(brush, path);
				}
			};
			this.Controls.Add(pnlSearch);

			// Nội dung Sidebar
			int yPos = 30; int xMargin = 25; int inputW = sidebarW - (xMargin * 2);

			pnlSearch.Controls.Add(CreateLabel("Tìm chuyến tàu", 14, FontStyle.Bold, AppColors.Text, xMargin, yPos));
			yPos += 50;

			// Ga đi
			pnlSearch.Controls.Add(CreateLabel("Ga đi", 10, FontStyle.Regular, AppColors.TextMuted, xMargin, yPos)); yPos += 30;
			txtDepStation = CreateSearchInput("Sài Gòn", "🚉", xMargin, yPos, inputW);
			pnlSearch.Controls.Add(txtDepStation); yPos += 70;

			// Ga đến
			pnlSearch.Controls.Add(CreateLabel("Ga đến", 10, FontStyle.Regular, AppColors.TextMuted, xMargin, yPos)); yPos += 30;
			txtArrStation = CreateSearchInput("Hà Nội", "🏁", xMargin, yPos, inputW);
			pnlSearch.Controls.Add(txtArrStation); yPos += 70;

			// Ngày đi
			pnlSearch.Controls.Add(CreateLabel("Ngày đi", 10, FontStyle.Regular, AppColors.TextMuted, xMargin, yPos)); yPos += 30;
			txtDate = CreateSearchInput("24/05/2024", "📅", xMargin, yPos, inputW);
			pnlSearch.Controls.Add(txtDate); yPos += 80;

			// Nút tìm kiếm
			RoundedButton btnSearch = new RoundedButton
			{
				Text = "🔍 TÌM KIẾM",
				Size = new Size(inputW, 50),
				Location = new Point(xMargin, yPos),
				BackColor = AppColors.Primary,
				ForeColor = Color.White
			};
			btnSearch.Click += BtnSearch_Click;
			pnlSearch.Controls.Add(btnSearch);
		}

		private void BtnSearch_Click(object sender, EventArgs e)
		{
			//TODO: Thực hiện tìm kiếm với các tham số
			string dep = txtDepStation.TextValue.Trim();
			string arr = txtArrStation.TextValue.Trim();

			// Cập nhật tiêu đề dựa trên input
			if (string.IsNullOrEmpty(dep) && string.IsNullOrEmpty(arr))
			{
				lblResultTitle.Text = "Kết quả tìm kiếm: Sài Gòn ➝ Hà Nội";
			}
			else
			{
				// Nếu thiếu 1 trong 2 thì điền "..."
				string d = string.IsNullOrEmpty(dep) ? "..." : dep;
				string a = string.IsNullOrEmpty(arr) ? "..." : arr;
				lblResultTitle.Text = $"Kết quả tìm kiếm: {d} ➝ {a}";
			}
		}

		private void SetupResultArea()
		{
			int contentX = 370;
			int contentW = this.Width - 400;

			// Title
			lblResultTitle = new Label
			{
				Text = "Kết quả tìm kiếm: Sài Gòn ➝ Hà Nội",
				Font = new Font("Segoe UI", 15, FontStyle.Bold),
				ForeColor = AppColors.Text,
				AutoSize = true,
				Location = new Point(contentX, 80)
			};
			this.Controls.Add(lblResultTitle);

			// Table Header Row
			Panel pnlTableHeader = new Panel
			{
				Size = new Size(contentW, 40),
				Location = new Point(contentX, 130),
				BackColor = Color.Transparent,
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
			};

			string[] headers = { "MÃ TÀU", "TÊN TÀU", "GIỜ ĐI", "GIỜ ĐẾN", "THỜI GIAN", "GIÁ VÉ", "TRẠNG THÁI", "" };
			int[] colWidths = { 100, 160, 100, 100, 150, 150, 120, 140 };
			int curX = 20;

			for (int i = 0; i < headers.Length; i++)
			{
				Label lblH = new Label
				{
					Text = headers[i],
					ForeColor = AppColors.TextMuted,
					Font = new Font("Segoe UI", 9, FontStyle.Bold),
					AutoSize = false,
					Size = new Size(colWidths[i], 30),
					Location = new Point(curX, 5),
					TextAlign = ContentAlignment.MiddleLeft
				};
				pnlTableHeader.Controls.Add(lblH);
				curX += colWidths[i];
			}
			this.Controls.Add(pnlTableHeader);

			// Results Container (FlowLayout)
			flowResults = new FlowLayoutPanel
			{
				Location = new Point(contentX, 170),
				Size = new Size(contentW + 50, this.Height - 200),
				FlowDirection = FlowDirection.LeftToRight,
				WrapContents = true,
				AutoScroll = true,
				BackColor = Color.Transparent,
				Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
			};
			this.Controls.Add(flowResults);
		}

		// =========================================================
		// 3. LOGIC & DỮ LIỆU
		// =========================================================

		private void LoadListTrain()
		{
			// TODO: Kết nối DB và load dữ liệu thật
			flowResults.SuspendLayout();
			flowResults.Controls.Clear();
			try
			{
				AddTrainItem("SE1", "Thống Nhất", "19:30", "04:50 (+1)", "33h 20m", "950.000đ", "10 ghế", 1);
				AddTrainItem("SE3", "Thống Nhất", "22:00", "06:00 (+1)", "32h 00m", "1.020.000đ", "3 ghế", 2);
				AddTrainItem("SE5", "Thống Nhất", "09:00", "20:05", "35h 05m", "980.000đ", "8 ghế", 1);
				AddTrainItem("SE7", "Thống Nhất", "06:00", "16:30", "34h 30m", "965.000đ", "Hết ghế", 3);
				AddTrainItem("TN1", "Tàu Nhanh", "14:00", "02:15 (+1)", "36h 15m", "850.000đ", "45 ghế", 1);
				AddTrainItem("TN2", "Tàu Chậm", "08:00", "22:00", "38h 00m", "750.000đ", "60 ghế", 1);
			}
			finally { flowResults.ResumeLayout(); }
		}

		private void AddTrainItem(string code, string name, string depTime, string arrTime, string duration, string price, string seatStatus, int statusType)
		{
			// Tính toán độ rộng item
			int w = flowResults.ClientSize.Width - 30;
			if (w < 1050) w = 1050;

			Panel pnlItem = new Panel
			{
				Size = new Size(w, 80),
				Margin = new Padding(0, 0, 0, 15),
				BackColor = Color.Transparent
			};

			// Vẽ nền item
			pnlItem.Paint += (s, e) =>
			{
				e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
				using (GraphicsPath path = RoundedButton.GetRoundedPath(new Rectangle(0, 0, w, 80), 12))
				using (SolidBrush brush = new SolidBrush(AppColors.CardBg)) // Dùng màu Card cho item nền
				{
					e.Graphics.FillPath(brush, path);
				}
			};

			int[] colWidths = { 100, 160, 100, 100, 150, 150, 120, 140 };
			int curX = 20;

			// Thêm các cột thông tin
			pnlItem.Controls.Add(CreateLabel(code, 11, FontStyle.Bold, AppColors.Text, curX, 30)); curX += colWidths[0];
			pnlItem.Controls.Add(CreateLabel(name, 10, FontStyle.Regular, AppColors.TextMuted, curX, 30)); curX += colWidths[1];
			pnlItem.Controls.Add(CreateLabel(depTime, 11, FontStyle.Regular, AppColors.Text, curX, 30)); curX += colWidths[2];
			pnlItem.Controls.Add(CreateLabel(arrTime, 11, FontStyle.Regular, AppColors.TextMuted, curX, 30)); curX += colWidths[3];
			pnlItem.Controls.Add(CreateLabel(duration, 10, FontStyle.Regular, AppColors.TextMuted, curX, 30)); curX += colWidths[4];
			pnlItem.Controls.Add(CreateLabel(price, 11, FontStyle.Bold, AppColors.Text, curX, 30)); curX += colWidths[5];

			// Trạng thái ghế (Màu sắc dựa trên statusType)
			Color statusColor = statusType == 1 ? ClrStatusGreen : (statusType == 2 ? ClrStatusOrange : ClrStatusRed);
			Label lblSeat = new Label
			{
				Text = seatStatus,
				ForeColor = statusColor,
				Font = new Font("Segoe UI", 9, FontStyle.Bold),
				AutoSize = true,
				Location = new Point(curX, 32)
			};
			pnlItem.Controls.Add(lblSeat);
			curX += colWidths[6];

			// Logic Nút Chọn
			if (statusType != 3) // Nếu còn vé
			{
				RoundedButton btnSelect = new RoundedButton
				{
					Text = "Chọn chuyến",
					Size = new Size(130, 40),
					Location = new Point(curX, 20),
					BackColor = AppColors.Primary,
					ForeColor = Color.White,
					Font = new Font("Segoe UI", 9, FontStyle.Bold)
				};

				// --- CHUYỂN SANG FORM BOOKING ---
				btnSelect.Click += (s, e) => {
					var bookingForm = new client.Forms.Booking.Booking(code, name, price);
					bookingForm.ShowDialog();
				};
				pnlItem.Controls.Add(btnSelect);
			}
			else // Hết vé
			{
				RoundedButton btnSoldOut = new RoundedButton
				{
					Text = "Hết vé",
					Size = new Size(130, 40),
					Location = new Point(curX, 20),
					BackColor = AppColors.InputBg, // Màu xám chìm
					ForeColor = Color.Gray,
					Font = new Font("Segoe UI", 9, FontStyle.Regular),
					Enabled = false
				};
				pnlItem.Controls.Add(btnSoldOut);
			}

			flowResults.Controls.Add(pnlItem);
		}

		private void OpenProfile()
		{
			this.Hide();
			// Đảm bảo namespace Profile đúng với project của bạn
			var profileForm = new client.Forms.Profile.Profile();
			profileForm.ShowDialog();
			this.Show();
		}

		// =========================================================
		// 4. HÀM HỖ TRỢ (HELPERS)
		// =========================================================

		private ModernTextBox CreateSearchInput(string placeholder, string icon, int x, int y, int w)
		{
			return new ModernTextBox
			{
				Location = new Point(x, y),
				Size = new Size(w, 45),
				PlaceholderText = placeholder,
				IconText = icon,
				BackColor = AppColors.Background,
				ForeColor = AppColors.Text
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

		private void AddWindowControls(Panel parent)
		{
			int btnW = 45;
			int startX = parent.Width - (btnW * 3) - 10;

			Label btnClose = CreateWindowButton("✕", startX + (btnW * 2));
			btnClose.Click += (s, e) => Application.Exit();
			btnClose.MouseEnter += (s, e) => btnClose.BackColor = AppColors.CloseHover;
			btnClose.MouseLeave += (s, e) => btnClose.BackColor = Color.Transparent;

			Label btnMax = CreateWindowButton("☐", startX + btnW);
			btnMax.Click += (s, e) => ToggleMaximize();
			btnMax.MouseEnter += (s, e) => btnMax.BackColor = AppColors.HeaderHover;
			btnMax.MouseLeave += (s, e) => btnMax.BackColor = Color.Transparent;

			Label btnMin = CreateWindowButton("―", startX);
			btnMin.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
			btnMin.MouseEnter += (s, e) => btnMin.BackColor = AppColors.HeaderHover;
			btnMin.MouseLeave += (s, e) => btnMin.BackColor = Color.Transparent;

			parent.Controls.Add(btnClose);
			parent.Controls.Add(btnMax);
			parent.Controls.Add(btnMin);
		}

		private Label CreateWindowButton(string text, int x)
		{
			return new Label
			{
				Text = text,
				Font = new Font("Segoe UI", 11, FontStyle.Regular),
				ForeColor = Color.White,
				AutoSize = false,
				Size = new Size(45, 30),
				Location = new Point(x, 15),
				TextAlign = ContentAlignment.MiddleCenter,
				Cursor = Cursors.Hand,
				Anchor = AnchorStyles.Top | AnchorStyles.Right
			};
		}

		private void ToggleMaximize()
		{
			if (isMaximized)
			{
				this.WindowState = FormWindowState.Normal;
				this.Size = new Size(1500, 850);
				this.CenterToScreen();
			}
			else
			{
				this.WindowState = FormWindowState.Maximized;
			}
			isMaximized = !isMaximized;
		}

		// Kéo thả Window
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		public static extern bool ReleaseCapture();
		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left) { ReleaseCapture(); SendMessage(Handle, 0xA1, 0x2, 0); }
		}
	}
}