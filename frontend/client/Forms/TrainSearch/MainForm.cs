using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using client.Forms.Authentication; // Sử dụng lại RoundedButton & ModernTextBox
using client.Forms.Booking;        // QUAN TRỌNG: Để gọi BookingForm

namespace client.Forms.TrainSearch
{
	public partial class MainForm : Form
	{
		// =========================================================
		// 1. CẤU HÌNH MÀU SẮC (THEME DARK MODE)
		// =========================================================
		private readonly Color ClrBackground = Color.FromArgb(15, 23, 42);      // Nền chính
		private readonly Color ClrSidebar = Color.FromArgb(30, 41, 59);         // Nền sidebar
		private readonly Color ClrHeader = Color.FromArgb(15, 23, 42);          // Nền Header
		private readonly Color ClrItemBg = Color.FromArgb(30, 41, 59);          // Nền item kết quả
		private readonly Color ClrText = Color.White;                           // Chữ trắng
		private readonly Color ClrTextGray = Color.FromArgb(148, 163, 184);     // Chữ xám
		private readonly Color ClrAccent = Color.FromArgb(37, 99, 235);         // Màu xanh dương
		private readonly Color ClrGreen = Color.FromArgb(34, 197, 94);          // Màu xanh lá
		private readonly Color ClrOrange = Color.FromArgb(249, 115, 22);        // Màu cam
		private readonly Color ClrRed = Color.FromArgb(239, 68, 68);            // Màu đỏ

		// Biến toàn cục
		private FlowLayoutPanel flowResults;
		private ModernTextBox txtDepStation, txtArrStation, txtDate;
		private bool isMaximized = false;

		public MainForm()
		{
			InitializeComponent();
			SetupUI();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			LoadDummyData();
		}

		// =========================================================
		// 2. HÀM DỰNG GIAO DIỆN (SETUP UI)
		// =========================================================
		private void SetupUI()
		{
			this.FormBorderStyle = FormBorderStyle.None;
			this.StartPosition = FormStartPosition.CenterScreen;
			this.Size = new Size(1500, 850);
			this.BackColor = ClrBackground;
			this.DoubleBuffered = true;

			// --- HEADER ---
			Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = ClrHeader, Padding = new Padding(20, 0, 20, 0) };
			Label lblLogo = new Label { Text = "🚆 Vé Tàu Cao Tốc", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = ClrAccent, AutoSize = true, Location = new Point(20, 15) };
			pnlHeader.Controls.Add(lblLogo);
			AddWindowControls(pnlHeader); // Nút điều khiển cửa sổ

			string[] menuItems = { "Đăng xuất", "Tài khoản"};
			int menuX = pnlHeader.Width - 160;
			foreach (var item in menuItems)
			{
				Label lblMenu = new Label { Text = item, Font = new Font("Segoe UI", 10, FontStyle.Regular), ForeColor = ClrTextGray, AutoSize = true, Cursor = Cursors.Hand, Anchor = AnchorStyles.Top | AnchorStyles.Right };
				lblMenu.Location = new Point(menuX - 80, 20);
				lblMenu.MouseEnter += (s, e) => lblMenu.ForeColor = ClrAccent;
				lblMenu.MouseLeave += (s, e) => lblMenu.ForeColor = ClrTextGray;

				// --- CẬP NHẬT TẠI ĐÂY ---
				if (item == "Đăng xuất")
					lblMenu.Click += (s, e) => Application.Exit();
				else if (item == "Tài khoản")
					lblMenu.Click += (s, e) => {
						// 1. Ẩn form chính đi (Cảm giác như đã tắt)
						this.Hide();

						// 2. Mở form Profile
						var profileForm = new client.Forms.Profile.Profile();
						profileForm.ShowDialog(); // Chương trình sẽ dừng ở dòng này chờ user đóng Profile

						// 3. Khi user đóng Profile, dòng này mới chạy -> Hiện lại form chính
						this.Show();
					};
				// -------------------------

				pnlHeader.Controls.Add(lblMenu);
				menuX -= 100;
			}
			this.Controls.Add(pnlHeader);

			// --- SIDEBAR ---
			int sidebarW = 320;
			Panel pnlSearch = new Panel { Size = new Size(sidebarW, 600), Location = new Point(30, 80), BackColor = Color.Transparent, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom };
			pnlSearch.Paint += (s, e) => {
				e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
				using (GraphicsPath path = GetRoundedPath(new Rectangle(0, 0, pnlSearch.Width, pnlSearch.Height - 20), 15))
				using (SolidBrush brush = new SolidBrush(ClrSidebar)) { e.Graphics.FillPath(brush, path); }
			};
			this.Controls.Add(pnlSearch);

			int yPos = 30; int xMargin = 25; int inputW = sidebarW - (xMargin * 2);
			pnlSearch.Controls.Add(CreateLabel("Tìm chuyến tàu", 14, FontStyle.Bold, ClrText, xMargin, yPos)); yPos += 50;

