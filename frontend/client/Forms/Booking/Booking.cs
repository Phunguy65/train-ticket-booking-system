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

namespace client.Forms.Booking
{
	public partial class Booking : Form
	{
		private readonly Color _clrBackground = Color.FromArgb(15, 23, 42);
		private readonly Color _clrSidebar = Color.FromArgb(20, 30, 50);
		private readonly Color _clrPanelRight = Color.FromArgb(15, 23, 42);
		private readonly Color _clrText = Color.White;
		private readonly Color _clrTextGray = Color.FromArgb(148, 163, 184);
		private readonly Color _clrAccent = Color.FromArgb(37, 99, 235);
		private readonly Color _clrSeatEmpty = Color.FromArgb(30, 41, 59);
		private readonly Color _clrSeatSold = Color.FromArgb(51, 65, 85);

		private FlowLayoutPanel _flowSeats;
		private Label _lblTotalPrice;
		private Label _lblSelectedList;
		private Panel _pnlLoading;

		private readonly Train _train;
		private readonly BookingService _bookingService;

		private List<Seat> _allSeats = new List<Seat>();
		private Dictionary<int, string> _selectedSeatInfo = new Dictionary<int, string>();
		private int _currentPage = 1;
		private readonly int _pageSize = 10;
		private int _totalPages;
		private bool _isLoadingSeats;

		private RoundedButton _btnPreviousPage;
		private RoundedButton _btnNextPage;
		private Label _lblSeatPageInfo;

