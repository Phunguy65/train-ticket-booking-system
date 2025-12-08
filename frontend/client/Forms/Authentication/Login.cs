using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using client.Controls;
using client.Helpers;

namespace client.Forms.Authentication
{
	public partial class Login : Form
	{
		// Các biến Control
		private Panel pnlCard;
		private ModernTextBox txtUsername;
		private ModernTextBox txtPassword;
		private RoundedButton btnLogin;

		public Login()
		{
			InitializeComponent();
			SetupModernUI();
		}

		private void SetupModernUI()
		{
			// Cấu hình Form chính
			this.FormBorderStyle = FormBorderStyle.None;
			this.StartPosition = FormStartPosition.CenterScreen;
			this.Size = new Size(1500, 850);
			this.BackColor = AppColors.Background; // Dùng màu từ Helper
			this.DoubleBuffered = true;

			// 1. Header
			SetupWindowControls();

			// 2. Card Login
			SetupLoginCard();
		}

		private void SetupLoginCard()
		{
			int cardW = 500;
			int cardH = 700;

			pnlCard = new Panel()
			{
				Size = new Size(cardW, cardH),
				BackColor = AppColors.CardBg,
				Location = new Point((this.Width - cardW) / 2, (this.Height - cardH) / 2 + 15),
			};
			// Dùng hàm static từ RoundedButton để vẽ bo góc cho Panel
			pnlCard.Paint += (s, e) => {
				e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
				using (var path = RoundedButton.GetRoundedPath(new Rectangle(0, 0, pnlCard.Width, pnlCard.Height), 25))
				using (var brush = new SolidBrush(pnlCard.BackColor))
					e.Graphics.FillPath(brush, path);
			};
			this.Controls.Add(pnlCard);

			int yPos = 50;
			int xMargin = 50;
			int inputWidth = cardW - (xMargin * 2);

			// Title
			Label lblTitle = new Label()
			{
				Text = "HỆ THỐNG ĐẶT VÉ\nTÀU HỎA",
				Font = new Font("Segoe UI", 22, FontStyle.Bold),
				ForeColor = AppColors.Text,
				AutoSize = false,
				Size = new Size(inputWidth, 100),
				Location = new Point(xMargin, yPos),
				TextAlign = ContentAlignment.MiddleCenter
			};
			pnlCard.Controls.Add(lblTitle);
			yPos += 110;

			// Username
			pnlCard.Controls.Add(CreateLabel("Tên đăng nhập / Email", xMargin, yPos));
			yPos += 35;

			txtUsername = new ModernTextBox
			{
				Location = new Point(xMargin, yPos),
				Size = new Size(inputWidth, 55),
				PlaceholderText = "Nhập tài khoản của bạn",
				BackColor = AppColors.InputBg,
				ForeColor = AppColors.Text,
				IconText = "👤",
				IsPasswordChar = false
			};
			pnlCard.Controls.Add(txtUsername);
			yPos += 85;

			// Password
			pnlCard.Controls.Add(CreateLabel("Mật khẩu", xMargin, yPos));
			yPos += 35;

			txtPassword = new ModernTextBox
			{
				Location = new Point(xMargin, yPos),
				Size = new Size(inputWidth, 55),
				PlaceholderText = "Nhập mật khẩu",
				BackColor = AppColors.InputBg,
				ForeColor = AppColors.Text,
				IconText = "🔒",
				IsPasswordChar = true
			};
			pnlCard.Controls.Add(txtPassword);
			yPos += 95;

			// Button Login
			btnLogin = new RoundedButton
			{
				Text = "ĐĂNG NHẬP",
				Size = new Size(inputWidth, 55),
				Location = new Point(xMargin, yPos),
				BackColor = AppColors.Primary,
				ForeColor = Color.White
			};
			btnLogin.Click += BtnLogin_Click;
			btnLogin.MouseEnter += (s, e) => btnLogin.BackColor = AppColors.PrimaryHover;
			btnLogin.MouseLeave += (s, e) => btnLogin.BackColor = AppColors.Primary;
			pnlCard.Controls.Add(btnLogin);
			yPos += 70;

			// Footer Register
			Label lblRegister = new Label
			{
				Text = "Chưa có tài khoản? Đăng ký ngay",
				Font = new Font("Segoe UI", 10, FontStyle.Regular),
				ForeColor = AppColors.TextMuted,
				AutoSize = false,
				Size = new Size(inputWidth, 30),
				Location = new Point(xMargin, yPos),
				TextAlign = ContentAlignment.MiddleCenter,
				Cursor = Cursors.Hand
			};
			lblRegister.MouseEnter += (s, e) => lblRegister.ForeColor = AppColors.Primary;
			lblRegister.MouseLeave += (s, e) => lblRegister.ForeColor = AppColors.TextMuted;
			lblRegister.Click += (s, e) => {
				// 1. Ẩn form Login hiện tại
				this.Hide();

				// 2. Khởi tạo form Register
				Register registerForm = new Register();

				// 3. Hiện form Register dưới dạng Dialog (Cửa sổ bắt buộc xử lý)
				// Code sẽ dừng ở dòng này cho đến khi Register tắt đi
				registerForm.ShowDialog();

				// 4. Khi Register đóng lại, dòng này sẽ chạy -> Hiện lại Login
				this.Show();
			};
			pnlCard.Controls.Add(lblRegister);

			// Copyright
			Label lblCopy = new Label
			{
				Text = "© 2024 VNR. All rights reserved.",
				Font = new Font("Segoe UI", 9, FontStyle.Regular),
				ForeColor = Color.Gray,
				AutoSize = false,
				Size = new Size(cardW, 30),
				Location = new Point(0, cardH - 40),
				TextAlign = ContentAlignment.MiddleCenter
			};
			pnlCard.Controls.Add(lblCopy);
		}

