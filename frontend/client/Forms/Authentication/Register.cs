using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace client.Forms.Authentication
{
	// =========================================================
	// FORM ĐĂNG KÝ (REGISTER) - ĐÃ ĐỒNG BỘ VỚI LOGIN
	// =========================================================
	public partial class Register : Form
	{
		// --- 1. BẢNG MÀU (GIỐNG LOGIN) ---
		private readonly Color ClrBackground = Color.FromArgb(30, 41, 59);
		private readonly Color ClrCard = Color.FromArgb(15, 23, 42);
		private readonly Color ClrInputBg = Color.FromArgb(51, 65, 85);
		private readonly Color ClrText = Color.White;
		private readonly Color ClrTextMuted = Color.FromArgb(148, 163, 184);
		private readonly Color ClrPrimary = Color.FromArgb(37, 99, 235);
		private readonly Color ClrPrimaryHover = Color.FromArgb(29, 78, 216);

		// Màu nút header
		private readonly Color ClrHeaderHover = Color.FromArgb(51, 65, 85);
		private readonly Color ClrCloseHover = Color.FromArgb(220, 38, 38);

		// --- 2. CÁC CONTROL ---
		private Panel pnlCard;
		private Panel pnlHeader;
		private ModernTextBox txtEmail;
		private ModernTextBox txtPassword;
		private ModernTextBox txtConfirmPass;
		private RoundedButton btnRegister;

		public Register()
		{
			InitializeComponent();
			SetupModernUI();
		}

		// --- 3. HÀM DỰNG GIAO DIỆN ---
		private void SetupModernUI()
		{
			// Cấu hình Form (1500x850)
			this.FormBorderStyle = FormBorderStyle.None;
			this.StartPosition = FormStartPosition.CenterScreen;
			this.Size = new Size(1500, 850);
			this.BackColor = ClrBackground;
			this.DoubleBuffered = true;

			// 1. Tạo thanh tiêu đề điều khiển (Header)
			SetupWindowControls();

			// 2. PANEL CARD TRUNG TÂM
			int cardW = 500;
			int cardH = 750; // Cao hơn Login chút để chứa đủ 3 ô nhập

			pnlCard = new Panel()
			{
				Size = new Size(cardW, cardH),
				BackColor = ClrCard,
				// Căn giữa màn hình
				Location = new Point((this.Width - cardW) / 2, (this.Height - cardH) / 2 + 15),
			};
			pnlCard.Paint += (s, e) => DrawRoundedPanel(s, e, 25); // Bo góc 25px
			this.Controls.Add(pnlCard);

			// --- NỘI DUNG BÊN TRONG CARD ---
			int yPos = 40;
			int xMargin = 50;
			int inputWidth = cardW - (xMargin * 2);

			// 3. Tiêu đề lớn
			Label lblTitle = new Label()
			{
				Text = "ĐĂNG KÝ TÀI KHOẢN",
				Font = new Font("Segoe UI", 22, FontStyle.Bold),
				ForeColor = ClrText,
				AutoSize = false,
				Size = new Size(inputWidth, 50),
				Location = new Point(xMargin, yPos),
				TextAlign = ContentAlignment.MiddleCenter
			};
			pnlCard.Controls.Add(lblTitle);
			yPos += 50;

			// 4. Tiêu đề nhỏ (Subtitle)
			Label lblSub = new Label()
			{
				Text = "Tham gia hệ thống đặt vé tàu ngay hôm nay.",
				Font = new Font("Segoe UI", 11, FontStyle.Regular),
				ForeColor = ClrTextMuted,
				AutoSize = false,
				Size = new Size(inputWidth, 30),
				Location = new Point(xMargin, yPos),
				TextAlign = ContentAlignment.MiddleCenter
			};
			pnlCard.Controls.Add(lblSub);
			yPos += 40;

			// 5. Email Input
			pnlCard.Controls.Add(CreateLabel("Email", xMargin, yPos));
			yPos += 35;
			txtEmail = new ModernTextBox
			{
				Location = new Point(xMargin, yPos),
				Size = new Size(inputWidth, 55),
				PlaceholderText = "Nhập email của bạn",
				BackColor = ClrInputBg,
				ForeColor = ClrText,
				IconText = "📧",
				IsPasswordChar = false
			};
			pnlCard.Controls.Add(txtEmail);
			yPos += 85;

			// 6. Password Input
			pnlCard.Controls.Add(CreateLabel("Mật khẩu", xMargin, yPos));
			yPos += 35;
			txtPassword = new ModernTextBox
			{
				Location = new Point(xMargin, yPos),
				Size = new Size(inputWidth, 55),
				PlaceholderText = "Nhập mật khẩu",
				BackColor = ClrInputBg,
				ForeColor = ClrText,
				IconText = "🔒",
				IsPasswordChar = true
			};
			pnlCard.Controls.Add(txtPassword);
			yPos += 85;

			// 7. Confirm Password Input
			pnlCard.Controls.Add(CreateLabel("Xác nhận mật khẩu", xMargin, yPos));
			yPos += 35;
			txtConfirmPass = new ModernTextBox
			{
				Location = new Point(xMargin, yPos),
				Size = new Size(inputWidth, 55),
				PlaceholderText = "Nhập lại mật khẩu",
				BackColor = ClrInputBg,
				ForeColor = ClrText,
				IconText = "🛡️",
				IsPasswordChar = true
			};
			pnlCard.Controls.Add(txtConfirmPass);
			yPos += 95; // Cách xa nút Register

			// 8. Nút Đăng ký
			btnRegister = new RoundedButton
			{
				Text = "ĐĂNG KÝ",
				Size = new Size(inputWidth, 55),
				Location = new Point(xMargin, yPos),
				BackColor = ClrPrimary,
				ForeColor = Color.White,
				Font = new Font("Segoe UI", 12, FontStyle.Bold),
				Cursor = Cursors.Hand,
				FlatStyle = FlatStyle.Flat
			};
			btnRegister.FlatAppearance.BorderSize = 0;
			btnRegister.Click += BtnRegister_Click;
			btnRegister.MouseEnter += (s, e) => btnRegister.BackColor = ClrPrimaryHover;
			btnRegister.MouseLeave += (s, e) => btnRegister.BackColor = ClrPrimary;
			pnlCard.Controls.Add(btnRegister);
			yPos += 70;

			// 9. Footer: Link quay lại Đăng nhập
			Label lblLogin = new Label
			{
				Text = "", // Sẽ vẽ bằng tay bên dưới
				Font = new Font("Segoe UI", 10, FontStyle.Regular),
				AutoSize = false,
				Size = new Size(inputWidth, 30),
				Location = new Point(xMargin, yPos),
				Cursor = Cursors.Hand
			};

			// Vẽ chữ 2 màu
			lblLogin.Paint += (s, e) => {
				string text1 = "Đã có tài khoản?";
				string text2 = "Đăng nhập ngay";
				Size size1 = TextRenderer.MeasureText(text1, lblLogin.Font);
				Size size2 = TextRenderer.MeasureText(text2, lblLogin.Font);
				int totalWidth = size1.Width + size2.Width;
				int startX = (lblLogin.Width - totalWidth) / 2;

				TextRenderer.DrawText(e.Graphics, text1, lblLogin.Font, new Point(startX, 5), ClrTextMuted);
				using (Font fontBold = new Font(lblLogin.Font, FontStyle.Bold | FontStyle.Underline))
				{
					TextRenderer.DrawText(e.Graphics, text2, fontBold, new Point(startX + size1.Width - 5, 5), ClrPrimary);
				}
			};

			lblLogin.Click += (s, e) => {
				this.Hide();
				var loginForm = new Login(); // Chuyển về màn hình Login
				loginForm.ShowDialog();
				this.Close();
			};
			pnlCard.Controls.Add(lblLogin);

			// 10. Copyright
			Label lblCopy = new Label
			{
				Text = "© 2024 VNR. All rights reserved.",
				Font = new Font("Segoe UI", 9, FontStyle.Regular),
				ForeColor = Color.Gray,
				AutoSize = false,
				Size = new Size(cardW, 30),
				Location = new Point(0, cardH - 35),
				TextAlign = ContentAlignment.MiddleCenter
			};
			pnlCard.Controls.Add(lblCopy);
		}

		// --- HÀM TẠO THANH HEADER (COPY TỪ LOGIN) ---
		private void SetupWindowControls()
		{
			pnlHeader = new Panel() { Dock = DockStyle.Top, Height = 40, BackColor = Color.Transparent };
			pnlHeader.MouseDown += Form_MouseDown;
			this.Controls.Add(pnlHeader);

			int btnSize = 45;

			// Nút Đóng (X)
			Label btnClose = CreateWindowButton("✕", this.Width - btnSize, 0, btnSize);
			btnClose.Click += (s, e) => Application.Exit();
			btnClose.MouseEnter += (s, e) => { btnClose.BackColor = ClrCloseHover; btnClose.ForeColor = Color.White; };
			btnClose.MouseLeave += (s, e) => { btnClose.BackColor = Color.Transparent; btnClose.ForeColor = Color.White; };
			pnlHeader.Controls.Add(btnClose);

			// Nút Phóng to
			Label btnMax = CreateWindowButton("□", this.Width - (btnSize * 2), 0, btnSize);
			btnMax.Font = new Font("Segoe UI", 13);
			btnMax.Click += (s, e) => {
				if (this.WindowState == FormWindowState.Normal) { this.WindowState = FormWindowState.Maximized; btnMax.Text = "❐"; }
				else { this.WindowState = FormWindowState.Normal; btnMax.Text = "□"; }
			};
			btnMax.MouseEnter += (s, e) => btnMax.BackColor = ClrHeaderHover;
			btnMax.MouseLeave += (s, e) => btnMax.BackColor = Color.Transparent;
			pnlHeader.Controls.Add(btnMax);

			// Nút Thu nhỏ
			Label btnMin = CreateWindowButton("―", this.Width - (btnSize * 3), 0, btnSize);
			btnMin.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
			btnMin.MouseEnter += (s, e) => btnMin.BackColor = ClrHeaderHover;
			btnMin.MouseLeave += (s, e) => btnMin.BackColor = Color.Transparent;
			pnlHeader.Controls.Add(btnMin);

			// Resize Event
			this.Resize += (s, e) => {
				btnClose.Location = new Point(this.Width - btnSize, 0);
				btnMax.Location = new Point(this.Width - (btnSize * 2), 0);
				btnMin.Location = new Point(this.Width - (btnSize * 3), 0);
				if (pnlCard != null) pnlCard.Location = new Point((this.Width - pnlCard.Width) / 2, (this.Height - pnlCard.Height) / 2 + 15);
			};
		}

		// --- CÁC HÀM HỖ TRỢ ---
		private Label CreateWindowButton(string text, int x, int y, int size)
		{
			return new Label() { Text = text, Font = new Font("Segoe UI", 11, FontStyle.Regular), ForeColor = Color.White, AutoSize = false, Size = new Size(size, 40), Location = new Point(x, y), TextAlign = ContentAlignment.MiddleCenter, Cursor = Cursors.Hand };
		}

		private Label CreateLabel(string text, int x, int y)
		{
			return new Label { Text = text, Font = new Font("Segoe UI", 11, FontStyle.Regular), ForeColor = ClrTextMuted, AutoSize = true, Location = new Point(x, y) };
		}

		private void DrawRoundedPanel(object sender, PaintEventArgs e, int radius)
		{
			Panel pnl = sender as Panel;
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			// Sử dụng RoundedButton.GetRoundedPath từ file Login.cs
			using (GraphicsPath path = RoundedButton.GetRoundedPath(new Rectangle(0, 0, pnl.Width, pnl.Height), radius))
			using (SolidBrush brush = new SolidBrush(pnl.BackColor)) { e.Graphics.FillPath(brush, path); }
		}

		private void BtnRegister_Click(object sender, EventArgs e)
		{
			if (txtPassword.TextValue != txtConfirmPass.TextValue)
			{
				MessageBox.Show("Mật khẩu xác nhận không trùng khớp!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			if (string.IsNullOrEmpty(txtEmail.TextValue) || string.IsNullOrEmpty(txtPassword.TextValue))
			{
				MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			MessageBox.Show($"Đăng ký thành công!\nEmail: {txtEmail.TextValue}", "Thông báo");
		}

		// Kéo thả form
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		public static extern bool ReleaseCapture();
		private void Form_MouseDown(object sender, MouseEventArgs e) { if (e.Button == MouseButtons.Left) { ReleaseCapture(); SendMessage(Handle, 0xA1, 0x2, 0); } }
	}
}