		public Booking(Train train)
		{
			InitializeComponent();

			_train = train ?? throw new ArgumentNullException(nameof(train));

			var apiClient = SessionManager.Instance.ApiClient;
			if (apiClient == null)
			{
				MessageBox.Show("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", "Lỗi",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				this.Close();
				return;
			}

			_bookingService = new BookingService(apiClient);

			SetupUi();
			this.Load += async (_, _) => await LoadSeatsAsync();
		}

		private void SetupUi()
		{
			this.FormBorderStyle = FormBorderStyle.None;
			this.StartPosition = FormStartPosition.CenterScreen;
			this.Size = new Size(1200, 750);
			this.BackColor = _clrBackground;
			this.DoubleBuffered = true;

			Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(20, 0, 20, 0) };
			Label lblLogo = new Label
			{
				Text = "🚆 Vé Tàu Cao Tốc",
				Font = new Font("Segoe UI", 12, FontStyle.Bold),
				ForeColor = Color.White,
				AutoSize = true,
				Location = new Point(20, 20)
			};
			Label btnClose = new Label
			{
				Text = "✕",
				ForeColor = Color.White,
				Font = new Font("Arial", 14, FontStyle.Bold),
				Location = new Point(this.Width - 40, 15),
				AutoSize = true,
				Cursor = Cursors.Hand,
				Anchor = AnchorStyles.Top | AnchorStyles.Right
			};
			btnClose.Click += (_, _) => this.Close();
			pnlHeader.Controls.Add(lblLogo);
			pnlHeader.Controls.Add(btnClose);
			this.Controls.Add(pnlHeader);

			Panel pnlLeft = new Panel
			{
				Location = new Point(30, 80), Size = new Size(750, 620), BackColor = _clrSidebar
			};
			pnlLeft.Paint += (s, e) => DrawRoundedBorder(s, e, pnlLeft.Width, pnlLeft.Height, 15);
			this.Controls.Add(pnlLeft);

			Panel pnlRight = new Panel
			{
				Location = new Point(810, 80), Size = new Size(360, 620), BackColor = _clrPanelRight
			};
			this.Controls.Add(pnlRight);

			Label lblBack = new Label
			{
				Text = "← Sơ đồ ghế ngồi",
				Font = new Font("Segoe UI", 14, FontStyle.Bold),
				ForeColor = _clrText,
				AutoSize = true,
				Location = new Point(30, 20),
				Cursor = Cursors.Hand
			};
			lblBack.Click += (_, _) => this.Close();
			pnlLeft.Controls.Add(lblBack);
			pnlLeft.Controls.Add(new Label
			{
				Text = $"Chọn ghế cho tàu {_train.TrainNumber}",
				Font = new Font("Segoe UI", 10, FontStyle.Regular),
				ForeColor = _clrTextGray,
				AutoSize = true,
				Location = new Point(30, 50)
			});

			_flowSeats = new FlowLayoutPanel
			{
				Location = new Point(50, 120),
				Size = new Size(650, 350),
				FlowDirection = FlowDirection.LeftToRight,
				BackColor = Color.Transparent,
				Padding = new Padding(35, 20, 0, 0)
			};
			pnlLeft.Controls.Add(_flowSeats);

			Panel pnlSeatPagination = new Panel
			{
				Location = new Point(50, 480), Size = new Size(650, 50), BackColor = Color.Transparent
			};

			_btnPreviousPage = new RoundedButton
			{
				Text = "◀ Trang trước",
				Size = new Size(120, 35),
				Location = new Point(150, 10),
				BackColor = _clrAccent,
				ForeColor = Color.White,
				Font = new Font("Segoe UI", 9, FontStyle.Bold),
				Cursor = Cursors.Hand,
				FlatStyle = FlatStyle.Flat
			};
			_btnPreviousPage.FlatAppearance.BorderSize = 0;
			_btnPreviousPage.Click += BtnPreviousPage_Click;
			pnlSeatPagination.Controls.Add(_btnPreviousPage);

			_lblSeatPageInfo = new Label
			{
				Text = "Trang 1 / 1",
				Font = new Font("Segoe UI", 10, FontStyle.Regular),
				ForeColor = _clrTextGray,
				AutoSize = true,
				Location = new Point(290, 18),
				TextAlign = ContentAlignment.MiddleCenter
			};
			pnlSeatPagination.Controls.Add(_lblSeatPageInfo);

			_btnNextPage = new RoundedButton
			{
				Text = "Trang sau ▶",
				Size = new Size(120, 35),
				Location = new Point(380, 10),
				BackColor = _clrAccent,
				ForeColor = Color.White,
				Font = new Font("Segoe UI", 9, FontStyle.Bold),
				Cursor = Cursors.Hand,
				FlatStyle = FlatStyle.Flat
			};
			_btnNextPage.FlatAppearance.BorderSize = 0;
			_btnNextPage.Click += BtnNextPage_Click;
			pnlSeatPagination.Controls.Add(_btnNextPage);

			pnlLeft.Controls.Add(pnlSeatPagination);

			_pnlLoading = new Panel
			{
				Location = new Point(50, 120),
				Size = new Size(650, 350),
				BackColor = Color.FromArgb(200, 15, 23, 42),
				Visible = false
			};
			Label lblLoading = new Label
			{
				Text = "Đang tải dữ liệu ghế...",
				Font = new Font("Segoe UI", 12, FontStyle.Bold),
				ForeColor = _clrText,
				AutoSize = true,
				Location = new Point(250, 160)
			};
			_pnlLoading.Controls.Add(lblLoading);
			pnlLeft.Controls.Add(_pnlLoading);

			Panel pnlTripInfo = new Panel
			{
				Size = new Size(360, 280), Location = new Point(0, 0), BackColor = Color.Transparent
			};
			pnlTripInfo.Controls.Add(new Label
			{
				Text = "Thông tin chuyến đi",
				Font = new Font("Segoe UI", 12, FontStyle.Bold),
				ForeColor = _clrText,
				Location = new Point(0, 0),
				AutoSize = true
			});

			Panel pnlTrainCard = new Panel
			{
				Size = new Size(360, 230), Location = new Point(0, 40), BackColor = Color.Transparent
			};
			pnlTrainCard.Paint += (_, e) =>
			{
				using GraphicsPath path = RoundedButton.GetRoundedPath(new Rectangle(0, 0, 359, 229), 10);
				using Pen pen = new Pen(Color.FromArgb(51, 65, 85));
				using SolidBrush br = new SolidBrush(Color.FromArgb(20, 30, 40));
				e.Graphics.FillPath(br, path);
				e.Graphics.DrawPath(pen, path);
			};

			int yPos = 15;
			AddTrainInfoRow(pnlTrainCard, "Tàu", $"{_train.TrainNumber} - {_train.TrainName}", yPos);
			yPos += 30;
			AddTrainInfoRow(pnlTrainCard, "Tuyến", $"{_train.DepartureStation} → {_train.ArrivalStation}", yPos);
			yPos += 30;
			AddTrainInfoRow(pnlTrainCard, "Khởi hành", _train.DepartureTime.ToString("dd/MM/yyyy HH:mm"), yPos);
			yPos += 30;
			AddTrainInfoRow(pnlTrainCard, "Đến nơi", _train.ArrivalTime.ToString("dd/MM/yyyy HH:mm"), yPos);
			yPos += 30;
			AddTrainInfoRow(pnlTrainCard, "Thời gian", CalculateDuration(_train.DepartureTime, _train.ArrivalTime),
				yPos);
			yPos += 30;
			AddTrainInfoRow(pnlTrainCard, "Giá vé", $"{_train.TicketPrice:N0}đ", yPos);
			yPos += 30;
			AddTrainInfoRow(pnlTrainCard, "Tổng ghế", _train.TotalSeats.ToString(), yPos);

			pnlTripInfo.Controls.Add(pnlTrainCard);
			pnlRight.Controls.Add(pnlTripInfo);

			int legendY = 290;
			pnlRight.Controls.Add(new Label
			{
				Text = "Chú thích",
				Font = new Font("Segoe UI", 10, FontStyle.Bold),
				ForeColor = _clrText,
				Location = new Point(0, legendY),
				AutoSize = true
			});
			CreateLegendItem(pnlRight, "Trống", _clrSeatEmpty, legendY + 30);
			CreateLegendItem(pnlRight, "Đang chọn", _clrAccent, legendY + 60);
			CreateLegendItem(pnlRight, "Đã bán", _clrSeatSold, legendY + 90);

			int seatListY = legendY + 140;
			pnlRight.Controls.Add(new Label
			{
				Text = "Ghế đang chọn",
				Font = new Font("Segoe UI", 10, FontStyle.Bold),
				ForeColor = _clrText,
				Location = new Point(0, seatListY),
				AutoSize = true
			});
			_lblSelectedList = new Label
			{
				Text = "---",
				Font = new Font("Segoe UI", 10, FontStyle.Bold),
				ForeColor = _clrAccent,
				AutoSize = false,
				Size = new Size(360, 25),
				Location = new Point(0, seatListY + 30),
				TextAlign = ContentAlignment.MiddleRight
			};
			pnlRight.Controls.Add(_lblSelectedList);

			int footerY = 500;
			pnlRight.Controls.Add(new Label
			{
				Text = "Tổng cộng", ForeColor = _clrTextGray, Location = new Point(0, footerY), AutoSize = true
			});
			_lblTotalPrice = new Label
			{
				Text = "0 VNĐ",
				ForeColor = _clrText,
				Font = new Font("Segoe UI", 16, FontStyle.Bold),
				Location = new Point(150, footerY - 5),
				Size = new Size(210, 40),
				TextAlign = ContentAlignment.MiddleRight
			};
			pnlRight.Controls.Add(_lblTotalPrice);

			RoundedButton btnConfirm = new RoundedButton
			{
				Text = "Xác nhận đặt vé",
				BackColor = _clrAccent,
				ForeColor = Color.White,
				Size = new Size(360, 50),
				Location = new Point(0, footerY + 50),
				Font = new Font("Segoe UI", 11, FontStyle.Bold),
				Cursor = Cursors.Hand,
				FlatStyle = FlatStyle.Flat
			};
			btnConfirm.FlatAppearance.BorderSize = 0;
			btnConfirm.Click += BtnConfirm_Click;
			pnlRight.Controls.Add(btnConfirm);
		}

