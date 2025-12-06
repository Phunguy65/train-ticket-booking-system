using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

// Đảm bảo namespace trùng với project của bạn
namespace client.Forms.Authentication
{
	// =========================================================
	// CLASS FORM LOGIN CHÍNH
	// =========================================================
	public partial class Login : Form
	{
		// --- 1. BẢNG MÀU (COLOR PALETTE) ---
		private readonly Color ClrBackground = Color.FromArgb(30, 41, 59);      // Nền chính (Slate 800)
		private readonly Color ClrCard = Color.FromArgb(15, 23, 42);            // Nền Card (Slate 900)
		private readonly Color ClrInputBg = Color.FromArgb(51, 65, 85);         // Nền Input (Slate 700)
		private readonly Color ClrText = Color.White;                           // Màu chữ chính
		private readonly Color ClrTextMuted = Color.FromArgb(148, 163, 184);    // Màu chữ phụ
		private readonly Color ClrPrimary = Color.FromArgb(37, 99, 235);        // Màu xanh nút
		private readonly Color ClrPrimaryHover = Color.FromArgb(29, 78, 216);   // Màu xanh hover

		// Màu cho thanh tiêu đề window
		private readonly Color ClrHeaderHover = Color.FromArgb(51, 65, 85);
		private readonly Color ClrCloseHover = Color.FromArgb(220, 38, 38);

		// Các biến Control
		private Panel pnlCard;
		private Panel pnlHeader;
		private ModernTextBox txtUsername;
		private ModernTextBox txtPassword;
		private RoundedButton btnLogin;

		// --- 2. HÀM KHỞI TẠO ---
		public Login()
		{
			InitializeComponent(); // Giữ nguyên để Designer hoạt động nếu cần
			SetupModernUI();       // Hàm vẽ giao diện custom
		}

