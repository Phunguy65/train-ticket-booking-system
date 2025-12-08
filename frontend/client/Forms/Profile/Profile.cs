using client.Controls;
using client.Helpers;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Xml.Linq;

namespace client.Forms.Profile
{
	public partial class Profile : Form
	{
		// =========================================================
		// 1. CẤU HÌNH MÀU SẮC RIÊNG (STATUS COLORS)
		// =========================================================
		// Các màu này dùng riêng cho trạng thái vé, giữ lại ở đây
		private readonly Color ClrSuccess = Color.FromArgb(34, 197, 94);  // Green
		private readonly Color ClrWarning = Color.FromArgb(249, 115, 22); // Orange
		private readonly Color ClrError = Color.FromArgb(239, 68, 68);    // Red

		// Biến UI Control quản lý Tab
		private Panel pnlContent;
		private Label btnTabHistory, btnTabProfile;
		private Panel lineActiveTab;
		private bool isMaximized = false;

		// Cấu hình cột bảng Lịch sử: [Mã vé, Tàu, Ngày đi, Trạng thái, Giá tiền]
		private readonly int[] colWidths = { 200, 350, 250, 250, 200 };

		public Profile()
		{
			InitializeComponent();
			SetupForm();
			SetupUI();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			// Mặc định vào tab Lịch sử
			SwitchTab("HISTORY");
		}

		// =========================================================
		// 2. DỰNG KHUNG GIAO DIỆN CHUNG
		// =========================================================
		private void SetupForm()
		{
			this.FormBorderStyle = FormBorderStyle.None;
			this.StartPosition = FormStartPosition.CenterScreen;
			this.Size = new Size(1500, 850);
			this.BackColor = AppColors.Background;
			this.DoubleBuffered = true;
		}

		private void SetupUI()
		{
			// Cấu hình Form cơ bản
			this.FormBorderStyle = FormBorderStyle.None;
			this.StartPosition = FormStartPosition.CenterScreen;
			this.Size = new Size(1500, 850);
			this.BackColor = AppColors.Background;
			this.DoubleBuffered = true;

			// =========================================================================
			// SẮP XẾP LẠI THỨ TỰ GỌI HÀM (QUAN TRỌNG)
			// Add từ dưới lên trên: Content -> Tabs -> Title -> Header (Cái sau đè lên cái trước)
			// =========================================================================

			// 1. ADD CONTENT (Lớp dưới cùng - Fill toàn bộ phần còn lại)
			pnlContent = new Panel { Dock = DockStyle.Fill, Padding = new Padding(50, 20, 50, 30) };
			this.Controls.Add(pnlContent);

			// 2. ADD TABS (Lớp thứ 2 - Dock Top)
			// Hàm này sẽ tự động tạo Panel và Add vào Form
			SetupTabs();

			// 3. ADD PAGE TITLE (Lớp thứ 3 - Dock Top)
			// Phần này chưa tách hàm nên viết trực tiếp ở đây
			Panel pnlPageTitle = new Panel { Dock = DockStyle.Top, Height = 70, Padding = new Padding(50, 0, 50, 0) };
			Label lblPageTitle = new Label
			{
				Text = "Quản lý tài khoản",
				Font = new Font("Segoe UI", 24, FontStyle.Bold),
				ForeColor = AppColors.Text,
				AutoSize = true,
				Location = new Point(50, 10)
			};
			pnlPageTitle.Controls.Add(lblPageTitle);
			this.Controls.Add(pnlPageTitle);

			// 4. ADD HEADER (Lớp trên cùng - Dock Top)
			// Gọi hàm này cuối cùng để Header luôn nằm trên đỉnh màn hình
			SetupHeader();
		}

		private void SetupHeader()
		{
			Panel pnlHeader = new Panel
			{
				Dock = DockStyle.Top,
				Height = 60,
				Padding = new Padding(30, 0, 30, 0),
				BackColor = AppColors.CardBg
			};

			// Logo
			Label lblLogo = new Label
			{
				Text = "🚆 Vé Tàu Cao Tốc",
				Font = new Font("Segoe UI", 16, FontStyle.Bold),
				ForeColor = AppColors.Primary,
				AutoSize = true,
				Location = new Point(30, 15),
				Cursor = Cursors.Hand
			};
			lblLogo.Click += (s, e) => this.Close();
			pnlHeader.Controls.Add(lblLogo);

			// Controls
			AddWindowControls(pnlHeader);
			SetupHeaderMenu(pnlHeader);

			this.Controls.Add(pnlHeader);
		}