		private async Task LoadSeatsAsync()
		{
			if (_isLoadingSeats) return;

			_isLoadingSeats = true;
			ShowLoadingIndicator();

			try
			{
				var seatMapData = await _bookingService.GetSeatMapAsync(_train.TrainId);
				var jsonString = JsonConvert.SerializeObject(seatMapData);
				var seats = JsonConvert.DeserializeObject<List<Seat>>(jsonString);

				if (seats == null || seats.Count == 0)
				{
					MessageBox.Show("Không có dữ liệu ghế cho chuyến tàu này.", "Thông báo",
						MessageBoxButtons.OK, MessageBoxIcon.Information);
					return;
				}

				_allSeats = seats;
				_totalPages = (int)Math.Ceiling(_allSeats.Count / (double)_pageSize);
				_currentPage = 1;

				DisplayCurrentPage();
				UpdateSeatPagination();
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Lỗi tải dữ liệu ghế: {ex.Message}", "Lỗi",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				_isLoadingSeats = false;
				HideLoadingIndicator();
			}
		}

		private void DisplayCurrentPage()
		{
			int startIndex = (_currentPage - 1) * _pageSize;
			var pageSeats = _allSeats.Skip(startIndex).Take(_pageSize).ToList();
			DisplaySeats(pageSeats);
		}

