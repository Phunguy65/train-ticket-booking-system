using client.Forms.Authentication;
using client.Services;
using Newtonsoft.Json;
using sdk_client.Protocol;
using sdk_client.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace client.Forms.TrainSearch
{
	public partial class MainForm : Form
	{
		// =========================================================
		// 1. CẤU HÌNH MÀU SẮC (THEME DARK MODE)
		// =========================================================
		private readonly Color _clrBackground = Color.FromArgb(15, 23, 42); // Nền chính
		private readonly Color _clrSidebar = Color.FromArgb(30, 41, 59); // Nền sidebar
		private readonly Color _clrHeader = Color.FromArgb(15, 23, 42); // Nền Header
		private readonly Color _clrItemBg = Color.FromArgb(30, 41, 59); // Nền item kết quả
		private readonly Color _clrText = Color.White; // Chữ trắng
		private readonly Color _clrTextGray = Color.FromArgb(148, 163, 184); // Chữ xám
		private readonly Color _clrAccent = Color.FromArgb(37, 99, 235); // Màu xanh dương
		private readonly Color _clrGreen = Color.FromArgb(34, 197, 94); // Màu xanh lá
		private readonly Color _clrOrange = Color.FromArgb(249, 115, 22); // Màu cam
		private readonly Color _clrRed = Color.FromArgb(239, 68, 68); // Màu đỏ

		// Biến toàn cục
		private FlowLayoutPanel _flowResults;
		private ModernTextBox _txtDepStation, _txtArrStation, _txtDate;
		private bool _isMaximized;

		// Pagination state
		private int _currentPage = 1;
		private int _pageSize = 10;
		private int _totalPages;
		private int _totalCount;
		private bool _isLoading;

		// Search criteria state
		private string? _lastDepartureStation;
		private string? _lastArrivalStation;
		private DateTime? _lastDepartureDate;

		// Pagination controls
		private RoundedButton _btnPrevious;
		private RoundedButton _btnNext;
		private RoundedButton _btnRefresh;
		private RoundedButton _btnClearFilters;
		private Label _lblPageInfo;
		private Label _lblResultTitle;
		private Label _lblFilterCount;
		private Panel _pnlLoading;

		public MainForm()
		{
			InitializeComponent();
			SetupUi();
			this.Shown += MainForm_Shown;
		}

		private async void MainForm_Shown(object? sender, EventArgs e)
		{
			await LoadTrainsAsync(null, null, null, 1);
		}

		// =========================================================
		// 2. HÀM DỰNG GIAO DIỆN (SETUP UI)
		// =========================================================
		private void SetupUi()
		{
			this.FormBorderStyle = FormBorderStyle.None;
			this.StartPosition = FormStartPosition.CenterScreen;
			this.Size = new Size(1500, 850);
			this.BackColor = _clrBackground;
			this.DoubleBuffered = true;

			// --- HEADER ---
			Panel pnlHeader = new Panel
			{
				Dock = DockStyle.Top, Height = 60, BackColor = _clrHeader, Padding = new Padding(20, 0, 20, 0)
			};
			Label lblLogo = new Label
			{
				Text = @"🚆 Vé Tàu Cao Tốc",
				Font = new Font("Segoe UI", 14, FontStyle.Bold),
				ForeColor = _clrAccent,
				AutoSize = true,
				Location = new Point(20, 15)
			};
			pnlHeader.Controls.Add(lblLogo);
			AddWindowControls(pnlHeader); // Nút điều khiển cửa sổ

			// Display username from session
			var currentUser = SessionManager.Instance.CurrentUser;
			if (currentUser != null)
			{
				Label lblUsername = new Label
				{
					Text = $"👤 {currentUser.Username}",
					Font = new Font("Segoe UI", 10, FontStyle.Bold),
					ForeColor = _clrAccent,
					AutoSize = true,
					Anchor = AnchorStyles.Top | AnchorStyles.Right
				};
				lblUsername.Location = new Point(pnlHeader.Width - 280, 20);
				pnlHeader.Controls.Add(lblUsername);
			}

			string[] menuItems = { "Đăng xuất", "Tài khoản" };
			int menuX = pnlHeader.Width - 160;
			foreach (var item in menuItems)
			{
				Label lblMenu = new Label
				{
					Text = item,
					Font = new Font("Segoe UI", 10, FontStyle.Regular),
					ForeColor = _clrTextGray,
					AutoSize = true,
					Cursor = Cursors.Hand,
					Anchor = AnchorStyles.Top | AnchorStyles.Right
				};
				lblMenu.Location = new Point(menuX - 80, 20);
				lblMenu.MouseEnter += (_, _) => lblMenu.ForeColor = _clrAccent;
				lblMenu.MouseLeave += (_, _) => lblMenu.ForeColor = _clrTextGray;

				if (item == "Đăng xuất")
				{
					lblMenu.Click += HandleLogout;
				}
				else if (item == "Tài khoản")
				{
					lblMenu.Click += (_, _) =>
					{
						this.Hide();
						var profileForm = new client.Forms.Profile.Profile();
						profileForm.ShowDialog();
						// 3. Khi user đóng Profile, dòng này mới chạy -> Hiện lại form chính
						this.Show();
					};
					// -------------------------

					pnlHeader.Controls.Add(lblMenu);
					menuX -= 100;
				}

				this.Controls.Add(pnlHeader);

				// --- SIDEBAR ---
				int sidebarW = 320;
				Panel pnlSearch = new Panel
				{
					Size = new Size(sidebarW, 600),
					Location = new Point(30, 80),
					BackColor = Color.Transparent,
					Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom
				};
				pnlSearch.Paint += (_, e) =>
				{
					e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
					using GraphicsPath path = GetRoundedPath(
						new Rectangle(0, 0, pnlSearch.Width, pnlSearch.Height - 20),
						15);
					using SolidBrush brush = new SolidBrush(_clrSidebar);
					e.Graphics.FillPath(brush, path);
				};
				this.Controls.Add(pnlSearch);

				int yPos = 30;
				int xMargin = 25;
				int inputW = sidebarW - (xMargin * 2);
				pnlSearch.Controls.Add(CreateLabel("Tìm chuyến tàu", 14, FontStyle.Bold, _clrText, xMargin, yPos));
				yPos += 40;

				Label lblInstruction = new Label
				{
					Text = "Nhập thông tin để tìm kiếm hoặc để trống để xem tất cả",
					Font = new Font("Segoe UI", 8, FontStyle.Italic),
					ForeColor = _clrTextGray,
					AutoSize = false,
					Size = new Size(inputW, 30),
					Location = new Point(xMargin, yPos),
					TextAlign = ContentAlignment.TopLeft
				};
				pnlSearch.Controls.Add(lblInstruction);
				yPos += 40;

				pnlSearch.Controls.Add(CreateLabel("Ga đi (tùy chọn)", 10, FontStyle.Regular, _clrTextGray, xMargin,
					yPos));
				yPos += 30;
				_txtDepStation = new ModernTextBox
				{
					Location = new Point(xMargin, yPos),
					Size = new Size(inputW, 45),
					PlaceholderText = "VD: Sài Gòn, Hà Nội...",
					IconText = "🚉",
					BackColor = _clrBackground,
					ForeColor = _clrText
				};
				_txtDepStation.InputKeyDown += SearchField_KeyDown;
				pnlSearch.Controls.Add(_txtDepStation);
				yPos += 70;

				pnlSearch.Controls.Add(CreateLabel("Ga đến (tùy chọn)", 10, FontStyle.Regular, _clrTextGray, xMargin,
					yPos));
				yPos += 30;
				_txtArrStation = new ModernTextBox
				{
					Location = new Point(xMargin, yPos),
					Size = new Size(inputW, 45),
					PlaceholderText = "VD: Đà Nẵng, Nha Trang...",
					IconText = "🏁",
					BackColor = _clrBackground,
					ForeColor = _clrText
				};
				_txtArrStation.InputKeyDown += SearchField_KeyDown;
				pnlSearch.Controls.Add(_txtArrStation);
				yPos += 70;

				pnlSearch.Controls.Add(CreateLabel("Ngày đi (tùy chọn)", 10, FontStyle.Regular, _clrTextGray, xMargin,
					yPos));
				yPos += 30;
				_txtDate = new ModernTextBox
				{
					Location = new Point(xMargin, yPos),
					Size = new Size(inputW, 45),
					PlaceholderText = "VD: 24/12/2025",
					IconText = "📅",
					BackColor = _clrBackground,
					ForeColor = _clrText
				};
				_txtDate.InputKeyDown += SearchField_KeyDown;
				pnlSearch.Controls.Add(_txtDate);
				yPos += 80;

				RoundedButton btnSearch = new RoundedButton
				{
					Text = "🔍 TÌM KIẾM",
					Size = new Size(inputW, 50),
					Location = new Point(xMargin, yPos),
					BackColor = _clrAccent,
					ForeColor = Color.White,
					Font = new Font("Segoe UI", 11, FontStyle.Bold),
					Cursor = Cursors.Hand,
					FlatStyle = FlatStyle.Flat
				};
				btnSearch.FlatAppearance.BorderSize = 0;
				btnSearch.Click += BtnSearch_Click;
				pnlSearch.Controls.Add(btnSearch);
				yPos += 60;

				// Clear filters button
				_btnClearFilters = new RoundedButton
				{
					Text = "🗑️ Xóa bộ lọc",
					Size = new Size(inputW, 45),
					Location = new Point(xMargin, yPos),
					BackColor = Color.FromArgb(51, 65, 85),
					ForeColor = _clrTextGray,
					Font = new Font("Segoe UI", 10, FontStyle.Regular),
					Cursor = Cursors.Hand,
					FlatStyle = FlatStyle.Flat,
					Visible = false
				};
				_btnClearFilters.FlatAppearance.BorderSize = 0;
				_btnClearFilters.Click += BtnClearFilters_Click;
				pnlSearch.Controls.Add(_btnClearFilters);
				yPos += 55;

				// Filter count label
				_lblFilterCount = new Label
				{
					Text = "",
					Font = new Font("Segoe UI", 9, FontStyle.Regular),
					ForeColor = _clrTextGray,
					AutoSize = true,
					Location = new Point(xMargin, yPos),
					Visible = false
				};
				pnlSearch.Controls.Add(_lblFilterCount);

				// --- KẾT QUẢ TÌM KIẾM ---
				int contentX = 370;
				int contentW = this.Width - 400;
				_lblResultTitle = new Label
				{
					Text = "Tất cả chuyến tàu",
					Font = new Font("Segoe UI", 15, FontStyle.Bold),
					ForeColor = _clrText,
					AutoSize = true,
					Location = new Point(contentX, 80)
				};
				this.Controls.Add(_lblResultTitle);

				// Refresh button
				_btnRefresh = new RoundedButton
				{
					Text = "🔄 Làm mới",
					Size = new Size(130, 40),
					Location = new Point(this.Width - 180, 75),
					BackColor = _clrAccent,
					ForeColor = Color.White,
					Font = new Font("Segoe UI", 9, FontStyle.Bold),
					Cursor = Cursors.Hand,
					FlatStyle = FlatStyle.Flat,
					Anchor = AnchorStyles.Top | AnchorStyles.Right
				};
				_btnRefresh.FlatAppearance.BorderSize = 0;
				_btnRefresh.Click += BtnRefresh_Click;
				this.Controls.Add(_btnRefresh);

				Panel pnlTableHeader = new Panel
				{
					Size = new Size(contentW, 40),
					Location = new Point(contentX, 130),
					BackColor = Color.Transparent,
					Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
				};
				string[] headers =
				{
					"MÃ TÀU", "TÊN TÀU", "GA ĐI", "GA ĐẾN", "GIỜ ĐI", "GIỜ ĐẾN", "THỜI GIAN", "GIÁ VÉ",
					"TRẠNG THÁI", ""
				};
				int[] colWidths = { 90, 140, 110, 110, 90, 90, 130, 130, 110, 130 };
				int curX = 20;
				for (int i = 0; i < headers.Length; i++)
				{
					Label lblH = new Label
					{
						Text = headers[i],
						ForeColor = _clrTextGray,
						Font = new Font("Segoe UI", 9, FontStyle.Bold),
						AutoSize = false,
						Size = new Size(colWidths[i], 30),
						Location = new Point(curX, 5),
						TextAlign = ContentAlignment.MiddleLeft
					};
					pnlTableHeader.Controls.Add(lblH);
					curX += colWidths[i];
				}

				this.Controls.Add(pnlTableHeader);

				_flowResults = new FlowLayoutPanel
				{
					Location = new Point(contentX, 170),
					Size = new Size(contentW + 50, this.Height - 280),
					FlowDirection = FlowDirection.LeftToRight,
					WrapContents = true,
					AutoScroll = true,
					BackColor = Color.Transparent,
					Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
				};
				this.Controls.Add(_flowResults);

				// --- PAGINATION CONTROLS ---
				Panel pnlPagination = new Panel
				{
					Location = new Point(contentX, this.Height - 100),
					Size = new Size(contentW, 60),
					BackColor = Color.Transparent,
					Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
				};

				_btnPrevious = new RoundedButton
				{
					Text = "◀ Trang trước",
					Size = new Size(130, 40),
					Location = new Point(0, 10),
					BackColor = _clrAccent,
					ForeColor = Color.White,
					Font = new Font("Segoe UI", 9, FontStyle.Bold),
					Cursor = Cursors.Hand,
					FlatStyle = FlatStyle.Flat
				};
				_btnPrevious.FlatAppearance.BorderSize = 0;
				_btnPrevious.Click += BtnPrevious_Click;
				pnlPagination.Controls.Add(_btnPrevious);

				_lblPageInfo = new Label
				{
					Text = "Trang 1 / 1",
					Font = new Font("Segoe UI", 10, FontStyle.Regular),
					ForeColor = _clrTextGray,
					AutoSize = true,
					Location = new Point(150, 20),
					TextAlign = ContentAlignment.MiddleCenter
				};
				pnlPagination.Controls.Add(_lblPageInfo);

				_btnNext = new RoundedButton
				{
					Text = "Trang sau ▶",
					Size = new Size(130, 40),
					Location = new Point(300, 10),
					BackColor = _clrAccent,
					ForeColor = Color.White,
					Font = new Font("Segoe UI", 9, FontStyle.Bold),
					Cursor = Cursors.Hand,
					FlatStyle = FlatStyle.Flat
				};
				_btnNext.FlatAppearance.BorderSize = 0;
				_btnNext.Click += BtnNext_Click;
				pnlPagination.Controls.Add(_btnNext);

				this.Controls.Add(pnlPagination);

				// --- LOADING INDICATOR ---
				_pnlLoading = new Panel
				{
					Location = new Point(contentX, 170),
					Size = new Size(contentW + 50, this.Height - 280),
					BackColor = Color.FromArgb(200, 15, 23, 42),
					Visible = false,
					Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
				};

				Label lblLoading = new Label
				{
					Text = "⏳ Đang tải dữ liệu...",
					Font = new Font("Segoe UI", 14, FontStyle.Bold),
					ForeColor = _clrText,
					AutoSize = true
				};
				lblLoading.Location = new Point(
					(_pnlLoading.Width - lblLoading.Width) / 2,
					(_pnlLoading.Height - lblLoading.Height) / 2
				);
				_pnlLoading.Controls.Add(lblLoading);
				this.Controls.Add(_pnlLoading);
				//_pnlLoading.BringToFront();
			}
		}


		// =========================================================
		// 3. XỬ LÝ LOGIC CHỌN VÉ & MỞ BOOKING
		// =========================================================
		private void AddTrainItem(string code, string name, string depStation, string arrStation, string depTime,
			string arrTime, string duration, string price, string seatStatus, int statusType)
		{
			int w = _flowResults.ClientSize.Width - 30;
			if (w < 1130) w = 1130;
			Panel pnlItem = new Panel
			{
				Size = new Size(w, 80), Margin = new Padding(0, 0, 0, 15), BackColor = Color.Transparent
			};
			pnlItem.Paint += (_, e) =>
			{
				e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
				using GraphicsPath path = GetRoundedPath(new Rectangle(0, 0, w, 80), 12);
				using SolidBrush brush = new SolidBrush(_clrItemBg);
				e.Graphics.FillPath(brush, path);
			};

			int[] colWidths = { 90, 140, 110, 110, 90, 90, 130, 130, 110, 130 };
			int curX = 20;

			pnlItem.Controls.Add(CreateLabel(code, 11, FontStyle.Bold, _clrText, curX, 30));
			curX += colWidths[0];
			pnlItem.Controls.Add(CreateLabel(name, 10, FontStyle.Regular, _clrTextGray, curX, 30));
			curX += colWidths[1];
			pnlItem.Controls.Add(CreateLabel(depStation, 10, FontStyle.Regular, _clrText, curX, 30));
			curX += colWidths[2];
			pnlItem.Controls.Add(CreateLabel(arrStation, 10, FontStyle.Regular, _clrText, curX, 30));
			curX += colWidths[3];
			pnlItem.Controls.Add(CreateLabel(depTime, 11, FontStyle.Regular, _clrText, curX, 30));
			curX += colWidths[4];
			pnlItem.Controls.Add(CreateLabel(arrTime, 11, FontStyle.Regular, _clrTextGray, curX, 30));
			curX += colWidths[5];
			pnlItem.Controls.Add(CreateLabel(duration, 10, FontStyle.Regular, _clrTextGray, curX, 30));
			curX += colWidths[6];
			pnlItem.Controls.Add(CreateLabel(price, 11, FontStyle.Bold, _clrText, curX, 30));
			curX += colWidths[7];

			Label lblSeat = new Label
			{
				Text = seatStatus,
				ForeColor = statusType == 1 ? _clrGreen : (statusType == 2 ? _clrOrange : _clrRed),
				Font = new Font("Segoe UI", 9, FontStyle.Bold),
				AutoSize = true,
				Location = new Point(curX, 32)
			};
			pnlItem.Controls.Add(lblSeat);
			curX += colWidths[6];

			// --- PHẦN LOGIC NÚT BẤM ĐƯỢC CHỈNH SỬA TẠI ĐÂY ---
			if (statusType != 3) // Nếu còn vé
			{
				RoundedButton btnSelect = new RoundedButton
				{
					Text = "Chọn chuyến",
					Size = new Size(130, 40),
					Location = new Point(curX, 20),
					BackColor = _clrAccent,
					ForeColor = Color.White,
					Font = new Font("Segoe UI", 9, FontStyle.Bold),
					Cursor = Cursors.Hand,
					FlatStyle = FlatStyle.Flat
				};
				btnSelect.FlatAppearance.BorderSize = 0;

				// --- CẬP NHẬT TẠI ĐÂY: Truyền dữ liệu sang form Booking ---
				btnSelect.Click += (_, _) =>
				{
					// Truyền Mã tàu, Tên tàu, Giá vé vào Constructor mới
					var bookingForm = new client.Forms.Booking.Booking(code, name, price);
					bookingForm.ShowDialog();
				};
				// -----------------------------------------------------------

				pnlItem.Controls.Add(btnSelect);
			}
			else // Nếu hết vé
			{
				RoundedButton btnSoldOut = new RoundedButton
				{
					Text = "Hết vé",
					Size = new Size(130, 40),
					Location = new Point(curX, 20),
					BackColor = Color.FromArgb(51, 65, 85),
					ForeColor = Color.Gray,
					Font = new Font("Segoe UI", 9, FontStyle.Regular),
					Enabled = false,
					FlatStyle = FlatStyle.Flat
				};
				btnSoldOut.FlatAppearance.BorderSize = 0;
				pnlItem.Controls.Add(btnSoldOut);
			}
			// ------------------------------------------------

			_flowResults.Controls.Add(pnlItem);
		}

		private void AddWindowControls(Panel parent)
		{
			int btnW = 45;
			int startX = parent.Width - (btnW * 3) - 10;
			Label btnClose = CreateWindowButton("✕", startX + (btnW * 2));
			btnClose.Click += (_, _) => Application.Exit();
			btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			parent.Controls.Add(btnClose);
			Label btnMax = CreateWindowButton("☐", startX + btnW);
			btnMax.Click += (_, _) => ToggleMaximize();
			btnMax.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			parent.Controls.Add(btnMax);
			Label btnMin = CreateWindowButton("―", startX);
			btnMin.Click += (_, _) => this.WindowState = FormWindowState.Minimized;
			btnMin.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			parent.Controls.Add(btnMin);
		}

		private Label CreateWindowButton(string text, int x)
		{
			Label lbl = new Label
			{
				Text = text,
				Font = new Font("Segoe UI", 11, FontStyle.Regular),
				ForeColor = Color.White,
				AutoSize = false,
				Size = new Size(45, 30),
				Location = new Point(x, 15),
				TextAlign = ContentAlignment.MiddleCenter,
				Cursor = Cursors.Hand
			};
			lbl.MouseEnter += (_, _) => lbl.BackColor = (text == "✕") ? _clrRed : Color.FromArgb(51, 65, 85);
			lbl.MouseLeave += (_, _) => lbl.BackColor = Color.Transparent;
			return lbl;
		}

		private void HandleLogout(object? sender, EventArgs e)
		{
			var result = MessageBox.Show(
				"Bạn có chắc chắn muốn đăng xuất?",
				"Xác nhận đăng xuất",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question
			);

			if (result == DialogResult.Yes)
			{
				SessionManager.Instance.ClearSession();

				this.Hide();
				var loginForm = new Login();
				loginForm.FormClosed += (_, _) => this.Close();
				loginForm.Show();
			}
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

		private void SearchField_KeyDown(object? sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				e.SuppressKeyPress = true;
				BtnSearch_Click(sender, EventArgs.Empty);
			}
		}

		private async void BtnSearch_Click(object sender, EventArgs e)
		{
			string? depStation = string.IsNullOrWhiteSpace(_txtDepStation.TextValue)
				? null
				: _txtDepStation.TextValue.Trim();
			string? arrStation = string.IsNullOrWhiteSpace(_txtArrStation.TextValue)
				? null
				: _txtArrStation.TextValue.Trim();

			DateTime? depDate = null;
			if (!string.IsNullOrWhiteSpace(_txtDate.TextValue))
			{
				if (DateTime.TryParse(_txtDate.TextValue, out DateTime parsed))
				{
					depDate = parsed;
				}
				else
				{
					MessageBox.Show(
						"Định dạng ngày không hợp lệ. Vui lòng nhập theo định dạng: dd/MM/yyyy\nVí dụ: 24/12/2025",
						"Lỗi định dạng",
						MessageBoxButtons.OK,
						MessageBoxIcon.Warning
					);
					return;
				}
			}

			await LoadTrainsAsync(depStation, arrStation, depDate, 1);
			UpdateFilterUI();
		}

		private void BtnClearFilters_Click(object sender, EventArgs e)
		{
			_txtDepStation.Clear();
			_txtArrStation.Clear();
			_txtDate.Clear();

			_ = LoadTrainsAsync(null, null, null, 1);
			UpdateFilterUI();
		}

		private void UpdateFilterUI()
		{
			int filterCount = 0;
			if (!string.IsNullOrWhiteSpace(_txtDepStation.TextValue)) filterCount++;
			if (!string.IsNullOrWhiteSpace(_txtArrStation.TextValue)) filterCount++;
			if (!string.IsNullOrWhiteSpace(_txtDate.TextValue)) filterCount++;

			if (filterCount > 0)
			{
				_btnClearFilters.Visible = true;
				_lblFilterCount.Visible = true;
				_lblFilterCount.Text = $"✓ {filterCount} bộ lọc đang áp dụng";
				_lblFilterCount.ForeColor = _clrAccent;
			}
			else
			{
				_btnClearFilters.Visible = false;
				_lblFilterCount.Visible = false;
			}
		}

		private async void BtnPrevious_Click(object sender, EventArgs e)
		{
			if (_currentPage > 1)
			{
				await LoadTrainsAsync(_lastDepartureStation, _lastArrivalStation, _lastDepartureDate,
					_currentPage - 1);
			}
		}

		private async void BtnNext_Click(object sender, EventArgs e)
		{
			if (_currentPage < _totalPages)
			{
				await LoadTrainsAsync(_lastDepartureStation, _lastArrivalStation, _lastDepartureDate,
					_currentPage + 1);
			}
		}

		private async void BtnRefresh_Click(object sender, EventArgs e)
		{
			await LoadTrainsAsync(_lastDepartureStation, _lastArrivalStation, _lastDepartureDate, _currentPage);
		}

		private async Task LoadTrainsAsync(string? depStation, string? arrStation, DateTime? depDate, int pageNumber)
		{
			if (_isLoading) return;

			_isLoading = true;
			ShowLoadingIndicator();
			DisableControls();

			try
			{
				var apiClient = SessionManager.Instance.ApiClient;
				if (apiClient == null)
				{
					MessageBox.Show("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", "Lỗi",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				var trainService = new TrainService(apiClient);
				var response = await trainService.SearchTrainsAsync(depStation, arrStation, depDate, pageNumber,
					_pageSize);

				if (response == null)
				{
					MessageBox.Show("Không nhận được dữ liệu từ server.", "Lỗi", MessageBoxButtons.OK,
						MessageBoxIcon.Error);
					return;
				}

				var jsonString = JsonConvert.SerializeObject(response);
				var pagedResult = JsonConvert.DeserializeObject<PagedResult<Train>>(jsonString);

				if (pagedResult == null)
				{
					MessageBox.Show("Lỗi xử lý dữ liệu từ server.", "Lỗi", MessageBoxButtons.OK,
						MessageBoxIcon.Error);
					return;
				}

				_currentPage = pagedResult.PageNumber;
				_totalPages = pagedResult.TotalPages;
				_totalCount = pagedResult.TotalCount;

				_lastDepartureStation = depStation;
				_lastArrivalStation = arrStation;
				_lastDepartureDate = depDate;

				DisplayTrains(pagedResult.Items.ToList());
				UpdatePaginationControls();
				UpdateResultTitle();
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				_isLoading = false;
				HideLoadingIndicator();
				EnableControls();
			}
		}

		private void DisplayTrains(IList<Train> trains)
		{
			_flowResults.SuspendLayout();
			_flowResults.Controls.Clear();

			if (!trains.Any())
			{
				ShowNoResultsMessage();
				_flowResults.ResumeLayout();
				return;
			}

			foreach (var train in trains)
			{
				string code = train.TrainNumber;
				string name = train.TrainName;
				string depStation = train.DepartureStation;
				string arrStation = train.ArrivalStation;
				string depTime = train.DepartureTime.ToString("HH:mm");
				string arrTime = train.ArrivalTime.ToString("HH:mm");
				string duration = CalculateDuration(train.DepartureTime, train.ArrivalTime);
				string price = FormatPrice(train.TicketPrice);

				string seatStatus = "Còn vé";
				int statusType = 1;

				AddTrainItem(code, name, depStation, arrStation, depTime, arrTime, duration, price, seatStatus,
					statusType);
			}

			_flowResults.ResumeLayout();
		}

		private void ShowNoResultsMessage()
		{
			Label lblNoResults = new Label
			{
				Text = "🚂 Không tìm thấy chuyến tàu phù hợp\n\nThử thay đổi ga đi/đến hoặc ngày khởi hành",
				Font = new Font("Segoe UI", 12, FontStyle.Regular),
				ForeColor = _clrTextGray,
				AutoSize = true,
				TextAlign = ContentAlignment.MiddleCenter
			};
			lblNoResults.Location = new Point(
				(_flowResults.Width - lblNoResults.Width) / 2,
				100
			);
			_flowResults.Controls.Add(lblNoResults);
		}

		private void UpdatePaginationControls()
		{
			_lblPageInfo.Text = _totalPages > 0
				? $"Trang {_currentPage} / {_totalPages} (Tổng: {_totalCount} chuyến)"
				: "Không có dữ liệu";

			_btnPrevious.Enabled = _currentPage > 1;
			_btnNext.Enabled = _currentPage < _totalPages;

			_btnPrevious.BackColor = _btnPrevious.Enabled ? _clrAccent : Color.FromArgb(51, 65, 85);
			_btnNext.BackColor = _btnNext.Enabled ? _clrAccent : Color.FromArgb(51, 65, 85);
		}

		private void UpdateResultTitle()
		{
			if (!string.IsNullOrEmpty(_lastDepartureStation) || !string.IsNullOrEmpty(_lastArrivalStation) ||
			    _lastDepartureDate.HasValue)
			{
				string dep = _lastDepartureStation ?? "Tất cả ga";
				string arr = _lastArrivalStation ?? "Tất cả ga";
				string dateStr = _lastDepartureDate.HasValue
					? $" - {_lastDepartureDate.Value:dd/MM/yyyy}"
					: "";
				_lblResultTitle.Text = $"🔍 {dep} ➝ {arr}{dateStr}";
			}
			else
			{
				_lblResultTitle.Text = "📋 Tất cả chuyến tàu";
			}
		}

		private string CalculateDuration(DateTime departure, DateTime arrival)
		{
			TimeSpan duration = arrival - departure;
			int hours = (int)duration.TotalHours;
			int minutes = duration.Minutes;
			return $"{hours}h {minutes}m";
		}

		private string FormatPrice(decimal price)
		{
			return price.ToString("N0") + "đ";
		}

		private void ShowLoadingIndicator()
		{
			_pnlLoading.Visible = true;
			_pnlLoading.BringToFront();
		}

		private void HideLoadingIndicator()
		{
			_pnlLoading.Visible = false;
			_pnlLoading.SendToBack();
			_flowResults.BringToFront();
			_flowResults.Refresh();
		}

		private void DisableControls()
		{
			_btnPrevious.Enabled = false;
			_btnNext.Enabled = false;
			_btnRefresh.Enabled = false;
		}

		private void EnableControls()
		{
			_btnRefresh.Enabled = true;
			UpdatePaginationControls();
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