		private void SetupTabs()
		{
			Panel pnlTabs = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(50, 0, 50, 0) };

			// Tab Buttons
			btnTabHistory = CreateTabButton("Lịch sử đặt vé", 50);
			btnTabHistory.Click += (s, e) => SwitchTab("HISTORY");
			pnlTabs.Controls.Add(btnTabHistory);

			btnTabProfile = CreateTabButton("Hồ sơ cá nhân", 250);
			btnTabProfile.Click += (s, e) => SwitchTab("PROFILE");
			pnlTabs.Controls.Add(btnTabProfile);

			// Active Line Indicator
			lineActiveTab = new Panel
			{
				Height = 4,
				BackColor = AppColors.Primary,
				Location = new Point(50, 46),
				Size = new Size(100, 4)
			};
			pnlTabs.Controls.Add(lineActiveTab);

			this.Controls.Add(pnlTabs);
		}

		private void SwitchTab(string tabName)
		{
			pnlContent.Controls.Clear();

			if (tabName == "HISTORY")
			{
				// UI State
				btnTabHistory.ForeColor = AppColors.Text;
				btnTabProfile.ForeColor = AppColors.TextMuted;
				lineActiveTab.Location = new Point(btnTabHistory.Location.X, 46);
				lineActiveTab.Width = btnTabHistory.Width;

				// Load Data
				LoadHistoryContent();
			}
			else
			{
				// UI State
				btnTabHistory.ForeColor = AppColors.TextMuted;
				btnTabProfile.ForeColor = AppColors.Text;
				lineActiveTab.Location = new Point(btnTabProfile.Location.X, 46);
				lineActiveTab.Width = btnTabProfile.Width;

				// Load Data
				LoadProfileContent();
			}
		}

		// =========================================================
		// 3. TAB LỊCH SỬ (HISTORY)
		// =========================================================
		private void LoadHistoryContent()
		{
			// 1. Header Bảng
			Panel pnlTableHeader = new Panel
			{
				Dock = DockStyle.Top,
				Height = 50,
				BackColor = AppColors.Background // Che nội dung khi scroll
			};

			string[] headers = { "MÃ VÉ", "THÔNG TIN TÀU", "NGÀY ĐI", "TRẠNG THÁI", "TỔNG TIỀN" };
			int curX = 20;
			for (int i = 0; i < headers.Length; i++)
			{
				Label lblH = new Label
				{
					Text = headers[i],
					ForeColor = AppColors.TextMuted,
					Font = new Font("Segoe UI", 10, FontStyle.Bold),
					AutoSize = false,
					Size = new Size(colWidths[i], 40),
					Location = new Point(curX, 10),
					TextAlign = ContentAlignment.MiddleLeft
				};
				pnlTableHeader.Controls.Add(lblH);
				curX += colWidths[i];
			}

			// 2. Danh sách (Scrollable)
			FlowLayoutPanel flowList = new FlowLayoutPanel
			{
				Dock = DockStyle.Fill,
				FlowDirection = FlowDirection.TopDown,
				WrapContents = false,
				AutoScroll = true,
				Padding = new Padding(0, 10, 0, 0),
				BackColor = Color.Transparent
			};

			// --- XỬ LÝ Z-ORDER (QUAN TRỌNG) ---
			pnlContent.Controls.Add(pnlTableHeader);
			pnlContent.Controls.Add(flowList);

			// Header phải được thêm vào Layout Engine trước để chiếm phần Top
			pnlTableHeader.SendToBack();
			flowList.BringToFront();

			// TODO: Load dữ liệu thực tế từ Database
			AddHistoryItem(flowList, "#VE12345", "Tàu SE1 - Toa 5 (Ghế 12A)", "15/08/2024", "Đã hoàn tất", ClrSuccess, "450,000đ");
			AddHistoryItem(flowList, "#VE67890", "Tàu TN2 - Toa 3 (Ghế 05B)", "22/09/2024", "Sắp tới", ClrWarning, "500,000đ");
			AddHistoryItem(flowList, "#VE13579", "Tàu SE7 - Toa 1 (Ghế 01C)", "01/07/2024", "Đã hủy", ClrError, "380,000đ");
			AddHistoryItem(flowList, "#VE99999", "Tàu HN1 - Toa VIP", "30/12/2024", "Sắp tới", ClrWarning, "1,200,000đ");
			AddHistoryItem(flowList, "#VE88888", "Tàu SE3 - Toa 2", "10/01/2025", "Sắp tới", ClrWarning, "600,000đ");
		}