			pnlSearch.Controls.Add(CreateLabel("Ga đi", 10, FontStyle.Regular, ClrTextGray, xMargin, yPos)); yPos += 30;
			txtDepStation = new ModernTextBox { Location = new Point(xMargin, yPos), Size = new Size(inputW, 45), PlaceholderText = "Sài Gòn", IconText = "🚉", BackColor = ClrBackground, ForeColor = ClrText };
			pnlSearch.Controls.Add(txtDepStation); yPos += 70;

			pnlSearch.Controls.Add(CreateLabel("Ga đến", 10, FontStyle.Regular, ClrTextGray, xMargin, yPos)); yPos += 30;
			txtArrStation = new ModernTextBox { Location = new Point(xMargin, yPos), Size = new Size(inputW, 45), PlaceholderText = "Hà Nội", IconText = "🏁", BackColor = ClrBackground, ForeColor = ClrText };
			pnlSearch.Controls.Add(txtArrStation); yPos += 70;

			pnlSearch.Controls.Add(CreateLabel("Ngày đi", 10, FontStyle.Regular, ClrTextGray, xMargin, yPos)); yPos += 30;
			txtDate = new ModernTextBox { Location = new Point(xMargin, yPos), Size = new Size(inputW, 45), PlaceholderText = "24/05/2024", IconText = "📅", BackColor = ClrBackground, ForeColor = ClrText };
			pnlSearch.Controls.Add(txtDate); yPos += 80;

			RoundedButton btnSearch = new RoundedButton { Text = "🔍 TÌM KIẾM", Size = new Size(inputW, 50), Location = new Point(xMargin, yPos), BackColor = ClrAccent, ForeColor = Color.White, Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand, FlatStyle = FlatStyle.Flat };
			btnSearch.FlatAppearance.BorderSize = 0;
			btnSearch.Click += BtnSearch_Click;
			pnlSearch.Controls.Add(btnSearch);

			// --- KẾT QUẢ TÌM KIẾM ---
			int contentX = 370; int contentW = this.Width - 400;
			Label lblResultTitle = new Label { Text = "Kết quả tìm kiếm: Sài Gòn ➝ Hà Nội", Font = new Font("Segoe UI", 15, FontStyle.Bold), ForeColor = ClrText, AutoSize = true, Location = new Point(contentX, 80) };
			this.Controls.Add(lblResultTitle);

			Panel pnlTableHeader = new Panel { Size = new Size(contentW, 40), Location = new Point(contentX, 130), BackColor = Color.Transparent, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
			string[] headers = { "MÃ TÀU", "TÊN TÀU", "GIỜ ĐI", "GIỜ ĐẾN", "THỜI GIAN", "GIÁ VÉ", "TRẠNG THÁI", "" };
			int[] colWidths = { 100, 160, 100, 100, 150, 150, 120, 140 };
			int curX = 20;
			for (int i = 0; i < headers.Length; i++)
			{
				Label lblH = new Label { Text = headers[i], ForeColor = ClrTextGray, Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = false, Size = new Size(colWidths[i], 30), Location = new Point(curX, 5), TextAlign = ContentAlignment.MiddleLeft };
				pnlTableHeader.Controls.Add(lblH); curX += colWidths[i];
			}
			this.Controls.Add(pnlTableHeader);

			flowResults = new FlowLayoutPanel { Location = new Point(contentX, 170), Size = new Size(contentW + 50, this.Height - 200), FlowDirection = FlowDirection.LeftToRight, WrapContents = true, AutoScroll = true, BackColor = Color.Transparent, Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right };
			this.Controls.Add(flowResults);
		}

