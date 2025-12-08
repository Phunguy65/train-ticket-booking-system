using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using client.Controls; // Sử dụng lại Controls đã tách
using client.Helpers;  // Sử dụng lại Bảng màu đã tách

namespace client.Forms.Authentication
{
	public partial class Register : Form
	{
		// --- CÁC CONTROL ---
		private Panel pnlCard;
		private ModernTextBox txtEmail;
		private ModernTextBox txtPassword;
		private ModernTextBox txtConfirmPass;
		private RoundedButton btnRegister;

		public Register()
		{
			InitializeComponent();
			SetupModernUI();
		}

		// --- HÀM DỰNG GIAO DIỆN ---
		private void SetupModernUI()
		{
			// Cấu hình Form (1500x850)
			this.FormBorderStyle = FormBorderStyle.None;
			this.StartPosition = FormStartPosition.CenterScreen;
			this.Size = new Size(1500, 850);
			this.BackColor = AppColors.Background; // Dùng màu chung
			this.DoubleBuffered = true;

			// 1. Tạo thanh tiêu đề điều khiển (Header)
			SetupWindowControls();

			// 2. PANEL CARD TRUNG TÂM
			SetupRegisterCard();
		}

		private void SetupRegisterCard()
		{
			int cardW = 500;
			int cardH = 750; // Cao hơn Login để chứa đủ 3 ô nhập

			pnlCard = new Panel()
			{
				Size = new Size(cardW, cardH),
				BackColor = AppColors.CardBg,
				Location = new Point((this.Width - cardW) / 2, (this.Height - cardH) / 2 + 15),
			};

			// Bo góc Panel
			pnlCard.Paint += (s, e) => {
				e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
				using (var path = RoundedButton.GetRoundedPath(new Rectangle(0, 0, pnlCard.Width, pnlCard.Height), 25))
				using (var brush = new SolidBrush(pnlCard.BackColor))
					e.Graphics.FillPath(brush, path);
			};
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
				ForeColor = AppColors.Text,
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
				ForeColor = AppColors.TextMuted,
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
				BackColor = AppColors.InputBg,
				ForeColor = AppColors.Text,
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
				BackColor = AppColors.InputBg,
				ForeColor = AppColors.Text,
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
				BackColor = AppColors.InputBg,
				ForeColor = AppColors.Text,
				IconText = "🛡️",
				IsPasswordChar = true
			};
			pnlCard.Controls.Add(txtConfirmPass);
			yPos += 95;

			// 8. Nút Đăng ký
			btnRegister = new RoundedButton
			{
				Text = "ĐĂNG KÝ",
				Size = new Size(inputWidth, 55),
				Location = new Point(xMargin, yPos),
				BackColor = AppColors.Primary,
				ForeColor = Color.White
			};
			btnRegister.Click += BtnRegister_Click;
			btnRegister.MouseEnter += (s, e) => btnRegister.BackColor = AppColors.PrimaryHover;
			btnRegister.MouseLeave += (s, e) => btnRegister.BackColor = AppColors.Primary;
			pnlCard.Controls.Add(btnRegister);
			yPos += 70;

			// 9. Footer: Link quay lại Đăng nhập
			Label lblLogin = new Label
			{
				Text = "", // Vẽ thủ công bên dưới
				Font = new Font("Segoe UI", 10, FontStyle.Regular),
				AutoSize = false,
				Size = new Size(inputWidth, 30),
				Location = new Point(xMargin, yPos),
				Cursor = Cursors.Hand
			};

			lblLogin.Paint += (s, e) => {
				string text1 = "Đã có tài khoản?";
				string text2 = "Đăng nhập ngay";
				Size size1 = TextRenderer.MeasureText(text1, lblLogin.Font);

				int totalWidth = size1.Width + TextRenderer.MeasureText(text2, lblLogin.Font).Width;
				int startX = (lblLogin.Width - totalWidth) / 2;

				TextRenderer.DrawText(e.Graphics, text1, lblLogin.Font, new Point(startX, 5), AppColors.TextMuted);
				using (Font fontBold = new Font(lblLogin.Font, FontStyle.Bold | FontStyle.Underline))
				{
					TextRenderer.DrawText(e.Graphics, text2, fontBold, new Point(startX + size1.Width - 5, 5), AppColors.Primary);
				}
			};

			lblLogin.Click += (s, e) => {
				this.Close(); // Đóng form register
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
			// TODO: Xử lý đăng ký tài khoản và thêm chuyển sang màn hình chính nếu thành công
			MessageBox.Show($"Đăng ký thành công!\nEmail: {txtEmail.TextValue}", "Thông báo");
		}

		// --- CÁC HÀM HỖ TRỢ (GIỮ NGUYÊN HOẶC TÁCH BASE FORM SAU NÀY) ---
		private void SetupWindowControls()
		{
			Panel pnlHeader = new Panel() { Dock = DockStyle.Top, Height = 40, BackColor = Color.Transparent };
			pnlHeader.MouseDown += Form_MouseDown;
			this.Controls.Add(pnlHeader);

			int btnSize = 45;

			Label btnClose = CreateWindowButton("✕", this.Width - btnSize, 0, btnSize);
			btnClose.Click += (s, e) => Application.Exit();
			btnClose.MouseEnter += (s, e) => btnClose.BackColor = AppColors.CloseHover;
			btnClose.MouseLeave += (s, e) => btnClose.BackColor = Color.Transparent;
			pnlHeader.Controls.Add(btnClose);

			// ... (Bạn có thể thêm nút Max/Min nếu muốn, code tương tự Login)
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

		// Kéo thả form
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