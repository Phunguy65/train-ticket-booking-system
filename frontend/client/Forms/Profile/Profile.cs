using client.Forms.Authentication; // Sử dụng lại RoundedButton & ModernTextBox
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using client.Services;
using Newtonsoft.Json;
using sdk_client.Protocol;

namespace client.Forms.Profile
{
	public partial class Profile : Form
	{
		// =========================================================
		// 1. CẤU HÌNH MÀU SẮC & THEME
		// =========================================================
		private readonly Color _clrBackground = Color.FromArgb(15, 23, 42);
		private readonly Color _clrHeader = Color.FromArgb(15, 23, 42);
		private readonly Color _clrTabActive = Color.FromArgb(37, 99, 235);
		private readonly Color _clrText = Color.White;
		private readonly Color _clrTextGray = Color.FromArgb(148, 163, 184);
		private readonly Color _clrItemBg = Color.FromArgb(30, 41, 59);

		private readonly Color _clrSuccess = Color.FromArgb(34, 197, 94);
		private readonly Color _clrWarning = Color.FromArgb(249, 115, 22);
		private readonly Color _clrError = Color.FromArgb(239, 68, 68);

		// Các biến UI Control
		private Panel _pnlContent;
		private Label _btnTabHistory, _btnTabProfile;
		private Panel _lineActiveTab;
		private bool _isMaximized;

		// Cấu hình cột: [Mã vé, Tàu, Ngày đi, Trạng thái, Giá tiền]
		private readonly int[] _colWidths = [200, 350, 250, 250, 200];

		// Pagination state variables
		private int _currentPage = 1;
		private int _totalPages = 1;
		private int _totalCount = 0;
		private const int _pageSize = 10; // 10 items per page

		// Pagination UI controls
		private Panel _pnlPagination;
		private Label _lblPageInfo;
		private RoundedButton _btnPrevious, _btnNext;
		private FlowLayoutPanel _flowList;

		public Profile()
		{
			InitializeComponent();
			SetupUI();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SwitchTab("HISTORY");
		}

		// =========================================================
		// 2. DỰNG GIAO DIỆN CHUNG
		// =========================================================
		private void SetupUI()
		{
			this.FormBorderStyle = FormBorderStyle.None;
			this.StartPosition = FormStartPosition.CenterScreen;
			this.Size = new Size(1500, 850);
			this.BackColor = _clrBackground;
			this.DoubleBuffered = true;

			// HEADER
			Panel pnlHeader = new Panel
			{
				Dock = DockStyle.Top, Height = 60, Padding = new Padding(30, 0, 30, 0), BackColor = _clrHeader
			};
			Label lblLogo = new Label
			{
				Text = "🚆 Vé Tàu Cao Tốc",
				Font = new Font("Segoe UI", 16, FontStyle.Bold),
				ForeColor = _clrTabActive,
				AutoSize = true,
				Location = new Point(30, 15),
				Cursor = Cursors.Hand
			};
			lblLogo.Click += (_, _) => this.Close();
			pnlHeader.Controls.Add(lblLogo);
			AddWindowControls(pnlHeader);
			SetupHeaderMenu(pnlHeader);
			this.Controls.Add(pnlHeader);

			// PAGE TITLE
			Panel pnlPageTitle = new Panel { Dock = DockStyle.Top, Height = 70, Padding = new Padding(50, 0, 50, 0) };
			Label lblPageTitle = new Label
			{
				Text = "Quản lý tài khoản",
				Font = new Font("Segoe UI", 24, FontStyle.Bold),
				ForeColor = Color.White,
				AutoSize = true,
				Location = new Point(50, 10)
			};
			pnlPageTitle.Controls.Add(lblPageTitle);

			// TABS
			Panel pnlTabs = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(50, 0, 50, 0) };
			_btnTabHistory = CreateTabButton("Lịch sử đặt vé", 50);
			_btnTabHistory.Click += (_, _) => SwitchTab("HISTORY");
			pnlTabs.Controls.Add(_btnTabHistory);

