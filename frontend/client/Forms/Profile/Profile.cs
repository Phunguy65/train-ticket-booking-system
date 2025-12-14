using client.Components; // Sử dụng lại RoundedButton & ModernTextBox
using client.Services;
using Newtonsoft.Json;
using sdk_client.Protocol;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

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

		// Profile form controls
		private ModernTextBox _txtFullName;
		private ModernTextBox _txtEmail;
		private ModernTextBox _txtPhoneNumber;
		private User _originalUserData;
		private bool _isLoadingProfile;
		private bool _isSavingProfile;

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
				var pagedResult = JsonConvert.DeserializeObject<PagedResult<BookingHistory>>(jsonString);

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
			_lblPageInfo.Text = $"Trang {_currentPage}/{_totalPages} ({_totalCount} vé)";
			_btnPrevious.Enabled = _currentPage > 1;
			_btnNext.Enabled = _currentPage < _totalPages;

			// Visual feedback for disabled buttons
			_btnPrevious.BackColor = _btnPrevious.Enabled ? _clrItemBg : Color.FromArgb(20, 30, 45);
			_btnNext.BackColor = _btnNext.Enabled ? _clrTabActive : Color.FromArgb(20, 50, 100);
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
			Color statusColor, string price, BookingHistory? booking = null)
		{
			int itemWidth = parent.ClientSize.Width - 20;
			if (itemWidth < 1200) itemWidth = 1200;

			Panel pnlItem = new Panel
			{
				Size = new Size(itemWidth, 70),
				Margin = new Padding(0, 0, 0, 15),
				BackColor = Color.Transparent,
				Cursor = Cursors.Hand
			};

			pnlItem.Paint += (_, e) =>
			{
				e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
				Rectangle rect = new Rectangle(0, 0, pnlItem.Width - 1, pnlItem.Height - 1);
				using GraphicsPath path = GetRoundedPath(rect, 15);
				using SolidBrush brush = new SolidBrush(_clrItemBg);
				e.Graphics.FillPath(brush, path);
			};

			pnlItem.MouseEnter += (_, _) => { pnlItem.BackColor = Color.FromArgb(51, 65, 85); };
			pnlItem.MouseLeave += (_, _) => { pnlItem.BackColor = Color.Transparent; };

			if (booking != null)
			{
				pnlItem.Click += (_, _) => OpenBookingDetail(booking);
			}

			int curX = 20;
			Label lblCode = CreateLabel(code, 11, FontStyle.Bold, _clrText, curX, 25);
			if (booking != null) lblCode.Click += (_, _) => OpenBookingDetail(booking);
			pnlItem.Controls.Add(lblCode);
			curX += _colWidths[0];

			Label lblTrain = CreateLabel(train, 11, FontStyle.Regular, _clrText, curX, 25);
			if (booking != null) lblTrain.Click += (_, _) => OpenBookingDetail(booking);
			pnlItem.Controls.Add(lblTrain);
			curX += _colWidths[1];

			Label lblDate = CreateLabel(date, 11, FontStyle.Regular, _clrTextGray, curX, 25);
			if (booking != null) lblDate.Click += (_, _) => OpenBookingDetail(booking);
			pnlItem.Controls.Add(lblDate);
			curX += _colWidths[2];

			Label lblStatus = new Label
			{
				Text = status,
				ForeColor = statusColor,
				Font = new Font("Segoe UI", 10, FontStyle.Bold),
				AutoSize = true,
				Location = new Point(curX, 25),
				Cursor = Cursors.Hand
			};
			if (booking != null) lblStatus.Click += (_, _) => OpenBookingDetail(booking);
			pnlItem.Controls.Add(lblStatus);
			curX += _colWidths[3];

			Label lblPrice = CreateLabel(price, 12, FontStyle.Bold, _clrText, curX, 23);
			if (booking != null) lblPrice.Click += (_, _) => OpenBookingDetail(booking);
			pnlItem.Controls.Add(lblPrice);

			parent.Controls.Add(pnlItem);
		}

		private void AddHistoryItemFromData(FlowLayoutPanel parent, BookingHistory booking)
		{
			// Format booking code
			string code = $"#VE{booking.BookingId:00000}";

			// Format train info: Train Name - Station to Station (Seats)
			string seatText = string.Join(", ", booking.SeatNumbers);
			string train = $@"{booking.TrainName} ({seatText})";

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

			// Use existing AddHistoryItem method to render with booking data
			AddHistoryItem(parent, code, train, date, status, statusColor, price, booking);
		}

		private void OpenBookingDetail(BookingHistory booking)
		{
			var detailForm = new Booking.BookingDetail(booking);
			detailForm.BookingCancelled += async (s, e) =>
			{
				await LoadBookingHistoryPageAsync(_currentPage);
			};
			detailForm.ShowDialog(this);
		}

		// =========================================================
		// 4. TAB HỒ SƠ (REFACTORED WITH DATA LOADING)
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

			_txtFullName = new ModernTextBox
			{
				Location = new Point(0, yPos),
				Size = new Size(380, 50),
				PlaceholderText = "Nguyễn Văn A",
				BackColor = _clrItemBg,
				ForeColor = _clrText,
				IconText = "👤"
			};
			pnlProfile.Controls.Add(_txtFullName);

			_txtPhoneNumber = new ModernTextBox
			{
				Location = new Point(420, yPos),
				Size = new Size(380, 50),
				PlaceholderText = "0909123456",
				BackColor = _clrItemBg,
				ForeColor = _clrText,
				IconText = "📞"
			};
			pnlProfile.Controls.Add(_txtPhoneNumber);
			yPos += 70;

			pnlProfile.Controls.Add(CreateLabel("Địa chỉ Email", 10, FontStyle.Regular, _clrTextGray, 0, yPos));
			yPos += 30;

			_txtEmail = new ModernTextBox
			{
				Location = new Point(0, yPos),
				Size = new Size(800, 50),
				PlaceholderText = "example@email.com",
				BackColor = _clrItemBg,
				ForeColor = _clrText,
				IconText = "📧"
			};
			pnlProfile.Controls.Add(_txtEmail);
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
			btnUpdate.Click += async (_, _) => await HandleSaveProfileAsync();
			pnlProfile.Controls.Add(btnUpdate);

			_pnlContent.Controls.Add(pnlProfile);

			// Load user data asynchronously
			Task.Run(async () => await LoadUserDataAsync());
		}

		private async Task LoadUserDataAsync()
		{
			if (_isLoadingProfile) return;

			_isLoadingProfile = true;

			try
			{
				var apiClient = SessionManager.Instance.ApiClient;
				if (apiClient == null)
				{
					this.Invoke((MethodInvoker)delegate
					{
						MessageBox.Show(@"Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", @"Lỗi",
							MessageBoxButtons.OK, MessageBoxIcon.Error);
					});
					return;
				}

				var userService = new sdk_client.Services.UserService(apiClient);
				var user = await userService.GetCurrentUserAsync();

				if (user == null)
				{
					this.Invoke((MethodInvoker)delegate
					{
						MessageBox.Show(@"Không thể tải thông tin người dùng.", @"Lỗi",
							MessageBoxButtons.OK, MessageBoxIcon.Error);
					});
					return;
				}

				// Store original user data for change detection
				_originalUserData = user;

				// Update UI controls with user data
				this.Invoke((MethodInvoker)delegate
				{
					_txtFullName.SetText(user.FullName);
					_txtEmail.SetText(user.Email);
					_txtPhoneNumber.SetText(user.PhoneNumber ?? string.Empty);
				});
			}
			catch (Exception ex)
			{
				this.Invoke((MethodInvoker)delegate
				{
					MessageBox.Show($@"Lỗi khi tải thông tin người dùng: {ex.Message}", @"Lỗi",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
				});
			}
			finally
			{
				_isLoadingProfile = false;
			}
		}

		private async Task HandleSaveProfileAsync()
		{
			if (_isSavingProfile) return;

			// Validate input fields
			string fullName = _txtFullName.GetText().Trim();
			string email = _txtEmail.GetText().Trim();
			string phoneNumber = _txtPhoneNumber.GetText().Trim();

			// Validation: Full Name is required
			if (string.IsNullOrEmpty(fullName))
			{
				MessageBox.Show(@"Họ và tên không được để trống.", @"Lỗi", MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				return;
			}

			// Validation: Email is required and must be valid format
			if (string.IsNullOrEmpty(email))
			{
				MessageBox.Show(@"Địa chỉ email không được để trống.", @"Lỗi", MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				return;
			}

			if (!IsValidEmail(email))
			{
				MessageBox.Show(@"Địa chỉ email không hợp lệ.", @"Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			// Phone number is optional, but if provided, validate format
			if (!string.IsNullOrEmpty(phoneNumber) && !IsValidPhoneNumber(phoneNumber))
			{
				MessageBox.Show(
					@"Số điện thoại không hợp lệ. Vui lòng nhập số điện thoại Việt Nam (ví dụ: 0909123456).", @"Lỗi",
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			// Check if any changes were made
			bool hasChanges = fullName != _originalUserData.FullName ||
			                  email != _originalUserData.Email ||
			                  phoneNumber != (_originalUserData.PhoneNumber ?? string.Empty);

			if (!hasChanges)
			{
				MessageBox.Show(@"Không có thay đổi nào để lưu.", @"Thông báo", MessageBoxButtons.OK,
					MessageBoxIcon.Information);
				return;
			}

			_isSavingProfile = true;

			try
			{
				var apiClient = SessionManager.Instance.ApiClient;
				if (apiClient == null)
				{
					MessageBox.Show(@"Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", @"Lỗi",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				var userService = new sdk_client.Services.UserService(apiClient);
				await userService.UpdateUserProfileAsync(fullName, email,
					string.IsNullOrEmpty(phoneNumber) ? null : phoneNumber);

				// Update original data after successful save
				_originalUserData = new User { FullName = fullName, Email = email, PhoneNumber = phoneNumber };

				MessageBox.Show(@"Cập nhật thông tin thành công!", @"Thành công", MessageBoxButtons.OK,
					MessageBoxIcon.Information);
			}
			catch (Exception ex)
			{
				MessageBox.Show($@"Lỗi khi cập nhật thông tin: {ex.Message}", @"Lỗi",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				_isSavingProfile = false;
			}
		}

		private bool IsValidEmail(string email)
		{
			if (string.IsNullOrWhiteSpace(email))
				return false;

			try
			{
				var addr = new System.Net.Mail.MailAddress(email);
				return addr.Address == email;
			}
			catch
			{
				return false;
			}
		}

		private bool IsValidPhoneNumber(string phoneNumber)
		{
			if (string.IsNullOrWhiteSpace(phoneNumber))
				return true; // Optional field

			// Vietnamese phone number validation: starts with 0 and has 10 digits
			return System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, @"^0\d{9}$");
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
				MaximumSize = new Size(250, 0),
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