		// --- 3. SETUP GIAO DIỆN ---
		private void SetupModernUI()
		{
			// Cấu hình Form chính (1500x850)
			this.FormBorderStyle = FormBorderStyle.None;
			this.StartPosition = FormStartPosition.CenterScreen;
			this.Size = new Size(1500, 850);
			this.BackColor = ClrBackground;
			this.DoubleBuffered = true;

			// 1. Tạo thanh tiêu đề (Header) chứa nút Đóng/Phóng/Ẩn
			SetupWindowControls();

			// 2. PANEL CARD TRUNG TÂM (Login Form)
			int cardW = 500;
			int cardH = 700;

			pnlCard = new Panel()
			{
				Size = new Size(cardW, cardH),
				BackColor = ClrCard,
				// Căn giữa màn hình
				Location = new Point((this.Width - cardW) / 2, (this.Height - cardH) / 2 + 15),
			};
			pnlCard.Paint += (s, e) => DrawRoundedPanel(s, e, 25); // Bo góc 25px
			this.Controls.Add(pnlCard);

			// --- CÁC THÀNH PHẦN BÊN TRONG CARD ---
			int yPos = 50;
			int xMargin = 50;
			int inputWidth = cardW - (xMargin * 2);

			// 3. Tiêu đề "HỆ THỐNG ĐẶT VÉ TÀU HỎA"
			Label lblTitle = new Label()
			{
				Text = "HỆ THỐNG ĐẶT VÉ\nTÀU HỎA",
				Font = new Font("Segoe UI", 22, FontStyle.Bold),
				ForeColor = ClrText,
				AutoSize = false,
				Size = new Size(inputWidth, 100), // Đủ cao để không mất chữ
				Location = new Point(xMargin, yPos),
				TextAlign = ContentAlignment.MiddleCenter
			};
			pnlCard.Controls.Add(lblTitle);
			yPos += 110;

			// 4. Label Username
			pnlCard.Controls.Add(CreateLabel("Tên đăng nhập / Email", xMargin, yPos));
			yPos += 35;

			// 5. Input Username (Sử dụng Class ModernTextBox bên dưới)
			txtUsername = new ModernTextBox
			{
				Location = new Point(xMargin, yPos),
				Size = new Size(inputWidth, 55),
				PlaceholderText = "Nhập tài khoản của bạn",
				BackColor = ClrInputBg,
				ForeColor = ClrText,
				IconText = "👤",
				IsPasswordChar = false
			};
			pnlCard.Controls.Add(txtUsername);
			yPos += 85;

			// 6. Label Password
			pnlCard.Controls.Add(CreateLabel("Mật khẩu", xMargin, yPos));
			yPos += 35;

			// 7. Input Password
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
			yPos += 95;

			// 8. Nút Login (Sử dụng Class RoundedButton bên dưới)
			btnLogin = new RoundedButton
			{
				Text = "ĐĂNG NHẬP",
				Size = new Size(inputWidth, 55),
				Location = new Point(xMargin, yPos),
				BackColor = ClrPrimary,
				ForeColor = Color.White,
				Font = new Font("Segoe UI", 12, FontStyle.Bold),
				Cursor = Cursors.Hand,
				FlatStyle = FlatStyle.Flat
			};
			btnLogin.FlatAppearance.BorderSize = 0;
			btnLogin.Click += BtnLogin_Click;
			btnLogin.MouseEnter += (s, e) => btnLogin.BackColor = ClrPrimaryHover;
			btnLogin.MouseLeave += (s, e) => btnLogin.BackColor = ClrPrimary;
			pnlCard.Controls.Add(btnLogin);
			yPos += 70;

			// 9. Footer (Đăng ký)
			Label lblRegister = new Label
			{
				Text = "Chưa có tài khoản? Đăng ký ngay",
				Font = new Font("Segoe UI", 10, FontStyle.Regular),
				ForeColor = ClrTextMuted,
				AutoSize = false,
				Size = new Size(inputWidth, 30),
				Location = new Point(xMargin, yPos),
				TextAlign = ContentAlignment.MiddleCenter,
				Cursor = Cursors.Hand
			};

			// Hiệu ứng hover đổi màu
			lblRegister.MouseEnter += (s, e) => lblRegister.ForeColor = ClrPrimary;
			lblRegister.MouseLeave += (s, e) => lblRegister.ForeColor = ClrTextMuted;

			// --- BỔ SUNG SỰ KIỆN CLICK Ở ĐÂY ---
			lblRegister.Click += (s, e) =>
			{
				// 1. Ẩn form Login hiện tại đi
				this.Hide();

				// 2. Khởi tạo và hiện form Register
				// (Đảm bảo bạn đã có class Register trong project)
				Register registerForm = new Register();
				registerForm.ShowDialog(); // Hiện form Register và đợi xử lý

				// 3. Khi form Register đóng lại, hiện lại form Login
				this.Show();
			};

			pnlCard.Controls.Add(lblRegister);

			// 10. Copyright
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

		// --- HÀM TẠO THANH ĐIỀU KHIỂN (HEADER) ---
		private void SetupWindowControls()
		{
			pnlHeader = new Panel()
			{
				Dock = DockStyle.Top,
				Height = 40,
				BackColor = Color.Transparent
			};
			pnlHeader.MouseDown += Form_MouseDown;
			this.Controls.Add(pnlHeader);

			int btnSize = 45;

			// Nút Đóng (X)
			Label btnClose = CreateWindowButton("✕", this.Width - btnSize, 0, btnSize);
			btnClose.Click += (s, e) => Application.Exit();
			btnClose.MouseEnter += (s, e) => { btnClose.BackColor = ClrCloseHover; btnClose.ForeColor = Color.White; };
			btnClose.MouseLeave += (s, e) => { btnClose.BackColor = Color.Transparent; btnClose.ForeColor = Color.White; };
			pnlHeader.Controls.Add(btnClose);

			// Nút Phóng to (□)
			Label btnMax = CreateWindowButton("□", this.Width - (btnSize * 2), 0, btnSize);
			btnMax.Font = new Font("Segoe UI", 13);
			btnMax.Click += (s, e) => {
				if (this.WindowState == FormWindowState.Normal)
				{
					this.WindowState = FormWindowState.Maximized;
					btnMax.Text = "❐";
				}
				else
				{
					this.WindowState = FormWindowState.Normal;
					btnMax.Text = "□";
				}
			};
			btnMax.MouseEnter += (s, e) => btnMax.BackColor = ClrHeaderHover;
			btnMax.MouseLeave += (s, e) => btnMax.BackColor = Color.Transparent;
			pnlHeader.Controls.Add(btnMax);

			// Nút Thu nhỏ (-)
			Label btnMin = CreateWindowButton("―", this.Width - (btnSize * 3), 0, btnSize);
			btnMin.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
			btnMin.MouseEnter += (s, e) => btnMin.BackColor = ClrHeaderHover;
			btnMin.MouseLeave += (s, e) => btnMin.BackColor = Color.Transparent;
			pnlHeader.Controls.Add(btnMin);

			// Sự kiện Resize để neo nút vào góc phải
			this.Resize += (s, e) => {
				btnClose.Location = new Point(this.Width - btnSize, 0);
				btnMax.Location = new Point(this.Width - (btnSize * 2), 0);
				btnMin.Location = new Point(this.Width - (btnSize * 3), 0);
				if (pnlCard != null)
					pnlCard.Location = new Point((this.Width - pnlCard.Width) / 2, (this.Height - pnlCard.Height) / 2 + 15);
			};
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
				ForeColor = ClrTextMuted,
				AutoSize = true,
				Location = new Point(x, y)
			};
		}

		private void BtnLogin_Click(object sender, EventArgs e)
		{
			MessageBox.Show($"Đang đăng nhập...\nUser: {txtUsername.TextValue}\nPass: {txtPassword.TextValue}", "Thông báo");
		}