			_btnTabProfile = CreateTabButton("Hồ sơ cá nhân", 250);
			_btnTabProfile.Click += (_, _) => SwitchTab("PROFILE");
			pnlTabs.Controls.Add(_btnTabProfile);

			_lineActiveTab = new Panel
			{
				Height = 4, BackColor = _clrTabActive, Location = new Point(50, 46), Size = new Size(100, 4)
			};
			pnlTabs.Controls.Add(_lineActiveTab);

			// CONTENT
			_pnlContent = new Panel { Dock = DockStyle.Fill, Padding = new Padding(50, 20, 50, 30) };

			// Thứ tự Add quan trọng cho Dock: Content trước -> Tabs -> Title -> Header
			this.Controls.Add(_pnlContent);
			this.Controls.Add(pnlTabs);
			this.Controls.Add(pnlPageTitle);
			this.Controls.Add(pnlHeader);
		}

		private void SwitchTab(string tabName)
		{
			_pnlContent.Controls.Clear();

			if (tabName == "HISTORY")
			{
				_btnTabHistory.ForeColor = _clrText;
				_btnTabProfile.ForeColor = _clrTextGray;
				_lineActiveTab.Location = _btnTabHistory.Location with { Y = 46 };
				_lineActiveTab.Width = _btnTabHistory.Width;
				LoadHistoryContent();
			}
			else
			{
				_btnTabHistory.ForeColor = _clrTextGray;
				_btnTabProfile.ForeColor = _clrText;
				_lineActiveTab.Location = _btnTabProfile.Location with { Y = 46 };
				_lineActiveTab.Width = _btnTabProfile.Width;
				LoadProfileContent();
			}
		}

		// =========================================================
		// 3. TAB LỊCH SỬ - LOAD REAL DATA WITH PAGINATION
		// =========================================================
		private async void LoadHistoryContent()
		{
			// Reset pagination state
			_currentPage = 1;

			// Clear content panel
			_pnlContent.Controls.Clear();

			// 1. Tạo Header Bảng
			Panel pnlTableHeader = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = _clrBackground };

			string[] headers = ["MÃ VÉ", "THÔNG TIN TÀU", "NGÀY ĐI", "TRẠNG THÁI", "TỔNG TIỀN"];
			int curX = 20;
			for (int i = 0; i < headers.Length; i++)
			{
				Label lblH = new Label
				{
					Text = headers[i],
					ForeColor = _clrTextGray,
					Font = new Font("Segoe UI", 10, FontStyle.Bold),
					AutoSize = false,
					Size = new Size(_colWidths[i], 40),
					Location = new Point(curX, 10),
					TextAlign = ContentAlignment.MiddleLeft
				};
				pnlTableHeader.Controls.Add(lblH);
				curX += _colWidths[i];
			}

			// 2. Container danh sách
			_flowList = new FlowLayoutPanel
			{
				Dock = DockStyle.Fill,
				FlowDirection = FlowDirection.TopDown,
				WrapContents = false,
				AutoScroll = true,
				Padding = new Padding(0, 10, 0, 0),
				BackColor = Color.Transparent
			};

			// 3. Create pagination panel
			_pnlPagination = CreatePaginationPanel();

			// 4. Add controls in correct dock order
			_pnlContent.Controls.Add(_flowList);
			_pnlContent.Controls.Add(_pnlPagination);
			_pnlContent.Controls.Add(pnlTableHeader);

			// Ensure proper docking
			pnlTableHeader.Dock = DockStyle.Top;
			_flowList.Dock = DockStyle.Fill;
			_pnlPagination.Dock = DockStyle.Bottom;