		private void DisplaySeats(List<Seat> seats)
		{
			_flowSeats.Controls.Clear();

			foreach (var seat in seats)
			{
				bool isSelected = _selectedSeatInfo.ContainsKey(seat.SeatId);
				bool isSold = !seat.IsAvailable;

				RoundedButton btnSeat = new RoundedButton
				{
					Text = seat.SeatNumber + "\n🛋️",
					Size = new Size(90, 90),
					Margin = new Padding(15),
					Font = new Font("Segoe UI", 11, FontStyle.Bold),
					FlatStyle = FlatStyle.Flat,
					Cursor = isSold ? Cursors.No : Cursors.Hand,
					Tag = seat.SeatId
				};
				btnSeat.FlatAppearance.BorderSize = 0;

				if (isSold)
				{
					btnSeat.BackColor = _clrSeatSold;
					btnSeat.ForeColor = Color.FromArgb(100, 116, 139);
				}
				else if (isSelected)
				{
					btnSeat.BackColor = _clrAccent;
					btnSeat.ForeColor = _clrText;
					btnSeat.Click += Seat_Click;
				}
				else
				{
					btnSeat.BackColor = _clrSeatEmpty;
					btnSeat.ForeColor = _clrText;
					btnSeat.Click += Seat_Click;
				}

				_flowSeats.Controls.Add(btnSeat);
			}
		}

		private void Seat_Click(object? sender, EventArgs? e)
		{
			if (sender is not RoundedButton btn || btn.Tag == null)
				return;

			int seatId = (int)btn.Tag;
			string seatName = btn.Text.Replace("\n🛋️", "");

			if (_selectedSeatInfo.ContainsKey(seatId))
			{
				btn.BackColor = _clrSeatEmpty;
				_selectedSeatInfo.Remove(seatId);
			}
			else
			{
				btn.BackColor = _clrAccent;
				_selectedSeatInfo[seatId] = seatName;
			}

			UpdateSummary();
		}

		private void UpdateSummary()
		{
			if (_selectedSeatInfo.Count > 0)
			{
				var sortedSeats = _selectedSeatInfo.Values.OrderBy(s => s).ToList();
				_lblSelectedList.Text = string.Join(", ", sortedSeats);
			}
			else
			{
				_lblSelectedList.Text = "---";
			}

			decimal total = _selectedSeatInfo.Count * _train.TicketPrice;
			_lblTotalPrice.Text = $"{total:N0} VNĐ";
		}

		private void BtnPreviousPage_Click(object sender, EventArgs e)
		{
			if (_currentPage > 1)
			{
				_currentPage--;
				DisplayCurrentPage();
				UpdateSeatPagination();
			}
		}

