using client.Configuration;
using client.Services;
using sdk_client;
using sdk_client.Exceptions;
using sdk_client.Services;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace client.Forms.Authentication
{
	// =========================================================
	// FORM ĐĂNG KÝ (REGISTER) - ĐÃ ĐỒNG BỘ VỚI LOGIN
	// =========================================================
	public partial class Register : Form
	{
		// --- 1. BẢNG MÀU (GIỐNG LOGIN) ---
		private readonly Color _clrBackground = Color.FromArgb(30, 41, 59);
		private readonly Color _clrCard = Color.FromArgb(15, 23, 42);
		private readonly Color _clrInputBg = Color.FromArgb(51, 65, 85);
		private readonly Color _clrText = Color.White;
		private readonly Color _clrTextMuted = Color.FromArgb(148, 163, 184);
		private readonly Color _clrPrimary = Color.FromArgb(37, 99, 235);
		private readonly Color _clrPrimaryHover = Color.FromArgb(29, 78, 216);

		// Màu nút header
		private readonly Color _clrHeaderHover = Color.FromArgb(51, 65, 85);
		private readonly Color _clrCloseHover = Color.FromArgb(220, 38, 38);

		// --- 2. CÁC CONTROL ---
		private Panel _pnlCard;
		private Panel _pnlHeader;
		private ModernTextBox _txtUsername;
		private ModernTextBox _txtFullName;
		private ModernTextBox _txtEmail;
		private ModernTextBox _txtPhoneNumber;
		private ModernTextBox _txtPassword;
		private ModernTextBox _txtConfirmPass;
		private RoundedButton _btnRegister;

		// Window control buttons (for event cleanup)
		private Label? _btnClose;
		private Label? _btnMax;
		private Label? _btnMin;

		// Services
		private AuthenticationService? _authService;
		private bool _isRegistering;

		public Register()
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
					ApiConfig.RequestTimeout
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

		// --- 3. HÀM DỰNG GIAO DIỆN ---
		private void SetupUi()
		{
			// Cấu hình Form (1500x850)
			this.FormBorderStyle = FormBorderStyle.None;
			this.StartPosition = FormStartPosition.CenterScreen;
			this.Size = new Size(1500, 850);
			this.BackColor = _clrBackground;
			this.DoubleBuffered = true;

			// 1. Tạo thanh tiêu đề điều khiển (Header)
			SetupWindowControls();

			// 2. PANEL CARD TRUNG TÂM
			int cardW = 550;
			int cardH = 800;

			_pnlCard = new Panel()
			{
				Size = new Size(cardW, cardH),
				BackColor = _clrCard,
				Location = new Point((this.Width - cardW) / 2, (this.Height - cardH) / 2 + 15),
			};
			_pnlCard.Paint += (s, e) => DrawRoundedPanel(s, e, 25);
			this.Controls.Add(_pnlCard);

			// --- NỘI DUNG BÊN TRONG CARD ---
			int yPos = 30;
			int xMargin = 50;
			int inputWidth = cardW - (xMargin * 2);

			// 3. Tiêu đề
			Label lblTitle = new Label()
			{
				Text = @"ĐĂNG KÝ TÀI KHOẢN",
				Font = new Font("Segoe UI", 20, FontStyle.Bold),
				ForeColor = _clrText,
				AutoSize = false,
				Size = new Size(inputWidth, 40),
				Location = new Point(xMargin, yPos),
				TextAlign = ContentAlignment.MiddleCenter
			};
			_pnlCard.Controls.Add(lblTitle);
			yPos += 45;

			// 4. Subtitle
			Label lblSub = new Label()
			{
				Text = @"Tham gia hệ thống đặt vé tàu ngay hôm nay.",
				Font = new Font("Segoe UI", 10, FontStyle.Regular),
				ForeColor = _clrTextMuted,
				AutoSize = false,
				Size = new Size(inputWidth, 25),
				Location = new Point(xMargin, yPos),
				TextAlign = ContentAlignment.MiddleCenter
			};
			_pnlCard.Controls.Add(lblSub);
			yPos += 35;

			// 5. Username Input
			_pnlCard.Controls.Add(CreateLabel("Tên đăng nhập", xMargin, yPos));
			yPos += 30;
			_txtUsername = new ModernTextBox
			{
				Location = new Point(xMargin, yPos),
				Size = new Size(inputWidth, 50),
				PlaceholderText = "Nhập tên đăng nhập",
				BackColor = _clrInputBg,
				ForeColor = _clrText,
				IconText = "👤",
				IsPasswordChar = false
			};
			_pnlCard.Controls.Add(_txtUsername);
			yPos += 70;

			// 6. Full Name Input
			_pnlCard.Controls.Add(CreateLabel("Họ và tên", xMargin, yPos));
			yPos += 30;
			_txtFullName = new ModernTextBox
			{
				Location = new Point(xMargin, yPos),
				Size = new Size(inputWidth, 50),
				PlaceholderText = "Nhập họ và tên đầy đủ",
				BackColor = _clrInputBg,
				ForeColor = _clrText,
				IconText = "📝",
				IsPasswordChar = false
			};
			_pnlCard.Controls.Add(_txtFullName);
			yPos += 70;

			// 7. Email Input
			_pnlCard.Controls.Add(CreateLabel("Email", xMargin, yPos));
			yPos += 30;
			_txtEmail = new ModernTextBox
			{
				Location = new Point(xMargin, yPos),
				Size = new Size(inputWidth, 50),
				PlaceholderText = "Nhập email của bạn",
				BackColor = _clrInputBg,
				ForeColor = _clrText,
				IconText = "📧",
				IsPasswordChar = false
			};
			_pnlCard.Controls.Add(_txtEmail);
			yPos += 70;

			// 8. Phone Number Input (Optional)
			_pnlCard.Controls.Add(CreateLabel("Số điện thoại (tùy chọn)", xMargin, yPos));
			yPos += 30;
			_txtPhoneNumber = new ModernTextBox
			{
				Location = new Point(xMargin, yPos),
				Size = new Size(inputWidth, 50),
				PlaceholderText = "Nhập số điện thoại",
				BackColor = _clrInputBg,
				ForeColor = _clrText,
				IconText = "📱",
				IsPasswordChar = false
			};
			_pnlCard.Controls.Add(_txtPhoneNumber);
			yPos += 70;

			// 9. Password Input
			_pnlCard.Controls.Add(CreateLabel("Mật khẩu", xMargin, yPos));
			yPos += 30;
			_txtPassword = new ModernTextBox
			{
				Location = new Point(xMargin, yPos),
				Size = new Size(inputWidth, 50),
				PlaceholderText = "Nhập mật khẩu",
				BackColor = _clrInputBg,
				ForeColor = _clrText,
				IconText = "🔒",
				IsPasswordChar = true
			};
			_pnlCard.Controls.Add(_txtPassword);
			yPos += 70;

			// 10. Confirm Password Input
			_pnlCard.Controls.Add(CreateLabel("Xác nhận mật khẩu", xMargin, yPos));
			yPos += 30;
			_txtConfirmPass = new ModernTextBox
			{
				Location = new Point(xMargin, yPos),
				Size = new Size(inputWidth, 50),
				PlaceholderText = "Nhập lại mật khẩu",
				BackColor = _clrInputBg,
				ForeColor = _clrText,
				IconText = "🛡️",
				IsPasswordChar = true
			};
			_pnlCard.Controls.Add(_txtConfirmPass);
			yPos += 75;

			// 8. Nút Đăng ký
			_btnRegister = new RoundedButton
			{
				Text = @"ĐĂNG KÝ",
				Size = new Size(inputWidth, 55),
				Location = new Point(xMargin, yPos),
				BackColor = _clrPrimary,
				ForeColor = Color.White,
				Font = new Font("Segoe UI", 12, FontStyle.Bold),
				Cursor = Cursors.Hand,
				FlatStyle = FlatStyle.Flat
			};
			_btnRegister.FlatAppearance.BorderSize = 0;
			_btnRegister.Click += BtnRegister_Click;
			_btnRegister.MouseEnter += (_, _) => _btnRegister.BackColor = _clrPrimaryHover;
			_btnRegister.MouseLeave += (_, _) => _btnRegister.BackColor = _clrPrimary;
			_pnlCard.Controls.Add(_btnRegister);
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
			lblLogin.Paint += (_, e) =>
			{
				string text1 = "Đã có tài khoản?";
				string text2 = "Đăng nhập ngay";
				Size size1 = TextRenderer.MeasureText(text1, lblLogin.Font);
				Size size2 = TextRenderer.MeasureText(text2, lblLogin.Font);
				int totalWidth = size1.Width + size2.Width;
				int startX = (lblLogin.Width - totalWidth) / 2;

				TextRenderer.DrawText(e.Graphics, text1, lblLogin.Font, new Point(startX, 5), _clrTextMuted);
				using (Font fontBold = new Font(lblLogin.Font, FontStyle.Bold | FontStyle.Underline))
				{
					TextRenderer.DrawText(e.Graphics, text2, fontBold, new Point(startX + size1.Width - 5, 5),
						_clrPrimary);
				}
			};

			lblLogin.Click += (_, _) =>
			{
				// Close Register form to return to Login
				this.Close();
			};
			_pnlCard.Controls.Add(lblLogin);

			// 10. Copyright
			Label lblCopy = new Label
			{
				Text = @"© 2024 VNR. All rights reserved.",
				Font = new Font("Segoe UI", 9, FontStyle.Regular),
				ForeColor = Color.Gray,
				AutoSize = false,
				Size = new Size(cardW, 30),
				Location = new Point(0, cardH - 35),
				TextAlign = ContentAlignment.MiddleCenter
			};
			_pnlCard.Controls.Add(lblCopy);
		}

		// --- HÀM TẠO THANH HEADER (COPY TỪ LOGIN) ---
		private void SetupWindowControls()
		{
			_pnlHeader = new Panel() { Dock = DockStyle.Top, Height = 40, BackColor = Color.Transparent };
			_pnlHeader.MouseDown += Form_MouseDown;
			this.Controls.Add(_pnlHeader);

			int btnSize = 45;

			// Nút Đóng (X)
			_btnClose = CreateWindowButton("✕", this.Width - btnSize, 0, btnSize);
			_btnClose.Click += (_, _) => Application.Exit();
			_btnClose.MouseEnter += (_, _) =>
			{
				_btnClose.BackColor = _clrCloseHover;
				_btnClose.ForeColor = Color.White;
			};
			_btnClose.MouseLeave += (_, _) =>
			{
				_btnClose.BackColor = Color.Transparent;
				_btnClose.ForeColor = Color.White;
			};
			_pnlHeader.Controls.Add(_btnClose);

			// Nút Phóng to
			_btnMax = CreateWindowButton("□", this.Width - (btnSize * 2), 0, btnSize);
			_btnMax.Font = new Font("Segoe UI", 13);
			_btnMax.Click += (_, _) =>
			{
				if (this.WindowState == FormWindowState.Normal)
				{
					this.WindowState = FormWindowState.Maximized;
					_btnMax.Text = "❐";
				}
				else
				{
					this.WindowState = FormWindowState.Normal;
					_btnMax.Text = "□";
				}
			};
			_btnMax.MouseEnter += (_, _) => _btnMax.BackColor = _clrHeaderHover;
			_btnMax.MouseLeave += (_, _) => _btnMax.BackColor = Color.Transparent;
			_pnlHeader.Controls.Add(_btnMax);

			// Nút Thu nhỏ
			_btnMin = CreateWindowButton("―", this.Width - (btnSize * 3), 0, btnSize);
			_btnMin.Click += (_, _) => this.WindowState = FormWindowState.Minimized;
			_btnMin.MouseEnter += (_, _) => _btnMin.BackColor = _clrHeaderHover;
			_btnMin.MouseLeave += (_, _) => _btnMin.BackColor = Color.Transparent;
			_pnlHeader.Controls.Add(_btnMin);

			// Resize Event - Use named method for proper cleanup
			this.Resize += OnFormResize;
		}

		// Named event handler for Resize event (allows proper detachment)
		private void OnFormResize(object? sender, EventArgs e)
		{
			// Safety check: prevent accessing disposed controls
			if (this.IsDisposed || this.Disposing)
			{
				return;
			}

			int btnSize = 45;

			if (_btnClose != null)
			{
				_btnClose.Location = new Point(this.Width - btnSize, 0);
			}

			if (_btnMax != null)
			{
				_btnMax.Location = new Point(this.Width - (btnSize * 2), 0);
			}

			if (_btnMin != null)
			{
				_btnMin.Location = new Point(this.Width - (btnSize * 3), 0);
			}

			_pnlCard.Location = new Point((this.Width - _pnlCard.Width) / 2,
				(this.Height - _pnlCard.Height) / 2 + 15);
		}

		// --- CÁC HÀM HỖ TRỢ ---
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

		private void DrawRoundedPanel(object sender, PaintEventArgs e, int radius)
		{
			if (sender is not Panel pnl) return;
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			// Sử dụng RoundedButton.GetRoundedPath từ file Login.cs
			using GraphicsPath path = RoundedButton.GetRoundedPath(new Rectangle(0, 0, pnl.Width, pnl.Height), radius);
			using SolidBrush brush = new SolidBrush(pnl.BackColor);
			e.Graphics.FillPath(brush, path);
		}

		private async void BtnRegister_Click(object sender, EventArgs e)
		{
			if (_isRegistering)
			{
				return;
			}

			var validationError = ValidateInputs();
			if (!string.IsNullOrEmpty(validationError))
			{
				MessageBox.Show(validationError, @"Lỗi xác thực", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

			await PerformRegistrationAsync();
		}

		private string? ValidateInputs()
		{
			var username = _txtUsername.TextValue.Trim();
			var fullName = _txtFullName.TextValue.Trim();
			var email = _txtEmail.TextValue.Trim();
			var phoneNumber = _txtPhoneNumber.TextValue.Trim();
			var password = _txtPassword.TextValue;
			var confirmPassword = _txtConfirmPass.TextValue;

			if (string.IsNullOrWhiteSpace(username))
			{
				return "Vui lòng nhập tên đăng nhập.";
			}

			if (username.Length < 3)
			{
				return "Tên đăng nhập phải có ít nhất 3 ký tự.";
			}

			if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$"))
			{
				return "Tên đăng nhập chỉ được chứa chữ cái, số và dấu gạch dưới.";
			}

			if (string.IsNullOrWhiteSpace(fullName))
			{
				return "Vui lòng nhập họ và tên.";
			}

			if (string.IsNullOrWhiteSpace(email))
			{
				return "Vui lòng nhập email.";
			}

			if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
			{
				return "Email không hợp lệ.";
			}

			if (!string.IsNullOrWhiteSpace(phoneNumber))
			{
				if (!Regex.IsMatch(phoneNumber, @"^[0-9]{10,11}$"))
				{
					return "Số điện thoại phải có 10-11 chữ số.";
				}
			}

			if (string.IsNullOrWhiteSpace(password))
			{
				return "Vui lòng nhập mật khẩu.";
			}

			if (password.Length < 6)
			{
				return "Mật khẩu phải có ít nhất 6 ký tự.";
			}

			if (password != confirmPassword)
			{
				return "Mật khẩu xác nhận không trùng khớp.";
			}

			return null;
		}

		private async Task PerformRegistrationAsync()
		{
			_isRegistering = true;
			SetRegisterButtonState(false, "ĐANG ĐĂNG KÝ...");

			try
			{
				var username = _txtUsername.TextValue.Trim();
				var fullName = _txtFullName.TextValue.Trim();
				var email = _txtEmail.TextValue.Trim();
				var phoneNumber = string.IsNullOrWhiteSpace(_txtPhoneNumber.TextValue)
					? null
					: _txtPhoneNumber.TextValue.Trim();
				var password = _txtPassword.TextValue.Trim();

				await _authService!.RegisterAsync(username, password, fullName, email, phoneNumber)
					.ConfigureAwait(false);

				// Safety check: only invoke if form is not disposed
				if (!this.IsDisposed && !this.Disposing)
				{
					this.Invoke((MethodInvoker)delegate
					{
						MessageBox.Show(
							@"Đăng ký thành công!
Bạn có thể đăng nhập ngay bây giờ.",
							@"Thành công",
							MessageBoxButtons.OK,
							MessageBoxIcon.Information
						);
					});
				}

				await AutoLoginAfterRegistrationAsync(username, password);
			}
			catch (ApiException apiEx)
			{
				// Safety check: only invoke if form is not disposed
				if (!this.IsDisposed && !this.Disposing)
				{
					this.Invoke((MethodInvoker)delegate
					{
						var errorMessage = TranslateErrorMessage(apiEx.Message);
						MessageBox.Show(
							errorMessage,
							@"Đăng ký thất bại",
							MessageBoxButtons.OK,
							MessageBoxIcon.Error
						);
					});
				}
			}
			catch (Exception ex)
			{
				// Safety check: only invoke if form is not disposed
				if (!this.IsDisposed && !this.Disposing)
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
			}
		}

		private async Task AutoLoginAfterRegistrationAsync(string username, string password)
		{
			try
			{
				var loginResponse = await _authService!.LoginAsync(username, password).ConfigureAwait(false);

				if (loginResponse != null)
				{
					SessionManager.Instance.SetSession(loginResponse);

					if (!this.IsDisposed && !this.Disposing)
					{
						this.Invoke((MethodInvoker)delegate
						{
							this.DialogResult = DialogResult.OK;
							var mainForm = new TrainSearch.MainForm();
							mainForm.FormClosed += (_, _) =>
							{
								SessionManager.Instance.ClearSession();
								Application.Exit();
							};
							mainForm.Show();
							this.Close();
						});
					}
				}
			}
			catch
			{
				if (!this.IsDisposed && !this.Disposing)
				{
					this.Invoke((MethodInvoker)delegate
					{
						this.DialogResult = DialogResult.Cancel;
						this.Close();
					});
				}
			}
		}

		private void SetRegisterButtonState(bool enabled, string text)
		{
			_btnRegister.Enabled = enabled;
			_btnRegister.Text = text;
			_btnRegister.BackColor = enabled ? _clrPrimary : Color.FromArgb(71, 85, 105);
		}

		private string TranslateErrorMessage(string errorMessage)
		{
			if (errorMessage.Contains("Username already exists"))
			{
				return "Tên đăng nhập đã tồn tại. Vui lòng chọn tên khác.";
			}

			if (errorMessage.Contains("Email already exists"))
			{
				return "Email đã được sử dụng. Vui lòng sử dụng email khác.";
			}

			if (errorMessage.Contains("Invalid email format"))
			{
				return "Định dạng email không hợp lệ.";
			}

			if (errorMessage.Contains("timeout") || errorMessage.Contains("connection"))
			{
				return "Không thể kết nối đến máy chủ. Vui lòng kiểm tra kết nối mạng.";
			}

			return errorMessage;
		}

		// Kéo thả form
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

		[System.Runtime.InteropServices.DllImport("user32.dll")]
		public static extern bool ReleaseCapture();

		private void Form_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				ReleaseCapture();
				SendMessage(Handle, 0xA1, 0x2, 0);
			}
		}
	}
}