			// 5. Load first page
			await LoadBookingHistoryPageAsync(_currentPage);
		}

		private async Task LoadBookingHistoryPageAsync(int pageNumber)
		{
			try
			{
				// Show loading indicator
				_flowList.Controls.Clear();
				Label lblLoading = new Label
				{
					Text = "⏳ Đang tải lịch sử đặt vé...",
					Font = new Font("Segoe UI", 12, FontStyle.Regular),
					ForeColor = _clrTextGray,
					AutoSize = true,
					Location = new Point(20, 20)
				};
				_flowList.Controls.Add(lblLoading);

				// Get API client from session manager
				var apiClient = SessionManager.Instance.ApiClient;
				if (apiClient == null)
				{
					ShowErrorMessage(_flowList, "Không thể kết nối đến máy chủ. Vui lòng đăng nhập lại.");
					return;
				}

				// Create booking service and fetch paginated history
				var bookingService = new sdk_client.Services.BookingService(apiClient);
				var response = await bookingService.GetBookingHistoryAsync(pageNumber, _pageSize);

				// Remove loading indicator
				_flowList.Controls.Remove(lblLoading);

				// Parse response
				if (response == null)
				{
					ShowEmptyState(_flowList);
					return;
				}

				// Deserialize to PagedResult
				var jsonString = JsonConvert.SerializeObject(response);
				var pagedResult = JsonConvert.DeserializeObject<PagedResult<BookingHistoryDTO>>(jsonString);

				if (pagedResult == null || !pagedResult.Items.Any())
				{
					ShowEmptyState(_flowList);
					return;
				}

				// Update pagination state
				_currentPage = pagedResult.PageNumber;
				_totalPages = pagedResult.TotalPages;
				_totalCount = pagedResult.TotalCount;

				// Render booking history items
				foreach (var booking in pagedResult.Items)
				{
					AddHistoryItemFromData(_flowList, booking);
				}

				// Update pagination controls
				UpdatePaginationControls();
			}
			catch (Exception ex)
			{
				ShowErrorMessage(_flowList, $"Lỗi khi tải lịch sử: {ex.Message}");
			}
		}

		private Panel CreatePaginationPanel()
		{
			Panel pnlPagination = new Panel
			{
				Height = 80,
				Dock = DockStyle.Bottom,
				BackColor = Color.Transparent,
				Padding = new Padding(50, 20, 50, 20)
			};

			// Previous button
			_btnPrevious = new RoundedButton
			{
				Text = "← Trang trước",
				Size = new Size(150, 40),
				Location = new Point(50, 20),
				BackColor = _clrItemBg,
				ForeColor = _clrText,
				Font = new Font("Segoe UI", 10, FontStyle.Regular),
				Cursor = Cursors.Hand,
				FlatStyle = FlatStyle.Flat,
				Enabled = false
			};
			_btnPrevious.FlatAppearance.BorderSize = 0;
			_btnPrevious.Click += async (_, _) => await OnPreviousPage();

			// Page info label
			_lblPageInfo = new Label
			{
				Text = "Trang 1/1 (0 vé)",
				Font = new Font("Segoe UI", 11, FontStyle.Regular),
				ForeColor = _clrTextGray,
				AutoSize = true,
				Location = new Point(220, 30),
				TextAlign = ContentAlignment.MiddleCenter
			};

			// Next button
			_btnNext = new RoundedButton
			{
				Text = "Trang sau →",
				Size = new Size(150, 40),
				Location = new Point(450, 20),
				BackColor = _clrTabActive,
				ForeColor = Color.White,
				Font = new Font("Segoe UI", 10, FontStyle.Regular),
				Cursor = Cursors.Hand,
				FlatStyle = FlatStyle.Flat,
				Enabled = false
			};
			_btnNext.FlatAppearance.BorderSize = 0;
			_btnNext.Click += async (_, _) => await OnNextPage();

			pnlPagination.Controls.Add(_btnPrevious);
			pnlPagination.Controls.Add(_lblPageInfo);
			pnlPagination.Controls.Add(_btnNext);

			return pnlPagination;
		}

		private async Task OnPreviousPage()
		{
			if (_currentPage <= 1) return;
			await LoadBookingHistoryPageAsync(_currentPage - 1);
		}

		private async Task OnNextPage()
		{
			if (_currentPage >= _totalPages) return;
			await LoadBookingHistoryPageAsync(_currentPage + 1);
		}

		private void UpdatePaginationControls()
		{
			if (_lblPageInfo == null || _btnPrevious == null || _btnNext == null) return;

			_lblPageInfo.Text = $"Trang {_currentPage}/{_totalPages} ({_totalCount} vé)";
			_btnPrevious.Enabled = _currentPage > 1;
			_btnNext.Enabled = _currentPage < _totalPages;

			// Visual feedback for disabled buttons
			_btnPrevious.BackColor = _btnPrevious.Enabled ? _clrItemBg : Color.FromArgb(20, 30, 45);
			_btnNext.BackColor = _btnNext.Enabled ? _clrTabActive : Color.FromArgb(20, 50, 100);
		}

		private async Task LoadBookingHistoryDataAsync(FlowLayoutPanel flowList)
		{
			try
			{
				// Show loading indicator
				Label lblLoading = new Label
				{
					Text = "⏳ Đang tải lịch sử đặt vé...",
					Font = new Font("Segoe UI", 12, FontStyle.Regular),
					ForeColor = _clrTextGray,
					AutoSize = true,
					Location = new Point(20, 20)
				};
				flowList.Controls.Add(lblLoading);

				// Get API client from session manager
				var apiClient = SessionManager.Instance.ApiClient;
				if (apiClient == null)
				{
					ShowErrorMessage(flowList, "Không thể kết nối đến máy chủ. Vui lòng đăng nhập lại.");
					return;
				}

				// Create booking service and fetch history
				var bookingService = new sdk_client.Services.BookingService(apiClient);
				var response = await bookingService.GetBookingHistoryAsync();

				// Remove loading indicator
				flowList.Controls.Remove(lblLoading);

				// Parse response
				if (response == null)
				{
					ShowEmptyState(flowList);
					return;
				}

				var bookingHistory = ParseBookingHistory(response);

				if (bookingHistory == null || bookingHistory.Count == 0)
				{
					ShowEmptyState(flowList);
					return;
				}

				// Render booking history items
				foreach (var booking in bookingHistory)
				{
					AddHistoryItemFromData(flowList, booking);
				}
			}
			catch (Exception ex)
			{
				ShowErrorMessage(flowList, $"Lỗi khi tải lịch sử: {ex.Message}");
			}
		}

		private List<BookingHistoryDTO>? ParseBookingHistory(object response)
		{
			try
			{
				var jsonString = JsonConvert.SerializeObject(response);
				return JsonConvert.DeserializeObject<List<BookingHistoryDTO>>(jsonString);
			}
			catch
			{
				return null;
			}
		}

		private void ShowEmptyState(FlowLayoutPanel flowList)
		{
			flowList.Controls.Clear();
			Label lblEmpty = new Label
			{
				Text = "📋 Bạn chưa có lịch sử đặt vé nào",
				Font = new Font("Segoe UI", 14, FontStyle.Regular),
				ForeColor = _clrTextGray,
				AutoSize = true,
				Location = new Point(20, 20)
			};
			flowList.Controls.Add(lblEmpty);
		}

		private void ShowErrorMessage(FlowLayoutPanel flowList, string message)
		{
			flowList.Controls.Clear();
			Label lblError = new Label
			{
				Text = $"❌ {message}",
				Font = new Font("Segoe UI", 12, FontStyle.Regular),
				ForeColor = _clrError,
				AutoSize = true,
				Location = new Point(20, 20)
			};
			flowList.Controls.Add(lblError);
		}

		private void AddHistoryItem(FlowLayoutPanel parent, string code, string train, string date, string status,
			Color statusColor, string price)
		{
			int itemWidth = parent.ClientSize.Width - 20;
			if (itemWidth < 1200) itemWidth = 1200;

			Panel pnlItem = new Panel
			{
				Size = new Size(itemWidth, 70), Margin = new Padding(0, 0, 0, 15), BackColor = Color.Transparent
			};

			pnlItem.Paint += (_, e) =>
			{
				e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
				Rectangle rect = new Rectangle(0, 0, pnlItem.Width - 1, pnlItem.Height - 1);
				using GraphicsPath path = GetRoundedPath(rect, 15);
				using SolidBrush brush = new SolidBrush(_clrItemBg);
				e.Graphics.FillPath(brush, path);
			};

			int curX = 20;
			pnlItem.Controls.Add(CreateLabel(code, 11, FontStyle.Bold, _clrText, curX, 25));
			curX += _colWidths[0];
			pnlItem.Controls.Add(CreateLabel(train, 11, FontStyle.Regular, _clrText, curX, 25));
			curX += _colWidths[1];
			pnlItem.Controls.Add(CreateLabel(date, 11, FontStyle.Regular, _clrTextGray, curX, 25));
			curX += _colWidths[2];
			Label lblStatus = new Label
			{
				Text = status,
				ForeColor = statusColor,
				Font = new Font("Segoe UI", 10, FontStyle.Bold),
				AutoSize = true,
				Location = new Point(curX, 25)
			};
			pnlItem.Controls.Add(lblStatus);
			curX += _colWidths[3];
			pnlItem.Controls.Add(CreateLabel(price, 12, FontStyle.Bold, _clrText, curX, 23));

			parent.Controls.Add(pnlItem);
		}

		private void AddHistoryItemFromData(FlowLayoutPanel parent, BookingHistoryDTO booking)
		{
			// Format booking code
			string code = $"#VE{booking.BookingId:00000}";

			// Format train info: Train Name - Station to Station (Seats)
			string seatText = string.Join(", ", booking.SeatNumbers);
			string train = $"{booking.TrainName} - {booking.DepartureStation} → {booking.ArrivalStation} ({seatText})";

			// Format date
			string date = booking.DepartureTime.ToString("dd/MM/yyyy HH:mm");

			// Determine status and color based on booking status
			string status;
			Color statusColor;
			switch (booking.BookingStatus)
			{
				case "Confirmed":
					status = "Đã xác nhận";
					statusColor = _clrSuccess;
					break;
				case "Pending":
					status = "Chờ xác nhận";
					statusColor = _clrWarning;
					break;
				case "Cancelled":
					status = "Đã hủy";
					statusColor = _clrError;
					break;
				default:
					status = booking.BookingStatus;
					statusColor = _clrTextGray;
					break;
			}

			// Format price
			string price = $"{booking.TotalAmount:N0}đ";

			// Use existing AddHistoryItem method to render
			AddHistoryItem(parent, code, train, date, status, statusColor, price);
		}

		// =========================================================
		// 4. TAB HỒ SƠ (GIỮ NGUYÊN)
		// =========================================================
		private void LoadProfileContent()
		{
			Panel pnlProfile = new Panel { Size = new Size(800, 500), Location = new Point(20, 20) };
			Label lblHeader = new Label
			{
				Text = "Thông tin cá nhân",
				Font = new Font("Segoe UI", 14, FontStyle.Bold),
				ForeColor = _clrText,
				AutoSize = true,
				Location = new Point(0, 0)
			};
			Label lblSub = new Label
			{
				Text = "Cập nhật thông tin định danh và liên hệ của bạn.",
				Font = new Font("Segoe UI", 10, FontStyle.Regular),
				ForeColor = _clrTextGray,
				AutoSize = true,
				Location = new Point(0, 35)
			};
			pnlProfile.Controls.Add(lblHeader);
			pnlProfile.Controls.Add(lblSub);

			int yPos = 80;
			pnlProfile.Controls.Add(CreateLabel("Họ và Tên", 10, FontStyle.Regular, _clrTextGray, 0, yPos));
			pnlProfile.Controls.Add(CreateLabel("Số điện thoại", 10, FontStyle.Regular, _clrTextGray, 420, yPos));
			yPos += 30;

			ModernTextBox txtName = new ModernTextBox
			{
				Location = new Point(0, yPos),
				Size = new Size(380, 50),
				PlaceholderText = "Nguyễn Văn A",
				BackColor = _clrItemBg,
				ForeColor = _clrText,
				IconText = "👤"
			};
			pnlProfile.Controls.Add(txtName);
			ModernTextBox txtPhone = new ModernTextBox
			{
				Location = new Point(420, yPos),
				Size = new Size(380, 50),
				PlaceholderText = "0909123456",
				BackColor = _clrItemBg,
				ForeColor = _clrText,
				IconText = "📞"
			};
			pnlProfile.Controls.Add(txtPhone);
			yPos += 70;

			pnlProfile.Controls.Add(CreateLabel("Địa chỉ Email", 10, FontStyle.Regular, _clrTextGray, 0, yPos));
			yPos += 30;
			ModernTextBox txtEmail = new ModernTextBox
			{
				Location = new Point(0, yPos),
				Size = new Size(800, 50),
				PlaceholderText = "example@email.com",
				BackColor = _clrItemBg,
				ForeColor = _clrText,
				IconText = "📧"
			};
			pnlProfile.Controls.Add(txtEmail);
			yPos += 90;

			RoundedButton btnUpdate = new RoundedButton
			{
				Text = "Lưu thay đổi",
				BackColor = _clrTabActive,
				ForeColor = Color.White,
				Size = new Size(200, 50),
				Location = new Point(0, yPos),
				Font = new Font("Segoe UI", 11, FontStyle.Bold),
				Cursor = Cursors.Hand,
				FlatStyle = FlatStyle.Flat
			};
			btnUpdate.FlatAppearance.BorderSize = 0;
			btnUpdate.Click += (_, _) => MessageBox.Show(@"Cập nhật thông tin thành công!", @"Hệ thống");
			pnlProfile.Controls.Add(btnUpdate);

			_pnlContent.Controls.Add(pnlProfile);
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
					ForeColor = _clrTextGray,
					AutoSize = true,
					Cursor = Cursors.Hand,
					Anchor = AnchorStyles.Top | AnchorStyles.Right
				};
				lblMenu.Location = new Point(menuX - 80, 20);

				lblMenu.MouseEnter += (_, _) => lblMenu.ForeColor = Color.White;
				lblMenu.MouseLeave += (_, _) => lblMenu.ForeColor = _clrTextGray;

				// --- XỬ LÝ SỰ KIỆN CLICK TẠI ĐÂY ---
				if (item == "Đăng xuất")
				{
					// Đóng hết ứng dụng hoặc quay về Login tùy logic
					lblMenu.Click += (_, _) => Application.Exit();
				}
				else if (item == "Trang chủ")
				{
					// CHỈ CẦN ĐÓNG PROFILE LÀ TỰ QUAY VỀ MAINFORM
					lblMenu.Click += (_, _) => this.Close();
				}
				// ------------------------------------

				pnlHeader.Controls.Add(lblMenu);
				menuX -= 120;
			}
		}

		private void AddWindowControls(Panel parent)
		{
			int btnSize = 45;
			int startX = parent.Width - (btnSize * 3) - 10;
			Label btnClose = CreateWindowButton("✕", startX + (btnSize * 2), _clrError);
			btnClose.Click += (_, _) => this.Close();
			btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			parent.Controls.Add(btnClose);
			Label btnMax = CreateWindowButton("☐", startX + btnSize, _clrItemBg);
			btnMax.Click += (_, _) => ToggleMaximize();
			btnMax.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			parent.Controls.Add(btnMax);
			Label btnMin = CreateWindowButton("―", startX, _clrItemBg);
			btnMin.Click += (_, _) => this.WindowState = FormWindowState.Minimized;
			btnMin.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			parent.Controls.Add(btnMin);
		}

		private Label CreateWindowButton(string text, int x, Color hoverColor)
		{
			Label lbl = new Label
			{
				Text = text,
				Font = new Font("Segoe UI", 12, FontStyle.Regular),
				ForeColor = Color.White,
				AutoSize = false,
				Size = new Size(45, 30),
				Location = new Point(x, 15),
				TextAlign = ContentAlignment.MiddleCenter,
				Cursor = Cursors.Hand
			};
			lbl.MouseEnter += (_, _) => lbl.BackColor = hoverColor;
			lbl.MouseLeave += (_, _) => lbl.BackColor = Color.Transparent;
			return lbl;
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

		private Label CreateTabButton(string text, int x)
		{
			return new Label
			{
				Text = text,
				Font = new Font("Segoe UI", 12, FontStyle.Bold),
				ForeColor = _clrTextGray,
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