		private void LoadDummyData()
		{
			flowResults.SuspendLayout(); flowResults.Controls.Clear();
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

		// =========================================================
		// 3. XỬ LÝ LOGIC CHỌN VÉ & MỞ BOOKING
		// =========================================================
		private void AddTrainItem(string code, string name, string depTime, string arrTime, string duration, string price, string seatStatus, int statusType)
		{
			int w = flowResults.ClientSize.Width - 30; if (w < 1050) w = 1050;
			Panel pnlItem = new Panel { Size = new Size(w, 80), Margin = new Padding(0, 0, 0, 15), BackColor = Color.Transparent };
			pnlItem.Paint += (s, e) => { e.Graphics.SmoothingMode = SmoothingMode.AntiAlias; using (GraphicsPath path = GetRoundedPath(new Rectangle(0, 0, w, 80), 12)) using (SolidBrush brush = new SolidBrush(ClrItemBg)) { e.Graphics.FillPath(brush, path); } };

			int[] colWidths = { 100, 160, 100, 100, 150, 150, 120, 140 };
			int curX = 20;

			pnlItem.Controls.Add(CreateLabel(code, 11, FontStyle.Bold, ClrText, curX, 30)); curX += colWidths[0];
			pnlItem.Controls.Add(CreateLabel(name, 10, FontStyle.Regular, ClrTextGray, curX, 30)); curX += colWidths[1];
			pnlItem.Controls.Add(CreateLabel(depTime, 11, FontStyle.Regular, ClrText, curX, 30)); curX += colWidths[2];
			pnlItem.Controls.Add(CreateLabel(arrTime, 11, FontStyle.Regular, ClrTextGray, curX, 30)); curX += colWidths[3];
			pnlItem.Controls.Add(CreateLabel(duration, 10, FontStyle.Regular, ClrTextGray, curX, 30)); curX += colWidths[4];
			pnlItem.Controls.Add(CreateLabel(price, 11, FontStyle.Bold, ClrText, curX, 30)); curX += colWidths[5];

			Label lblSeat = new Label { Text = seatStatus, ForeColor = statusType == 1 ? ClrGreen : (statusType == 2 ? ClrOrange : ClrRed), Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = true, Location = new Point(curX, 32) };
			pnlItem.Controls.Add(lblSeat); curX += colWidths[6];

			// --- PHẦN LOGIC NÚT BẤM ĐƯỢC CHỈNH SỬA TẠI ĐÂY ---
			if (statusType != 3) // Nếu còn vé
			{
				RoundedButton btnSelect = new RoundedButton
				{
					Text = "Chọn chuyến",
					Size = new Size(130, 40),
					Location = new Point(curX, 20),
					BackColor = ClrAccent,
					ForeColor = Color.White,
					Font = new Font("Segoe UI", 9, FontStyle.Bold),
					Cursor = Cursors.Hand,
					FlatStyle = FlatStyle.Flat
				};
				btnSelect.FlatAppearance.BorderSize = 0;

				// --- CẬP NHẬT TẠI ĐÂY: Truyền dữ liệu sang form Booking ---
				btnSelect.Click += (s, e) => {
					// Truyền Mã tàu, Tên tàu, Giá vé vào Constructor mới
					var bookingForm = new client.Forms.Booking.Booking(code, name, price);
					bookingForm.ShowDialog();
				};
				// -----------------------------------------------------------

				pnlItem.Controls.Add(btnSelect);
			}
			else // Nếu hết vé
			{
				RoundedButton btnSoldOut = new RoundedButton { Text = "Hết vé", Size = new Size(130, 40), Location = new Point(curX, 20), BackColor = Color.FromArgb(51, 65, 85), ForeColor = Color.Gray, Font = new Font("Segoe UI", 9, FontStyle.Regular), Enabled = false, FlatStyle = FlatStyle.Flat };
				btnSoldOut.FlatAppearance.BorderSize = 0;
				pnlItem.Controls.Add(btnSoldOut);
			}
			// ------------------------------------------------

			flowResults.Controls.Add(pnlItem);
		}

		private void AddWindowControls(Panel parent)
		{
			int btnW = 45; int startX = parent.Width - (btnW * 3) - 10;
			Label btnClose = CreateWindowButton("✕", startX + (btnW * 2), ClrRed); btnClose.Click += (s, e) => Application.Exit(); btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right; parent.Controls.Add(btnClose);
			Label btnMax = CreateWindowButton("☐", startX + btnW, ClrHeader); btnMax.Click += (s, e) => ToggleMaximize(); btnMax.Anchor = AnchorStyles.Top | AnchorStyles.Right; parent.Controls.Add(btnMax);
			Label btnMin = CreateWindowButton("―", startX, ClrHeader); btnMin.Click += (s, e) => this.WindowState = FormWindowState.Minimized; btnMin.Anchor = AnchorStyles.Top | AnchorStyles.Right; parent.Controls.Add(btnMin);
		}
		private Label CreateWindowButton(string text, int x, Color hoverColor)
		{
			Label lbl = new Label { Text = text, Font = new Font("Segoe UI", 11, FontStyle.Regular), ForeColor = Color.White, AutoSize = false, Size = new Size(45, 30), Location = new Point(x, 15), TextAlign = ContentAlignment.MiddleCenter, Cursor = Cursors.Hand };
			lbl.MouseEnter += (s, e) => lbl.BackColor = (text == "✕") ? ClrRed : Color.FromArgb(51, 65, 85); lbl.MouseLeave += (s, e) => lbl.BackColor = Color.Transparent; return lbl;
		}
		private void ToggleMaximize() { if (isMaximized) { this.WindowState = FormWindowState.Normal; this.Size = new Size(1500, 850); this.CenterToScreen(); } else { this.WindowState = FormWindowState.Maximized; } isMaximized = !isMaximized; }
		private void BtnSearch_Click(object sender, EventArgs e) { MessageBox.Show($"Tìm kiếm: {txtDepStation.TextValue} -> {txtArrStation.TextValue}", "Đang xử lý"); }
		private Label CreateLabel(string text, float size, FontStyle style, Color color, int x, int y) { return new Label { Text = text, Font = new Font("Segoe UI", size, style), ForeColor = color, AutoSize = true, Location = new Point(x, y) }; }
		public static GraphicsPath GetRoundedPath(Rectangle rect, int radius) { GraphicsPath path = new GraphicsPath(); float d = radius * 2F; path.AddArc(rect.X, rect.Y, d, d, 180, 90); path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90); path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90); path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90); path.CloseFigure(); return path; }
		[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
		[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern bool ReleaseCapture();
		protected override void OnMouseDown(MouseEventArgs e) { if (e.Button == MouseButtons.Left) { ReleaseCapture(); SendMessage(Handle, 0xA1, 0x2, 0); } }
	}
}