		private void AddHistoryItem(FlowLayoutPanel parent, string code, string train, string date, string status, Color statusColor, string price)
		{
			int itemWidth = parent.ClientSize.Width - 20;
			if (itemWidth < 1200) itemWidth = 1200;

			Panel pnlItem = new Panel { Size = new Size(itemWidth, 70), Margin = new Padding(0, 0, 0, 15), BackColor = Color.Transparent };

			// Vẽ nền item bo góc
			pnlItem.Paint += (s, e) => {
				e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
				// Sử dụng hàm static từ RoundedButton để tránh code trùng lặp
				using (var path = RoundedButton.GetRoundedPath(new Rectangle(0, 0, pnlItem.Width - 1, pnlItem.Height - 1), 15))
				using (var brush = new SolidBrush(AppColors.CardBg)) // Dùng màu Card
				{
					e.Graphics.FillPath(brush, path);
				}
			};

			int curX = 20;
			pnlItem.Controls.Add(CreateLabel(code, 11, FontStyle.Bold, AppColors.Text, curX, 25)); curX += colWidths[0];
			pnlItem.Controls.Add(CreateLabel(train, 11, FontStyle.Regular, AppColors.Text, curX, 25)); curX += colWidths[1];
			pnlItem.Controls.Add(CreateLabel(date, 11, FontStyle.Regular, AppColors.TextMuted, curX, 25)); curX += colWidths[2];

			Label lblStatus = new Label { Text = status, ForeColor = statusColor, Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true, Location = new Point(curX, 25) };
			pnlItem.Controls.Add(lblStatus); curX += colWidths[3];

			pnlItem.Controls.Add(CreateLabel(price, 12, FontStyle.Bold, AppColors.Text, curX, 23));

			parent.Controls.Add(pnlItem);
		}

