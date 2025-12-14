using client.Components;
using client.Services;
using sdk_client.Protocol;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace client.Forms.Booking
{
	/// <summary>
	/// Detail view form for displaying complete booking information.
	/// Allows conditional ticket cancellation for eligible bookings.
	/// </summary>
	public partial class BookingDetail : Form
	{
		// Theme colors (consistent with Profile.cs)
		private readonly Color _clrBackground = Color.FromArgb(15, 23, 42);
		private readonly Color _clrHeader = Color.FromArgb(15, 23, 42);
		private readonly Color _clrCardBg = Color.FromArgb(30, 41, 59);
		private readonly Color _clrText = Color.White;
		private readonly Color _clrTextGray = Color.FromArgb(148, 163, 184);
		private readonly Color _clrAccent = Color.FromArgb(37, 99, 235);
		private readonly Color _clrSuccess = Color.FromArgb(34, 197, 94);
		private readonly Color _clrWarning = Color.FromArgb(249, 115, 22);
		private readonly Color _clrError = Color.FromArgb(239, 68, 68);

		private readonly BookingHistory _booking;
		private RoundedButton _btnCancel;
		private RoundedButton _btnClose;
		private bool _isCancelling;

		/// <summary>
		/// Event raised when booking is successfully cancelled.
		/// </summary>
		public event EventHandler? BookingCancelled;

		public BookingDetail(BookingHistory booking)
		{
			_booking = booking ?? throw new ArgumentNullException(nameof(booking));
			InitializeComponent();
			SetupUi();
		}

		private void SetupUi()
		{
			this.FormBorderStyle = FormBorderStyle.None;
			this.StartPosition = FormStartPosition.CenterParent;
			this.Size = new Size(700, 700);
			this.BackColor = _clrBackground;
			this.DoubleBuffered = true;

			Panel pnlHeader = new Panel
			{
				Dock = DockStyle.Top, Height = 60, BackColor = _clrHeader, Padding = new Padding(20, 0, 20, 0)
			};

			Label lblTitle = new Label
			{
				Text = "🚆 Chi Tiết Vé",
				Font = new Font("Segoe UI", 14, FontStyle.Bold),
				ForeColor = _clrAccent,
				AutoSize = true,
				Location = new Point(20, 18)
			};
			pnlHeader.Controls.Add(lblTitle);

			Label btnCloseHeader = new Label
			{
				Text = "✕",
				Font = new Font("Segoe UI", 14, FontStyle.Bold),
				ForeColor = Color.White,
				AutoSize = false,
				Size = new Size(40, 30),
				Location = new Point(this.Width - 60, 15),
				TextAlign = ContentAlignment.MiddleCenter,
				Cursor = Cursors.Hand,
				Anchor = AnchorStyles.Top | AnchorStyles.Right
			};
			btnCloseHeader.MouseEnter += (_, _) => btnCloseHeader.BackColor = _clrError;
			btnCloseHeader.MouseLeave += (_, _) => btnCloseHeader.BackColor = Color.Transparent;
			btnCloseHeader.Click += (_, _) => this.Close();
			pnlHeader.Controls.Add(btnCloseHeader);

			this.Controls.Add(pnlHeader);

			Panel pnlContent = new Panel
			{
				Location = new Point(30, 80),
				Size = new Size(640, 540),
				BackColor = Color.Transparent,
				AutoScroll = true
			};

			int yPos = 0;

			string bookingCode = $"#VE{_booking.BookingId:00000}";
			Label lblBookingCode = new Label
			{
				Text = bookingCode,
				Font = new Font("Segoe UI", 18, FontStyle.Bold),
				ForeColor = _clrText,
				AutoSize = true,
				Location = new Point(0, yPos)
			};
			pnlContent.Controls.Add(lblBookingCode);
			yPos += 40;

			Color statusColor = GetStatusColor(_booking.BookingStatus);
			string statusText = GetStatusText(_booking.BookingStatus);
			Label lblStatus = new Label
			{
				Text = statusText,
				Font = new Font("Segoe UI", 10, FontStyle.Bold),
				ForeColor = statusColor,
				AutoSize = true,
				Location = new Point(0, yPos)
			};
			pnlContent.Controls.Add(lblStatus);
			yPos += 40;

			Panel pnlTrainInfo = CreateTrainInfoPanel();
			pnlTrainInfo.Location = new Point(0, yPos);
			pnlContent.Controls.Add(pnlTrainInfo);
			yPos += pnlTrainInfo.Height + 20;

			yPos = AddInfoRow(pnlContent, "Ghế:", string.Join(", ", _booking.SeatNumbers), yPos);
			yPos = AddInfoRow(pnlContent, "Tổng tiền:", $"{_booking.TotalAmount:N0}đ", yPos);
			yPos = AddInfoRow(pnlContent, "Ngày đặt:", _booking.BookingDate.ToString("dd/MM/yyyy HH:mm"), yPos);
			yPos = AddInfoRow(pnlContent, "Thanh toán:", GetPaymentStatusText(_booking.PaymentStatus), yPos);

			if (_booking.CancelledAt.HasValue)
			{
				AddInfoRow(pnlContent, "Ngày hủy:", _booking.CancelledAt.Value.ToString("dd/MM/yyyy HH:mm"), yPos);
			}

			this.Controls.Add(pnlContent);

			Panel pnlActions = new Panel
			{
				Location = new Point(30, 630), Size = new Size(640, 50), BackColor = Color.Transparent
			};

			_btnClose = new RoundedButton
			{
				Text = "Đóng",
				Size = new Size(150, 45),
				Location = new Point(490, 0),
				BackColor = Color.FromArgb(51, 65, 85),
				ForeColor = _clrText,
				Font = new Font("Segoe UI", 11, FontStyle.Bold),
				Cursor = Cursors.Hand,
				FlatStyle = FlatStyle.Flat
			};
			_btnClose.FlatAppearance.BorderSize = 0;
			_btnClose.Click += (_, _) => this.Close();
			pnlActions.Controls.Add(_btnClose);

			if (CanCancelBooking())
			{
				_btnCancel = new RoundedButton
				{
					Text = "🗑️ Hủy vé",
					Size = new Size(150, 45),
					Location = new Point(0, 0),
					BackColor = _clrError,
					ForeColor = Color.White,
					Font = new Font("Segoe UI", 11, FontStyle.Bold),
					Cursor = Cursors.Hand,
					FlatStyle = FlatStyle.Flat
				};
				_btnCancel.FlatAppearance.BorderSize = 0;
				_btnCancel.Click += BtnCancel_Click;
				pnlActions.Controls.Add(_btnCancel);
			}
			else
			{
				Label lblCannotCancel = new Label
				{
					Text = GetCancellationDisabledReason(),
					Font = new Font("Segoe UI", 9, FontStyle.Italic),
					ForeColor = _clrTextGray,
					AutoSize = true,
					Location = new Point(0, 15)
				};
				pnlActions.Controls.Add(lblCannotCancel);
			}

			this.Controls.Add(pnlActions);
		}

		private Panel CreateTrainInfoPanel()
		{
			Panel panel = new Panel { Size = new Size(640, 220), BackColor = Color.Transparent };

			panel.Paint += (_, e) =>
			{
				e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
				using GraphicsPath path = GetRoundedPath(new Rectangle(0, 0, panel.Width, panel.Height), 12);
				using SolidBrush brush = new SolidBrush(_clrCardBg);
				e.Graphics.FillPath(brush, path);
			};

			Label lblHeader = new Label
			{
				Text = "THÔNG TIN CHUYẾN TÀU",
				Font = new Font("Segoe UI", 11, FontStyle.Bold),
				ForeColor = _clrAccent,
				AutoSize = true,
				Location = new Point(20, 15)
			};
			panel.Controls.Add(lblHeader);

			int yPos = 50;
			yPos = AddCardRow(panel, "Tàu:", $"{_booking.TrainNumber} - {_booking.TrainName}", yPos);
			yPos = AddCardRow(panel, "Tuyến:", $"{_booking.DepartureStation} → {_booking.ArrivalStation}", yPos);
			yPos = AddCardRow(panel, "Khởi hành:", _booking.DepartureTime.ToString("dd/MM/yyyy HH:mm"), yPos);
			yPos = AddCardRow(panel, "Đến nơi:", _booking.ArrivalTime.ToString("dd/MM/yyyy HH:mm"), yPos);

			TimeSpan duration = _booking.ArrivalTime - _booking.DepartureTime;
			string durationText = $"{(int)duration.TotalHours}h {duration.Minutes}m";
			AddCardRow(panel, "Thời gian:", durationText, yPos);

			return panel;
		}

		private int AddCardRow(Panel parent, string label, string value, int yPos)
		{
			Label lblLabel = new Label
			{
				Text = label,
				Font = new Font("Segoe UI", 10, FontStyle.Regular),
				ForeColor = _clrTextGray,
				AutoSize = true,
				Location = new Point(20, yPos)
			};
			parent.Controls.Add(lblLabel);

			Label lblValue = new Label
			{
				Text = value,
				Font = new Font("Segoe UI", 10, FontStyle.Bold),
				ForeColor = _clrText,
				AutoSize = true,
				Location = new Point(150, yPos)
			};
			parent.Controls.Add(lblValue);

			return yPos + 30;
		}

		private int AddInfoRow(Panel parent, string label, string value, int yPos)
		{
			Label lblLabel = new Label
			{
				Text = label,
				Font = new Font("Segoe UI", 11, FontStyle.Regular),
				ForeColor = _clrTextGray,
				AutoSize = true,
				Location = new Point(0, yPos)
			};
			parent.Controls.Add(lblLabel);

			Label lblValue = new Label
			{
				Text = value,
				Font = new Font("Segoe UI", 11, FontStyle.Bold),
				ForeColor = _clrText,
				AutoSize = true,
				Location = new Point(150, yPos)
			};
			parent.Controls.Add(lblValue);

			return yPos + 35;
		}

		private bool CanCancelBooking()
		{
			if (_booking.BookingStatus != "Confirmed")
				return false;

			if (_booking.PaymentStatus != "Paid")
				return false;

			if (_booking.DepartureTime <= DateTime.Now)
				return false;

			return true;
		}

		private string GetCancellationDisabledReason()
		{
			if (_booking.BookingStatus == "Cancelled")
				return "⚠️ Vé đã được hủy";

			if (_booking.BookingStatus == "Pending")
				return "⚠️ Vé chưa được xác nhận";

			if (_booking.DepartureTime <= DateTime.Now)
				return "⚠️ Không thể hủy vé sau khi tàu đã khởi hành";

			if (_booking.PaymentStatus != "Paid")
				return "⚠️ Chỉ có thể hủy vé đã thanh toán";

			return "⚠️ Không thể hủy vé này";
		}

		private async void BtnCancel_Click(object? sender, EventArgs e)
		{
			if (_isCancelling)
				return;

			var confirmResult = MessageBox.Show(
				$"Bạn có chắc chắn muốn hủy vé #{_booking.BookingId:00000}?\n\n" +
				$"Tàu: {_booking.TrainNumber} - {_booking.TrainName}\n" +
				$"Ghế: {string.Join(", ", _booking.SeatNumbers)}\n" +
				$"Tổng tiền: {_booking.TotalAmount:N0}đ\n\n" +
				"Số tiền sẽ được hoàn trả về tài khoản của bạn.",
				"Xác nhận hủy vé",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question
			);

			if (confirmResult != DialogResult.Yes)
				return;

			await CancelBookingAsync();
		}

		private async System.Threading.Tasks.Task CancelBookingAsync()
		{
			_isCancelling = true;

			if (_btnCancel != null)
			{
				_btnCancel.Enabled = false;
				_btnCancel.Text = "Đang hủy...";
			}

			try
			{
				var apiClient = SessionManager.Instance.ApiClient;
				if (apiClient == null)
				{
					MessageBox.Show(
						"Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.",
						"Lỗi",
						MessageBoxButtons.OK,
						MessageBoxIcon.Error
					);
					return;
				}

				var bookingService = new sdk_client.Services.BookingService(apiClient);
				var response = await bookingService.CancelBookingAsync(_booking.BookingId);

				if (response.Success)
				{
					MessageBox.Show(
						"Hủy vé thành công!\n\nSố tiền sẽ được hoàn trả về tài khoản của bạn trong vòng 3-5 ngày làm việc.",
						"Thành công",
						MessageBoxButtons.OK,
						MessageBoxIcon.Information
					);

					BookingCancelled?.Invoke(this, EventArgs.Empty);
					this.Close();
				}
				else
				{
					MessageBox.Show(
						$"Không thể hủy vé: {response.ErrorMessage}",
						"Lỗi",
						MessageBoxButtons.OK,
						MessageBoxIcon.Error
					);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(
					$"Lỗi kết nối: {ex.Message}",
					"Lỗi",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error
				);
			}
			finally
			{
				_isCancelling = false;
				if (_btnCancel != null)
				{
					_btnCancel.Enabled = true;
					_btnCancel.Text = "🗑️ Hủy vé";
				}
			}
		}

		private Color GetStatusColor(string status)
		{
			return status switch
			{
				"Confirmed" => _clrSuccess,
				"Pending" => _clrWarning,
				"Cancelled" => _clrError,
				_ => _clrTextGray
			};
		}

		private string GetStatusText(string status)
		{
			return status switch
			{
				"Confirmed" => "✓ Đã xác nhận",
				"Pending" => "⏳ Chờ xác nhận",
				"Cancelled" => "✕ Đã hủy",
				_ => status
			};
		}

		private string GetPaymentStatusText(string paymentStatus)
		{
			return paymentStatus switch
			{
				"Paid" => "✓ Đã thanh toán",
				"Pending" => "⏳ Chờ thanh toán",
				"Refunded" => "↩ Đã hoàn tiền",
				_ => paymentStatus
			};
		}

		private static GraphicsPath GetRoundedPath(Rectangle rect, int radius)
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
			if (e.Button == MouseButtons.Left && e.Y < 60)
			{
				ReleaseCapture();
				SendMessage(Handle, 0xA1, 0x2, 0);
			}
		}
	}
}