		private void BtnNextPage_Click(object sender, EventArgs e)
		{
			if (_currentPage < _totalPages)
			{
				_currentPage++;
				DisplayCurrentPage();
				UpdateSeatPagination();
			}
		}

		private void UpdateSeatPagination()
		{
			_lblSeatPageInfo.Text = _totalPages > 0
				? $"Trang {_currentPage} / {_totalPages}"
				: "Không có dữ liệu";

			_btnPreviousPage.Enabled = _currentPage > 1;
			_btnNextPage.Enabled = _currentPage < _totalPages;

			_btnPreviousPage.BackColor = _btnPreviousPage.Enabled ? _clrAccent : Color.FromArgb(51, 65, 85);
			_btnNextPage.BackColor = _btnNextPage.Enabled ? _clrAccent : Color.FromArgb(51, 65, 85);
		}

		private void ShowLoadingIndicator()
		{
			_pnlLoading.Visible = true;
			_pnlLoading.BringToFront();
		}

		private void HideLoadingIndicator()
		{
			_pnlLoading.Visible = false;
		}

		private void AddTrainInfoRow(Panel parent, string label, string value, int yPos)
		{
			parent.Controls.Add(new Label
			{
				Text = label,
				ForeColor = _clrTextGray,
				Location = new Point(20, yPos),
				AutoSize = true,
				Font = new Font("Segoe UI", 9, FontStyle.Regular)
			});
			parent.Controls.Add(new Label
			{
				Text = value,
				ForeColor = _clrText,
				Font = new Font("Segoe UI", 9, FontStyle.Bold),
				Location = new Point(120, yPos),
				AutoSize = true
			});
		}

		private string CalculateDuration(DateTime departure, DateTime arrival)
		{
			TimeSpan duration = arrival - departure;
			int hours = (int)duration.TotalHours;
			int minutes = duration.Minutes;
			return $"{hours}h {minutes}m";
		}

		private void CreateLegendItem(Panel parent, string text, Color color, int y)
		{
			Panel pnlColor = new Panel { Size = new Size(20, 20), Location = new Point(0, y), BackColor = color };
			pnlColor.Paint += (_, e) =>
			{
				using GraphicsPath path = RoundedButton.GetRoundedPath(new Rectangle(0, 0, 20, 20), 5);
				using SolidBrush br = new SolidBrush(color);
				e.Graphics.FillPath(br, path);
			};
			Label lblText = new Label
			{
				Text = text, ForeColor = _clrTextGray, Location = new Point(30, y), AutoSize = true
			};
			parent.Controls.Add(pnlColor);
			parent.Controls.Add(lblText);
		}

		private async void BtnConfirm_Click(object? sender, EventArgs? e)
		{
			if (_selectedSeatInfo.Count == 0)
			{
				MessageBox.Show("Vui lòng chọn ít nhất 1 ghế!", "Thông báo",
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			try
			{
				var seatIds = _selectedSeatInfo.Keys.ToList();
				var response = await _bookingService.BookMultipleTicketsAsync(_train.TrainId, seatIds);

				if (response.Success)
				{
					MessageBox.Show(
						$"Đặt vé thành công cho tàu {_train.TrainNumber}!\n" +
						$"Ghế: {_lblSelectedList.Text}\n" +
						$"Tổng tiền: {_lblTotalPrice.Text}",
						"Thành công",
						MessageBoxButtons.OK,
						MessageBoxIcon.Information);

					this.DialogResult = DialogResult.OK;
					this.Close();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Lỗi đặt vé: {ex.Message}", "Lỗi",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void DrawRoundedBorder(object? sender, PaintEventArgs e, int w, int h, int r)
		{
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

			using GraphicsPath path = RoundedButton.GetRoundedPath(new Rectangle(0, 0, w, h), r);
			using SolidBrush brush = new SolidBrush((sender as Control)?.BackColor ?? Color.Transparent);
			e.Graphics.FillPath(brush, path);
		}
	}
}