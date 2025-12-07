using client.Forms.Authentication;
using client.Forms.Booking;
using client.Services;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace client.Forms.TrainSearch
{
	public partial class MainForm : Form
	{
		// =========================================================
		// 1. CẤU HÌNH MÀU SẮC (THEME DARK MODE)
		// =========================================================
		private readonly Color _clrBackground = Color.FromArgb(15, 23, 42); // Nền chính
		private readonly Color _clrSidebar = Color.FromArgb(30, 41, 59); // Nền sidebar
		private readonly Color _clrHeader = Color.FromArgb(15, 23, 42); // Nền Header
		private readonly Color _clrItemBg = Color.FromArgb(30, 41, 59); // Nền item kết quả
		private readonly Color _clrText = Color.White; // Chữ trắng
		private readonly Color _clrTextGray = Color.FromArgb(148, 163, 184); // Chữ xám
		private readonly Color _clrAccent = Color.FromArgb(37, 99, 235); // Màu xanh dương
		private readonly Color _clrGreen = Color.FromArgb(34, 197, 94); // Màu xanh lá
		private readonly Color _clrOrange = Color.FromArgb(249, 115, 22); // Màu cam
		private readonly Color _clrRed = Color.FromArgb(239, 68, 68); // Màu đỏ

		// Biến toàn cục
		private FlowLayoutPanel _flowResults;
		private ModernTextBox _txtDepStation, _txtArrStation, _txtDate;
		private bool _isMaximized;

		public MainForm()
		{
			InitializeComponent();
			SetupUi();
		}

		// =========================================================
		// 2. HÀM DỰNG GIAO DIỆN (SETUP UI)
		// =========================================================
		private void SetupUi()
		{
			this.FormBorderStyle = FormBorderStyle.None;
			this.StartPosition = FormStartPosition.CenterScreen;
			this.Size = new Size(1500, 850);
			this.BackColor = _clrBackground;
			this.DoubleBuffered = true;

			// --- HEADER ---
			Panel pnlHeader = new Panel
			{
				Dock = DockStyle.Top, Height = 60, BackColor = _clrHeader, Padding = new Padding(20, 0, 20, 0)
			};
			Label lblLogo = new Label
			{
				Text = "🚆 Vé Tàu Cao Tốc",
				Font = new Font("Segoe UI", 14, FontStyle.Bold),
				ForeColor = _clrAccent,
				AutoSize = true,
				Location = new Point(20, 15)
			};
			pnlHeader.Controls.Add(lblLogo);
			AddWindowControls(pnlHeader); // Nút điều khiển cửa sổ

			// Display username from session
			var currentUser = SessionManager.Instance.CurrentUser;
			if (currentUser != null)
			{
				Label lblUsername = new Label
				{
					Text = $"👤 {currentUser.Username}",
					Font = new Font("Segoe UI", 10, FontStyle.Bold),
					ForeColor = _clrAccent,
					AutoSize = true,
					Anchor = AnchorStyles.Top | AnchorStyles.Right
				};
				lblUsername.Location = new Point(pnlHeader.Width - 280, 20);
				pnlHeader.Controls.Add(lblUsername);
			}

			string[] menuItems = { "Đăng xuất", "Tài khoản" };
			int menuX = pnlHeader.Width - 160;
			foreach (var item in menuItems)
			{
				Label lblMenu = new Label
				{
					Text = item,
					Font = new Font("Segoe UI", 10, FontStyle.Regular),
					ForeColor = _clrTextGray,
					AutoSize = true,
					Cursor = Cursors.Hand,
					Anchor = AnchorStyles.Top | AnchorStyles.Right
				};
				lblMenu.Location = new Point(menuX - 80, 20);
				lblMenu.MouseEnter += (_, _) => lblMenu.ForeColor = _clrAccent;
				lblMenu.MouseLeave += (_, _) => lblMenu.ForeColor = _clrTextGray;

				if (item == "Đăng xuất")
				{
					lblMenu.Click += HandleLogout;
				}
				else if (item == "Tài khoản")
				{
					lblMenu.Click += (_, _) =>
					{
						this.Hide();
						var profileForm = new client.Forms.Profile.Profile();
						profileForm.ShowDialog();
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
				Panel pnlSearch = new Panel
				{
					Size = new Size(sidebarW, 600),
					Location = new Point(30, 80),
					BackColor = Color.Transparent,
					Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom
				};
				pnlSearch.Paint += (_, e) =>
				{
					e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
					using GraphicsPath path = GetRoundedPath(
						new Rectangle(0, 0, pnlSearch.Width, pnlSearch.Height - 20),
						15);
					using (SolidBrush brush = new SolidBrush(_clrSidebar)) { e.Graphics.FillPath(brush, path); }
				};
				this.Controls.Add(pnlSearch);

				int yPos = 30;
				int xMargin = 25;
				int inputW = sidebarW - (xMargin * 2);
				pnlSearch.Controls.Add(CreateLabel("Tìm chuyến tàu", 14, FontStyle.Bold, _clrText, xMargin, yPos));
				yPos += 50;

				pnlSearch.Controls.Add(CreateLabel("Ga đi", 10, FontStyle.Regular, _clrTextGray, xMargin, yPos));
				yPos += 30;
				_txtDepStation = new ModernTextBox
				{
					Location = new Point(xMargin, yPos),
					Size = new Size(inputW, 45),
					PlaceholderText = "Sài Gòn",
					IconText = "🚉",
					BackColor = _clrBackground,
					ForeColor = _clrText
				};
				pnlSearch.Controls.Add(_txtDepStation);
				yPos += 70;

				pnlSearch.Controls.Add(CreateLabel("Ga đến", 10, FontStyle.Regular, _clrTextGray, xMargin, yPos));
				yPos += 30;
				_txtArrStation = new ModernTextBox
				{
					Location = new Point(xMargin, yPos),
					Size = new Size(inputW, 45),
					PlaceholderText = "Hà Nội",
					IconText = "🏁",
					BackColor = _clrBackground,
					ForeColor = _clrText
				};
				pnlSearch.Controls.Add(_txtArrStation);
				yPos += 70;

				pnlSearch.Controls.Add(CreateLabel("Ngày đi", 10, FontStyle.Regular, _clrTextGray, xMargin, yPos));
				yPos += 30;
				_txtDate = new ModernTextBox
				{
					Location = new Point(xMargin, yPos),
					Size = new Size(inputW, 45),
					PlaceholderText = "24/05/2024",
					IconText = "📅",
					BackColor = _clrBackground,
					ForeColor = _clrText
				};
				pnlSearch.Controls.Add(_txtDate);
				yPos += 80;

				RoundedButton btnSearch = new RoundedButton
				{
					Text = "🔍 TÌM KIẾM",
					Size = new Size(inputW, 50),
					Location = new Point(xMargin, yPos),
					BackColor = _clrAccent,
					ForeColor = Color.White,
					Font = new Font("Segoe UI", 11, FontStyle.Bold),
					Cursor = Cursors.Hand,
					FlatStyle = FlatStyle.Flat
				};
				btnSearch.FlatAppearance.BorderSize = 0;
				btnSearch.Click += BtnSearch_Click;
				pnlSearch.Controls.Add(btnSearch);

				// --- KẾT QUẢ TÌM KIẾM ---
				int contentX = 370;
				int contentW = this.Width - 400;
				Label lblResultTitle = new Label
				{
					Text = "Kết quả tìm kiếm: Sài Gòn ➝ Hà Nội",
					Font = new Font("Segoe UI", 15, FontStyle.Bold),
					ForeColor = _clrText,
					AutoSize = true,
					Location = new Point(contentX, 80)
				};
				this.Controls.Add(lblResultTitle);

				Panel pnlTableHeader = new Panel
				{
					Size = new Size(contentW, 40),
					Location = new Point(contentX, 130),
					BackColor = Color.Transparent,
					Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
				};
				string[] headers =
				{
					"MÃ TÀU", "TÊN TÀU", "GIỜ ĐI", "GIỜ ĐẾN", "THỜI GIAN", "GIÁ VÉ", "TRẠNG THÁI", ""
				};
				int[] colWidths = { 100, 160, 100, 100, 150, 150, 120, 140 };
				int curX = 20;
				for (int i = 0; i < headers.Length; i++)
				{
					Label lblH = new Label
					{
						Text = headers[i],
						ForeColor = _clrTextGray,
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

				_flowResults = new FlowLayoutPanel
				{
					Location = new Point(contentX, 170),
					Size = new Size(contentW + 50, this.Height - 200),
					FlowDirection = FlowDirection.LeftToRight,
					WrapContents = true,
					AutoScroll = true,
					BackColor = Color.Transparent,
					Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
				};
				this.Controls.Add(_flowResults);
			}
		}


		// =========================================================
		// 3. XỬ LÝ LOGIC CHỌN VÉ & MỞ BOOKING
		// =========================================================
		private void AddTrainItem(string code, string name, string depTime, string arrTime, string duration,
			string price, string seatStatus, int statusType)
		{
			int w = _flowResults.ClientSize.Width - 30;
			if (w < 1050) w = 1050;
			Panel pnlItem = new Panel
			{
				Size = new Size(w, 80), Margin = new Padding(0, 0, 0, 15), BackColor = Color.Transparent
			};
			pnlItem.Paint += (_, e) =>
			{
				e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
				using GraphicsPath path = GetRoundedPath(new Rectangle(0, 0, w, 80), 12);
				using SolidBrush brush = new SolidBrush(_clrItemBg);
				e.Graphics.FillPath(brush, path);
			};

			int[] colWidths = { 100, 160, 100, 100, 150, 150, 120, 140 };
			int curX = 20;

			pnlItem.Controls.Add(CreateLabel(code, 11, FontStyle.Bold, _clrText, curX, 30));
			curX += colWidths[0];
			pnlItem.Controls.Add(CreateLabel(name, 10, FontStyle.Regular, _clrTextGray, curX, 30));
			curX += colWidths[1];
			pnlItem.Controls.Add(CreateLabel(depTime, 11, FontStyle.Regular, _clrText, curX, 30));
			curX += colWidths[2];
			pnlItem.Controls.Add(CreateLabel(arrTime, 11, FontStyle.Regular, _clrTextGray, curX, 30));
			curX += colWidths[3];
			pnlItem.Controls.Add(CreateLabel(duration, 10, FontStyle.Regular, _clrTextGray, curX, 30));
			curX += colWidths[4];
			pnlItem.Controls.Add(CreateLabel(price, 11, FontStyle.Bold, _clrText, curX, 30));
			curX += colWidths[5];

			Label lblSeat = new Label
			{
				Text = seatStatus,
				ForeColor = statusType == 1 ? _clrGreen : (statusType == 2 ? _clrOrange : _clrRed),
				Font = new Font("Segoe UI", 9, FontStyle.Bold),
				AutoSize = true,
				Location = new Point(curX, 32)
			};
			pnlItem.Controls.Add(lblSeat);
			curX += colWidths[6];

			// --- PHẦN LOGIC NÚT BẤM ĐƯỢC CHỈNH SỬA TẠI ĐÂY ---
			if (statusType != 3) // Nếu còn vé
			{
				RoundedButton btnSelect = new RoundedButton
				{
					Text = "Chọn chuyến",
					Size = new Size(130, 40),
					Location = new Point(curX, 20),
					BackColor = _clrAccent,
					ForeColor = Color.White,
					Font = new Font("Segoe UI", 9, FontStyle.Bold),
					Cursor = Cursors.Hand,
					FlatStyle = FlatStyle.Flat
				};
				btnSelect.FlatAppearance.BorderSize = 0;

				// --- CẬP NHẬT TẠI ĐÂY: Truyền dữ liệu sang form Booking ---
				btnSelect.Click += (_, _) =>
				{
					// Truyền Mã tàu, Tên tàu, Giá vé vào Constructor mới
					var bookingForm = new client.Forms.Booking.Booking(code, name, price);
					bookingForm.ShowDialog();
				};
				// -----------------------------------------------------------

				pnlItem.Controls.Add(btnSelect);
			}
			else // Nếu hết vé
			{
				RoundedButton btnSoldOut = new RoundedButton
				{
					Text = "Hết vé",
					Size = new Size(130, 40),
					Location = new Point(curX, 20),
					BackColor = Color.FromArgb(51, 65, 85),
					ForeColor = Color.Gray,
					Font = new Font("Segoe UI", 9, FontStyle.Regular),
					Enabled = false,
					FlatStyle = FlatStyle.Flat
				};
				btnSoldOut.FlatAppearance.BorderSize = 0;
				pnlItem.Controls.Add(btnSoldOut);
			}
			// ------------------------------------------------

			_flowResults.Controls.Add(pnlItem);
		}

		private void AddWindowControls(Panel parent)
		{
			int btnW = 45;
			int startX = parent.Width - (btnW * 3) - 10;
			Label btnClose = CreateWindowButton("✕", startX + (btnW * 2));
			btnClose.Click += (_, _) => Application.Exit();
			btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			parent.Controls.Add(btnClose);
			Label btnMax = CreateWindowButton("☐", startX + btnW);
			btnMax.Click += (_, _) => ToggleMaximize();
			btnMax.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			parent.Controls.Add(btnMax);
			Label btnMin = CreateWindowButton("―", startX);
			btnMin.Click += (_, _) => this.WindowState = FormWindowState.Minimized;
			btnMin.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			parent.Controls.Add(btnMin);
		}

		private Label CreateWindowButton(string text, int x)
		{
			Label lbl = new Label
			{
				Text = text,
				Font = new Font("Segoe UI", 11, FontStyle.Regular),
				ForeColor = Color.White,
				AutoSize = false,
				Size = new Size(45, 30),
				Location = new Point(x, 15),
				TextAlign = ContentAlignment.MiddleCenter,
				Cursor = Cursors.Hand
			};
			lbl.MouseEnter += (_, _) => lbl.BackColor = (text == "✕") ? _clrRed : Color.FromArgb(51, 65, 85);
			lbl.MouseLeave += (_, _) => lbl.BackColor = Color.Transparent;
			return lbl;
		}

		private void HandleLogout(object? sender, EventArgs e)
		{
			var result = MessageBox.Show(
				"Bạn có chắc chắn muốn đăng xuất?",
				"Xác nhận đăng xuất",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question
			);

			if (result == DialogResult.Yes)
			{
				SessionManager.Instance.ClearSession();

				this.Hide();
				var loginForm = new Login();
				loginForm.FormClosed += (_, _) => this.Close();
				loginForm.Show();
			}
		}

		private void ToggleMaximize()
		{
			if (_isMaximized)
			{
				this.WindowState = FormWindowState.Normal;
				this.Size = new Size(1500, 850);
				this.CenterToScreen();
			}
			else { this.WindowState = FormWindowState.Maximized; }

			_isMaximized = !_isMaximized;
		}

		private void BtnSearch_Click(object sender, EventArgs e)
		{
			MessageBox.Show($"Tìm kiếm: {_txtDepStation.TextValue} -> {_txtArrStation.TextValue}", "Đang xử lý");
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