		private void SetupWindowControls()
		{
			Panel pnlHeader = new Panel() { Dock = DockStyle.Top, Height = 40, BackColor = Color.Transparent };
			pnlHeader.MouseDown += Form_MouseDown;
			this.Controls.Add(pnlHeader);

			int btnSize = 45;
			// Nút Close
			Label btnClose = CreateWindowButton("✕", this.Width - btnSize, 0, btnSize);
			btnClose.Click += (s, e) => Application.Exit();
			btnClose.MouseEnter += (s, e) => { btnClose.BackColor = AppColors.CloseHover; };
			btnClose.MouseLeave += (s, e) => { btnClose.BackColor = Color.Transparent; };
			pnlHeader.Controls.Add(btnClose);

			// ... (Bạn có thể thêm nút Max/Min tương tự, lược bớt cho gọn code demo)
		}

		private Label CreateWindowButton(string text, int x, int y, int size)
		{
			return new Label()
			{
				Text = text,
				Font = new Font("Segoe UI", 11, FontStyle.Regular),
				ForeColor = Color.White,
				AutoSize = false,
				Size = new Size(size, 40),
				Location = new Point(x, y),
				TextAlign = ContentAlignment.MiddleCenter,
				Cursor = Cursors.Hand
			};
		}

		private Label CreateLabel(string text, int x, int y)
		{
			return new Label
			{
				Text = text,
				Font = new Font("Segoe UI", 11, FontStyle.Regular),
				ForeColor = AppColors.TextMuted,
				AutoSize = true,
				Location = new Point(x, y)
			};
		}

		private void BtnLogin_Click(object sender, EventArgs e)
		{
			// TODO: Xử lý đăng nhập và chuyển sang màn hình chính nếu thành công
			MessageBox.Show($"Đang đăng nhập...\nUser: {txtUsername.TextValue}\nPass: {txtPassword.TextValue}", "Thông báo");
		}

		// --- Kéo thả Window không viền ---
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		public static extern bool ReleaseCapture();
		private void Form_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left) { ReleaseCapture(); SendMessage(Handle, 0xA1, 0x2, 0); }
		}
	}
}