		private void DrawRoundedPanel(object sender, PaintEventArgs e, int radius)
		{
			Panel pnl = sender as Panel;
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			using (GraphicsPath path = RoundedButton.GetRoundedPath(new Rectangle(0, 0, pnl.Width, pnl.Height), radius))
			using (SolidBrush brush = new SolidBrush(pnl.BackColor))
			{
				e.Graphics.FillPath(brush, path);
			}
		}

		// API Windows để kéo thả form không viền
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		public static extern bool ReleaseCapture();
		private void Form_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left) { ReleaseCapture(); SendMessage(Handle, 0xA1, 0x2, 0); }
		}
	}

	// =========================================================
	// CÁC CUSTOM CONTROLS (NẰM CÙNG NAMESPACE ĐỂ KHÔNG BỊ LỖI)
	// =========================================================

	// 1. CLASS NÚT BO TRÒN (RoundedButton)
	public class RoundedButton : Button
	{
		protected override void OnPaint(PaintEventArgs pevent)
		{
			Graphics g = pevent.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
			using (GraphicsPath path = GetRoundedPath(new Rectangle(0, 0, Width, Height), 12))
			using (SolidBrush brush = new SolidBrush(this.BackColor))
			{
				this.Region = new Region(path);
				g.FillPath(brush, path);
				SizeF textSize = g.MeasureString(this.Text, this.Font);
				g.DrawString(this.Text, this.Font, new SolidBrush(this.ForeColor), new PointF((Width - textSize.Width) / 2, (Height - textSize.Height) / 2));
			}
		}
		public static GraphicsPath GetRoundedPath(Rectangle rect, int radius)
		{
			GraphicsPath path = new GraphicsPath(); float d = radius * 2F;
			path.AddArc(rect.X, rect.Y, d, d, 180, 90); path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
			path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90); path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
			path.CloseFigure(); return path;
		}
	}

	// 2. CLASS INPUT HIỆN ĐẠI (ModernTextBox)
	public class ModernTextBox : Panel
	{
		private TextBox txtInput; private Label lblIcon, lblTogglePass; private string _placeholder = ""; private bool _isPassword = false;
		public string TextValue => txtInput.Text == _placeholder ? "" : txtInput.Text;
		public string PlaceholderText { get => _placeholder; set { _placeholder = value; SetPlaceholder(); } }
		public string IconText { get => lblIcon.Text; set => lblIcon.Text = value; }
		public bool IsPasswordChar { get => _isPassword; set { _isPassword = value; SetupPasswordMode(); } }
		public override Color BackColor { get => base.BackColor; set { base.BackColor = value; if (txtInput != null) txtInput.BackColor = value; } }
		public override Color ForeColor { get => base.ForeColor; set { base.ForeColor = value; if (txtInput != null) txtInput.ForeColor = value; } }

		public ModernTextBox()
		{
			this.Padding = new Padding(10);
			lblIcon = new Label { Dock = DockStyle.Left, Width = 35, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI Emoji", 12), ForeColor = Color.Gray };
			txtInput = new TextBox { BorderStyle = BorderStyle.None, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 12), ForeColor = Color.Gray, BackColor = this.BackColor };
			txtInput.Enter += (s, e) => { if (txtInput.Text == _placeholder) { txtInput.Text = ""; txtInput.ForeColor = this.ForeColor; if (_isPassword) txtInput.UseSystemPasswordChar = true; } };
			txtInput.Leave += SetPlaceholder;
			lblTogglePass = new Label { Dock = DockStyle.Right, Width = 35, TextAlign = ContentAlignment.MiddleCenter, Text = "👁️", Cursor = Cursors.Hand, ForeColor = Color.Gray, Visible = false };
			lblTogglePass.Click += (s, e) => { if (txtInput.Text != _placeholder) { txtInput.UseSystemPasswordChar = !txtInput.UseSystemPasswordChar; lblTogglePass.ForeColor = txtInput.UseSystemPasswordChar ? Color.Gray : Color.White; } };
			this.Controls.Add(txtInput); this.Controls.Add(lblIcon); this.Controls.Add(lblTogglePass);
		}
		protected override void OnPaint(PaintEventArgs e) { base.OnPaint(e); e.Graphics.SmoothingMode = SmoothingMode.AntiAlias; using (GraphicsPath path = RoundedButton.GetRoundedPath(new Rectangle(0, 0, Width - 1, Height - 1), 12)) using (Pen pen = new Pen(Color.FromArgb(71, 85, 105), 1)) e.Graphics.DrawPath(pen, path); }
		private void SetPlaceholder(object s = null, EventArgs e = null) { if (string.IsNullOrWhiteSpace(txtInput.Text)) { txtInput.Text = _placeholder; txtInput.ForeColor = Color.Gray; if (_isPassword) txtInput.UseSystemPasswordChar = false; } }
		private void SetupPasswordMode() { if (_isPassword) { lblTogglePass.Visible = true; if (txtInput.Text != _placeholder) txtInput.UseSystemPasswordChar = true; } else { lblTogglePass.Visible = false; txtInput.UseSystemPasswordChar = false; } }
	}
}