		// =========================================================
		// 4. TAB HỒ SƠ (PROFILE)
		// =========================================================
		private void LoadProfileContent()
		{
			// TODO: Hiển thị thông tin hồ sơ người dùng và cho phép chỉnh sửa
			// var user = GetCurrentUser();

			// 1. Tạo Card nền (Giữ nguyên)
			Panel pnlCard = new Panel
			{
				Size = new Size(1000, 550),
				Location = new Point(50, 20),
				BackColor = AppColors.CardBg,
			};

			pnlCard.Paint += (s, e) => {
				e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
				using (var path = RoundedButton.GetRoundedPath(new Rectangle(0, 0, pnlCard.Width - 1, pnlCard.Height - 1), 20))
				using (var brush = new SolidBrush(AppColors.CardBg))
				using (var pen = new Pen(Color.FromArgb(51, 65, 85), 1))
				{
					e.Graphics.FillPath(brush, path);
					e.Graphics.DrawPath(pen, path);
				}
			};
			pnlContent.Controls.Add(pnlCard);

			int startX = 150;
			int inputW = 320; 
			int gapX = 40;  
			int currentY = 50;

			// Title
			Label lblTitle = new Label
			{
				Text = "Thông tin chung",
				Font = new Font("Segoe UI", 16, FontStyle.Bold),
				ForeColor = AppColors.Text,
				AutoSize = true,
				Location = new Point(startX, 40)
			};
			pnlCard.Controls.Add(lblTitle);
			currentY += 60;


			// --- HÀNG 1: Họ tên & Ngày sinh ---
			pnlCard.Controls.Add(CreateLabel("Họ và Tên", 10, FontStyle.Regular, AppColors.TextMuted, startX, currentY));
			pnlCard.Controls.Add(CreateLabel("Ngày sinh", 10, FontStyle.Regular, AppColors.TextMuted, startX + inputW + gapX, currentY));
			currentY += 30;

			ModernTextBox txtName = new ModernTextBox
			{
				Location = new Point(startX, currentY),
				Size = new Size(inputW, 50),
				PlaceholderText = "Nhập họ tên",
				BackColor = AppColors.InputBg,
				ForeColor = AppColors.Text,
				IconText = "👤",
				Text = "Nguyễn Văn A" // TODO: Giả sử lấy từ user.FullName
			};
			pnlCard.Controls.Add(txtName);

			ModernTextBox txtDob = new ModernTextBox
			{
				Location = new Point(startX + inputW + gapX, currentY),
				Size = new Size(inputW, 50),
				PlaceholderText = "DD/MM/YYYY",
				BackColor = AppColors.InputBg,
				ForeColor = AppColors.Text,
				IconText = "📅",
				Text = "01/01/1990" // TODO: Giả sử lấy từ user.DateOfBirth
			};
			pnlCard.Controls.Add(txtDob);
			currentY += 80;

			// --- HÀNG 2: Email & SĐT ---
			pnlCard.Controls.Add(CreateLabel("Email", 10, FontStyle.Regular, AppColors.TextMuted, startX, currentY));
			pnlCard.Controls.Add(CreateLabel("Số điện thoại", 10, FontStyle.Regular, AppColors.TextMuted, startX + inputW + gapX, currentY));
			currentY += 30;

			ModernTextBox txtEmail = new ModernTextBox
			{
				Location = new Point(startX, currentY),
				Size = new Size(inputW, 50),
				PlaceholderText = "example@email.com",
				BackColor = AppColors.InputBg,
				ForeColor = AppColors.Text,
				IconText = "📧",
				Text = "nvana@email.com" // TODO: Giả sử lấy từ user.Email
			};
			pnlCard.Controls.Add(txtEmail);

			ModernTextBox txtPhone = new ModernTextBox
			{
				Location = new Point(startX + inputW + gapX, currentY),
				Size = new Size(inputW, 50),
				PlaceholderText = "0909xxxxxx",
				BackColor = AppColors.InputBg,
				ForeColor = AppColors.Text,
				IconText = "📞",
				Text = "0909123456" // TODO: Giả sử lấy từ user.PhoneNumber
			};
			pnlCard.Controls.Add(txtPhone);
			currentY += 80;

			// --- HÀNG 3: Địa chỉ (Full width) ---
			pnlCard.Controls.Add(CreateLabel("Địa chỉ thường trú", 10, FontStyle.Regular, AppColors.TextMuted, startX, currentY));
			currentY += 30;
			// Input địa chỉ dài bằng 2 ô trên cộng lại
			ModernTextBox txtAddress = new ModernTextBox
			{
				Location = new Point(startX, currentY),
				Size = new Size((inputW * 2) + gapX, 50),
				PlaceholderText = "Nhập địa chỉ của bạn...",
				BackColor = AppColors.InputBg,
				ForeColor = AppColors.Text, IconText = "📍",
				Text = "123 Đường ABC, Phường XYZ, Quận 1, TP.HCM" // TODO: Giả sử lấy từ user.Address
			};
			pnlCard.Controls.Add(txtAddress);
			currentY += 90;

			// --- BUTTON ACTIONS ---
			RoundedButton btnSave = new RoundedButton
			{
				Text = "LƯU THAY ĐỔI",
				BackColor = AppColors.Primary,
				ForeColor = Color.White,
				Size = new Size(200, 50),
				Location = new Point(startX, currentY),
				Font = new Font("Segoe UI", 11, FontStyle.Bold)
			};
			btnSave.MouseEnter += (s, e) => btnSave.BackColor = AppColors.PrimaryHover;
			btnSave.MouseLeave += (s, e) => btnSave.BackColor = AppColors.Primary;
			btnSave.Click += (s, e) => MessageBox.Show("Đã lưu thông tin hồ sơ!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
			pnlCard.Controls.Add(btnSave);

			// Nút Hủy
			Label btnCancel = new Label
			{
				Text = "Hủy bỏ",
				Font = new Font("Segoe UI", 11, FontStyle.Regular),
				ForeColor = AppColors.TextMuted,
				AutoSize = false,
				Size = new Size(100, 50),
				TextAlign = ContentAlignment.MiddleCenter,
				Location = new Point(startX + 220, currentY),
				Cursor = Cursors.Hand
			};
			btnCancel.MouseEnter += (s, e) => { btnCancel.ForeColor = Color.White; };
			btnCancel.MouseLeave += (s, e) => { btnCancel.ForeColor = AppColors.TextMuted; };
			pnlCard.Controls.Add(btnCancel);
		}

		// TODO: hàm đổ dữ liệu người dùng lên UI
		//private void BindDataToUI()
		//{
		//	if (_currentUserData != null)
		//	{
		//		txtName.Text = _currentUserData.FullName;
		//		txtDob.Text = _currentUserData.Dob;
		//		txtEmail.Text = _currentUserData.Email;
		//		txtPhone.Text = _currentUserData.Phone;
		//		txtAddress.Text = _currentUserData.Address;
		//	}
		//}

		// =========================================================
		// XỬ LÝ SỰ KIỆN NÚT BẤM
		// =========================================================

		// 1. SỰ KIỆN LƯU (SAVE)
		private void BtnSave_Click(object sender, EventArgs e)
		{
			// TODO: Lấy data từ các TextBox và lưu vào database
		}

		// 2. SỰ KIỆN HỦY (CANCEL)
		private void BtnCancel_Click(object sender, EventArgs e)
		{
			// TODO: Lấy data cũ từ database rồi hiển thị lại lên UI
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
					ForeColor = AppColors.TextMuted,
					AutoSize = true,
					Cursor = Cursors.Hand,
					Anchor = AnchorStyles.Top | AnchorStyles.Right
				};
				lblMenu.Location = new Point(menuX - 80, 20);

				lblMenu.MouseEnter += (s, e) => lblMenu.ForeColor = Color.White;
				lblMenu.MouseLeave += (s, e) => lblMenu.ForeColor = AppColors.TextMuted;

				// Xử lý sự kiện
				if (item == "Đăng xuất")
				{
					lblMenu.Click += (s, e) => Application.Exit();
				}
				else if (item == "Trang chủ")
				{
					// Đóng Profile sẽ quay về form gọi nó (MainForm) do dùng ShowDialog
					lblMenu.Click += (s, e) => this.Close();
				}

				pnlHeader.Controls.Add(lblMenu);
				menuX -= 120;
			}
		}

		private void AddWindowControls(Panel parent)
		{
			int btnSize = 45;
			int startX = parent.Width - (btnSize * 3) - 10;

			Label btnClose = CreateWindowButton("✕", startX + (btnSize * 2));
			btnClose.MouseEnter += (s, e) => btnClose.BackColor = AppColors.CloseHover;
			btnClose.MouseLeave += (s, e) => btnClose.BackColor = Color.Transparent;
			btnClose.Click += (s, e) => this.Close();

			Label btnMax = CreateWindowButton("☐", startX + btnSize);
			btnMax.MouseEnter += (s, e) => btnMax.BackColor = AppColors.HeaderHover;
			btnMax.MouseLeave += (s, e) => btnMax.BackColor = Color.Transparent;
			btnMax.Click += (s, e) => ToggleMaximize();

			Label btnMin = CreateWindowButton("―", startX);
			btnMin.MouseEnter += (s, e) => btnMin.BackColor = AppColors.HeaderHover;
			btnMin.MouseLeave += (s, e) => btnMin.BackColor = Color.Transparent;
			btnMin.Click += (s, e) => this.WindowState = FormWindowState.Minimized;

			parent.Controls.Add(btnClose);
			parent.Controls.Add(btnMax);
			parent.Controls.Add(btnMin);
		}

		private Label CreateWindowButton(string text, int x)
		{
			return new Label
			{
				Text = text,
				Font = new Font("Segoe UI", 12, FontStyle.Regular),
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

		private Label CreateTabButton(string text, int x)
		{
			return new Label
			{
				Text = text,
				Font = new Font("Segoe UI", 12, FontStyle.Bold),
				ForeColor = AppColors.TextMuted,
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