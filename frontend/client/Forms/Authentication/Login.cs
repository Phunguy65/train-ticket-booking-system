using client.Configuration;
using client.Components;
using client.Services;
using sdk_client.Exceptions;
using sdk_client.Services;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace client.Forms.Authentication
{
	// =========================================================
	// CLASS FORM LOGIN CHÍNH
	// =========================================================
	public partial class Login : Form
	{
		// --- 1. BẢNG MÀU (COLOR PALETTE) ---
		private readonly Color _clrBackground = Color.FromArgb(30, 41, 59); // Nền chính (Slate 800)
		private readonly Color _clrCard = Color.FromArgb(15, 23, 42); // Nền Card (Slate 900)
		private readonly Color _clrInputBg = Color.FromArgb(51, 65, 85); // Nền Input (Slate 700)
		private readonly Color _clrText = Color.White; // Màu chữ chính
		private readonly Color _clrTextMuted = Color.FromArgb(148, 163, 184); // Màu chữ phụ
		private readonly Color _clrPrimary = Color.FromArgb(37, 99, 235); // Màu xanh nút
		private readonly Color _clrPrimaryHover = Color.FromArgb(29, 78, 216); // Màu xanh hover

		// Màu cho thanh tiêu đề window
		private readonly Color _clrHeaderHover = Color.FromArgb(51, 65, 85);
		private readonly Color _clrCloseHover = Color.FromArgb(220, 38, 38);

		// Các biến Control
		private Panel _pnlCard;
		private Panel _pnlHeader;
		private ModernTextBox _txtUsername;
		private ModernTextBox _txtPassword;
		private RoundedButton _btnLogin;

		// Services
		private AuthenticationService? _authService;
		private bool _isLoggingIn;

		// --- 2. HÀM KHỞI TẠO ---
		public Login()
		{
			InitializeComponent();
			SetupUi();
			InitializeApiClient();
		}

		private void InitializeApiClient()
		{
			try
			{
				SessionManager.Instance.Initialize(
					ApiConfig.Host,
					ApiConfig.Port,
					ApiConfig.ConnectionTimeout,
					ApiConfig.RequestTimeout,
					ApiConfig.SignalRHost,
					ApiConfig.SignalRPort
				);

				// Use SessionManager's ApiClient directly instead of storing local reference
				var apiClient = SessionManager.Instance.ApiClient;
				if (apiClient != null)
				{
					_authService = new AuthenticationService(apiClient);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(
					$@"Không thể kết nối đến máy chủ.
Chi tiết: {ex.Message}",
					@"Lỗi kết nối",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error
				);
			}
		}

		// --- 3. SETUP GIAO DIỆN ---
		private void SetupUi()
		{
			// Cấu hình Form chính (1500x850)
			this.FormBorderStyle = FormBorderStyle.None;
			this.StartPosition = FormStartPosition.CenterScreen;
			this.Size = new Size(1500, 850);
			this.BackColor = _clrBackground;
			this.DoubleBuffered = true;

			// 1. Tạo thanh tiêu đề (Header) chứa nút Đóng/Phóng/Ẩn
			SetupWindowControls();

			// 2. PANEL CARD TRUNG TÂM (Login Form)
			int cardW = 500;
			int cardH = 700;

			_pnlCard = new Panel()
			{
				Size = new Size(cardW, cardH),
				BackColor = _clrCard,
				// Căn giữa màn hình
				Location = new Point((this.Width - cardW) / 2, (this.Height - cardH) / 2 + 15),
			};
			_pnlCard.Paint += (s, e) => DrawRoundedPanel(s, e, 25); // Bo góc 25px
			this.Controls.Add(_pnlCard);

			// --- CÁC THÀNH PHẦN BÊN TRONG CARD ---
			int yPos = 50;
			int xMargin = 50;
			int inputWidth = cardW - (xMargin * 2);

			// 3. Tiêu đề "HỆ THỐNG ĐẶT VÉ TÀU HỎA"
			Label lblTitle = new Label()
			{
				Text = "HỆ THỐNG ĐẶT VÉ TÀU HỎA",
				Font = new Font("Segoe UI", 22, FontStyle.Bold),
				ForeColor = _clrText,
				AutoSize = false,
				Size = new Size(inputWidth, 100), // Đủ cao để không mất chữ
				Location = new Point(xMargin, yPos),
				TextAlign = ContentAlignment.MiddleCenter
			};
			_pnlCard.Controls.Add(lblTitle);
			yPos += 110;

			// 4. Label Username
			_pnlCard.Controls.Add(CreateLabel("Tên đăng nhập / Email", xMargin, yPos));
			yPos += 35;

			// 5. Input Username (Sử dụng Class ModernTextBox bên dưới)
			_txtUsername = new ModernTextBox
			{
				Location = new Point(xMargin, yPos),
				Size = new Size(inputWidth, 55),
				PlaceholderText = "Nhập tài khoản của bạn",
				BackColor = _clrInputBg,
				ForeColor = _clrText,
				IconText = "👤",
				IsPasswordChar = false
			};
			_pnlCard.Controls.Add(_txtUsername);
			yPos += 85;

			// 6. Label Password
			_pnlCard.Controls.Add(CreateLabel("Mật khẩu", xMargin, yPos));
			yPos += 35;

			// 7. Input Password
			_txtPassword = new ModernTextBox
			{
				Location = new Point(xMargin, yPos),
				Size = new Size(inputWidth, 55),
				PlaceholderText = "Nhập mật khẩu",
				BackColor = _clrInputBg,
				ForeColor = _clrText,
				IconText = "🔒",
				IsPasswordChar = true
			};
			_pnlCard.Controls.Add(_txtPassword);
			yPos += 95;

			// 8. Nút Login (Sử dụng Class RoundedButton bên dưới)
			_btnLogin = new RoundedButton
			{
				Text = "ĐĂNG NHẬP",
				Size = new Size(inputWidth, 55),
				Location = new Point(xMargin, yPos),
				BackColor = _clrPrimary,
				ForeColor = Color.White,
				Font = new Font("Segoe UI", 12, FontStyle.Bold),
				Cursor = Cursors.Hand,
				FlatStyle = FlatStyle.Flat
			};
			_btnLogin.FlatAppearance.BorderSize = 0;
			_btnLogin.Click += BtnLogin_Click;
			_btnLogin.MouseEnter += (_, _) => _btnLogin.BackColor = _clrPrimaryHover;
			_btnLogin.MouseLeave += (_, _) => _btnLogin.BackColor = _clrPrimary;
			_pnlCard.Controls.Add(_btnLogin);
			yPos += 70;

			// 9. Footer (Đăng ký)
			Label lblRegister = new Label
			{
				Text = "Chưa có tài khoản? Đăng ký ngay",
				Font = new Font("Segoe UI", 10, FontStyle.Regular),
				ForeColor = _clrTextMuted,
				AutoSize = false,
				Size = new Size(inputWidth, 30),
				Location = new Point(xMargin, yPos),
				TextAlign = ContentAlignment.MiddleCenter,
				Cursor = Cursors.Hand
			};

			// Hiệu ứng hover đổi màu
			lblRegister.MouseEnter += (_, _) => lblRegister.ForeColor = _clrPrimary;
			lblRegister.MouseLeave += (_, _) => lblRegister.ForeColor = _clrTextMuted;

			lblRegister.Click += (_, _) =>
			{
				this.Hide();

				DialogResult result;
				using (Register registerForm = new Register())
				{
					result = registerForm.ShowDialog();
				}

				if (result != DialogResult.OK && !this.IsDisposed)
				{
					this.Show();
				}
			};

			_pnlCard.Controls.Add(lblRegister);

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
			_pnlCard.Controls.Add(lblCopy);
		}

		// --- HÀM TẠO THANH ĐIỀU KHIỂN (HEADER) ---
		private void SetupWindowControls()
		{
			_pnlHeader = new Panel() { Dock = DockStyle.Top, Height = 40, BackColor = Color.Transparent };
			_pnlHeader.MouseDown += Form_MouseDown;
			this.Controls.Add(_pnlHeader);

			int btnSize = 45;

			// Nút Đóng (X)
			Label btnClose = CreateWindowButton("✕", this.Width - btnSize, 0, btnSize);
			btnClose.Click += (_, _) => Application.Exit();
			btnClose.MouseEnter += (_, _) =>
			{
				btnClose.BackColor = _clrCloseHover;
				btnClose.ForeColor = Color.White;
			};
			btnClose.MouseLeave += (_, _) =>
			{
				btnClose.BackColor = Color.Transparent;
				btnClose.ForeColor = Color.White;
			};
			_pnlHeader.Controls.Add(btnClose);

			// Nút Phóng to (□)
			Label btnMax = CreateWindowButton("□", this.Width - (btnSize * 2), 0, btnSize);
			btnMax.Font = new Font("Segoe UI", 13);
			btnMax.Click += (_, _) =>
			{
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
			btnMax.MouseEnter += (_, _) => btnMax.BackColor = _clrHeaderHover;
			btnMax.MouseLeave += (_, _) => btnMax.BackColor = Color.Transparent;
			_pnlHeader.Controls.Add(btnMax);

			// Nút Thu nhỏ (-)
			Label btnMin = CreateWindowButton("―", this.Width - (btnSize * 3), 0, btnSize);
			btnMin.Click += (_, _) => this.WindowState = FormWindowState.Minimized;
			btnMin.MouseEnter += (_, _) => btnMin.BackColor = _clrHeaderHover;
			btnMin.MouseLeave += (_, _) => btnMin.BackColor = Color.Transparent;
			_pnlHeader.Controls.Add(btnMin);

			// Sự kiện Resize để neo nút vào góc phải
			this.Resize += (_, _) =>
			{
				btnClose.Location = new Point(this.Width - btnSize, 0);
				btnMax.Location = new Point(this.Width - (btnSize * 2), 0);
				btnMin.Location = new Point(this.Width - (btnSize * 3), 0);
				_pnlCard.Location = new Point((this.Width - _pnlCard.Width) / 2,
					(this.Height - _pnlCard.Height) / 2 + 15);
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
				ForeColor = _clrTextMuted,
				AutoSize = true,
				Location = new Point(x, y)
			};
		}

		private async void BtnLogin_Click(object? sender, EventArgs e)
		{
			if (_isLoggingIn)
			{
				return;
			}

			var username = _txtUsername.TextValue.Trim();
			var password = _txtPassword.TextValue;

			if (string.IsNullOrWhiteSpace(username))
			{
				MessageBox.Show(
					@"Vui lòng nhập tên đăng nhập.",
					@"Thông báo",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning
				);
				return;
			}

			if (string.IsNullOrWhiteSpace(password))
			{
				MessageBox.Show(
					@"Vui lòng nhập mật khẩu.",
					@"Thông báo",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning
				);
				return;
			}

			if (_authService == null)
			{
				MessageBox.Show(
					@"Không thể kết nối đến máy chủ. Vui lòng khởi động lại ứng dụng.",
					@"Lỗi",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error
				);
				return;
			}

			await PerformLoginAsync(username, password);
		}

		private async Task PerformLoginAsync(string username, string password)
		{
			_isLoggingIn = true;
			SetLoginButtonState(false, "ĐANG ĐĂNG NHẬP...");

			try
			{
				var loginResponse = await _authService!.LoginAsync(username, password).ConfigureAwait(false);

				if (loginResponse != null)
				{
					SessionManager.Instance.SetSession(loginResponse);

					this.Invoke((MethodInvoker)delegate
					{
						this.Hide();
						var mainForm = new TrainSearch.MainForm();
						mainForm.FormClosed += (_, _) =>
						{
							SessionManager.Instance.ClearSession();
							this.Close();
						};
						mainForm.Show();
					});
				}
			}
			catch (ApiException apiEx)
			{
				this.Invoke((MethodInvoker)delegate
				{
					_txtPassword.Clear();

					var errorMessage = TranslateErrorMessage(apiEx.Message);
					MessageBox.Show(
						errorMessage,
						@"Đăng nhập thất bại",
						MessageBoxButtons.OK,
						MessageBoxIcon.Error
					);
				});
			}
			catch (Exception ex)
			{
				this.Invoke((MethodInvoker)delegate
				{
					MessageBox.Show(
						$@"Lỗi kết nối đến máy chủ.
Vui lòng kiểm tra kết nối mạng và thử lại.

Chi tiết: {ex.Message}",
						@"Lỗi kết nối",
						MessageBoxButtons.OK,
						MessageBoxIcon.Error
					);
				});
			}
			finally
			{
				this.Invoke((MethodInvoker)delegate
				{
					SetLoginButtonState(true, "ĐĂNG NHẬP");
					_isLoggingIn = false;
				});
			}
		}

		private void SetLoginButtonState(bool enabled, string text)
		{
			_btnLogin.Enabled = enabled;
			_btnLogin.Text = text;
			_btnLogin.BackColor = enabled ? _clrPrimary : Color.FromArgb(71, 85, 105);
		}

		private string TranslateErrorMessage(string errorMessage)
		{
			if (errorMessage.Contains("Invalid username or password"))
			{
				return "Tên đăng nhập hoặc mật khẩu không đúng.";
			}

			if (errorMessage.Contains("Account is locked"))
			{
				return "Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên.";
			}

			if (errorMessage.Contains("timeout") || errorMessage.Contains("connection"))
			{
				return "Không thể kết nối đến máy chủ. Vui lòng kiểm tra kết nối mạng.";
			}

			return errorMessage;
		}

		private void DrawRoundedPanel(object? sender, PaintEventArgs e, int radius)
		{
			if (sender is not Panel pnl)
			{
				return;
			}

			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			using GraphicsPath path =
				RoundedButton.GetRoundedPath(new Rectangle(0, 0, pnl.Width, pnl.Height), radius);
			using SolidBrush brush = new SolidBrush(pnl.BackColor);
			e.Graphics.FillPath(brush, path);
		}

		// API Windows để kéo thả form không viền
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

		[System.Runtime.InteropServices.DllImport("user32.dll")]
		public static extern bool ReleaseCapture();

		private void Form_MouseDown(object? sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				ReleaseCapture();
				SendMessage(Handle, 0xA1, 0x2, 0);
			}
		}
	}
}