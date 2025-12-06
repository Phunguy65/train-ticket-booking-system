using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using client.Forms.Authentication;

namespace client.Forms.Booking
{
	public partial class Booking : Form
	{
		// ... (Giữ nguyên các màu sắc) ...
		private readonly Color ClrBackground = Color.FromArgb(15, 23, 42);
		private readonly Color ClrSidebar = Color.FromArgb(20, 30, 50);
		private readonly Color ClrPanelRight = Color.FromArgb(15, 23, 42);
		private readonly Color ClrText = Color.White;
		private readonly Color ClrTextGray = Color.FromArgb(148, 163, 184);
		private readonly Color ClrAccent = Color.FromArgb(37, 99, 235);
		private readonly Color ClrSeatEmpty = Color.FromArgb(30, 41, 59);
		private readonly Color ClrSeatSold = Color.FromArgb(51, 65, 85);

		// Biến logic
		private FlowLayoutPanel flowSeats;
		private Label lblTotalPrice;
		private Label lblSelectedList;
		private List<string> selectedSeats = new List<string>();

		// CÁC BIẾN ĐỂ LƯU THÔNG TIN TÀU ĐƯỢC TRUYỀN SANG
		private string _trainCode;
		private string _trainName;
		private long _ticketPrice;

		// --- SỬA CONSTRUCTOR ĐỂ NHẬN DỮ LIỆU ---
		public Booking(string code, string name, string priceStr)
		{
			InitializeComponent();

			// Lưu thông tin được truyền từ MainForm
			_trainCode = code;
			_trainName = name;
			_ticketPrice = ParsePrice(priceStr); // Chuyển đổi chuỗi "950.000đ" thành số

			SetupUI();
			GenerateSeats();
		}

		// Hàm hỗ trợ chuyển đổi giá tiền (VD: "950.000đ" -> 950000)
		private long ParsePrice(string priceStr)
		{
			string cleanStr = priceStr.Replace(".", "").Replace(",", "").Replace("đ", "").Trim();
			if (long.TryParse(cleanStr, out long result)) return result;
			return 250000; // Giá mặc định nếu lỗi
		}

		private void SetupUI()
		{
			// ... (Cấu hình Form giữ nguyên) ...
			this.FormBorderStyle = FormBorderStyle.None;
			this.StartPosition = FormStartPosition.CenterScreen;
			this.Size = new Size(1200, 750);
			this.BackColor = ClrBackground;
			this.DoubleBuffered = true;

			// HEADER (Giữ nguyên)
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
			btnClose.Click += (s, e) => this.Close();
			pnlHeader.Controls.Add(lblLogo);
			pnlHeader.Controls.Add(btnClose);
			this.Controls.Add(pnlHeader);

			// SPLIT LAYOUT (Giữ nguyên)
			Panel pnlLeft = new Panel
			{
				Location = new Point(30, 80), Size = new Size(750, 620), BackColor = ClrSidebar
			};
			pnlLeft.Paint += (s, e) => DrawRoundedBorder(s, e, pnlLeft.Width, pnlLeft.Height, 15);
			this.Controls.Add(pnlLeft);

			Panel pnlRight = new Panel
			{
				Location = new Point(810, 80), Size = new Size(360, 620), BackColor = ClrPanelRight
			};
			this.Controls.Add(pnlRight);

			// CỘT TRÁI (Giữ nguyên)
			Label lblBack = new Label
			{
				Text = "← Sơ đồ ghế ngồi",
				Font = new Font("Segoe UI", 14, FontStyle.Bold),
				ForeColor = ClrText,
				AutoSize = true,
				Location = new Point(30, 20),
				Cursor = Cursors.Hand
			};
			lblBack.Click += (s, e) => this.Close();
			pnlLeft.Controls.Add(lblBack);
			pnlLeft.Controls.Add(new Label
			{
				Text = $"Chọn ghế cho tàu {_trainCode}",
				Font = new Font("Segoe UI", 10, FontStyle.Regular),
				ForeColor = ClrTextGray,
				AutoSize = true,
				Location = new Point(30, 50)
			});

			flowSeats = new FlowLayoutPanel
			{
				Location = new Point(50, 120),
				Size = new Size(650, 400),
				FlowDirection = FlowDirection.LeftToRight,
				BackColor = Color.Transparent,
				Padding = new Padding(35, 20, 0, 0)
			};
			pnlLeft.Controls.Add(flowSeats);

			// === CỘT PHẢI (CẬP NHẬT THÔNG TIN ĐỘNG) ===
			Panel pnlTripInfo = new Panel
			{
				Size = new Size(360, 120), Location = new Point(0, 0), BackColor = Color.Transparent
			};
			pnlTripInfo.Controls.Add(new Label
			{
				Text = "Thông tin chuyến đi",
				Font = new Font("Segoe UI", 12, FontStyle.Bold),
				ForeColor = ClrText,
				Location = new Point(0, 0),
				AutoSize = true
			});

			Panel pnlTrainCard = new Panel
			{
				Size = new Size(360, 70), Location = new Point(0, 40), BackColor = Color.Transparent
			};
			pnlTrainCard.Paint += (s, e) =>
			{
				try
				{
					using (GraphicsPath path = RoundedButton.GetRoundedPath(new Rectangle(0, 0, 359, 69), 10))
					using (Pen pen = new Pen(Color.FromArgb(51, 65, 85)))
					using (SolidBrush br = new SolidBrush(Color.FromArgb(20, 30, 40)))
					{
						e.Graphics.FillPath(br, path);
						e.Graphics.DrawPath(pen, path);
					}
				}
				catch { }
			};

			// HIỂN THỊ DỮ LIỆU ĐƯỢC TRUYỀN VÀO
			pnlTrainCard.Controls.Add(new Label
			{
				Text = "Tàu", ForeColor = ClrTextGray, Location = new Point(20, 15), AutoSize = true
			});
			pnlTrainCard.Controls.Add(new Label
			{
				Text = $"{_trainCode} - {_trainName}",
				ForeColor = ClrText,
				Font = new Font("Segoe UI", 10, FontStyle.Bold),
				Location = new Point(200, 15),
				AutoSize = true
			}); // Hiển thị mã tàu thật

			pnlTrainCard.Controls.Add(new Label
			{
				Text = "Giá vé", ForeColor = ClrTextGray, Location = new Point(20, 40), AutoSize = true
			});
			pnlTrainCard.Controls.Add(new Label
			{
				Text = string.Format("{0:N0}đ", _ticketPrice),
				ForeColor = ClrText,
				Font = new Font("Segoe UI", 10, FontStyle.Bold),
				Location = new Point(200, 40),
				AutoSize = true
			}); // Hiển thị giá thật

			pnlTripInfo.Controls.Add(pnlTrainCard);
			pnlRight.Controls.Add(pnlTripInfo);

			// (Các phần Chú thích, Tổng cộng giữ nguyên)
			int legendY = 140;
			pnlRight.Controls.Add(new Label
			{
				Text = "Chú thích",
				Font = new Font("Segoe UI", 10, FontStyle.Bold),
				ForeColor = ClrText,
				Location = new Point(0, legendY),
				AutoSize = true
			});
			CreateLegendItem(pnlRight, "Trống", ClrSeatEmpty, legendY + 30);
			CreateLegendItem(pnlRight, "Đang chọn", ClrAccent, legendY + 60);
			CreateLegendItem(pnlRight, "Đã bán", ClrSeatSold, legendY + 90);

			int seatListY = legendY + 140;
			pnlRight.Controls.Add(new Label
			{
				Text = "Ghế đang chọn",
				Font = new Font("Segoe UI", 10, FontStyle.Bold),
				ForeColor = ClrText,
				Location = new Point(0, seatListY),
				AutoSize = true
			});
			lblSelectedList = new Label
			{
				Text = "---",
				Font = new Font("Segoe UI", 10, FontStyle.Bold),
				ForeColor = ClrAccent,
				AutoSize = false,
				Size = new Size(360, 25),
				Location = new Point(0, seatListY + 30),
				TextAlign = ContentAlignment.MiddleRight
			};
			pnlRight.Controls.Add(lblSelectedList);

			int footerY = 500;
			pnlRight.Controls.Add(new Label
			{
				Text = "Tổng cộng", ForeColor = ClrTextGray, Location = new Point(0, footerY), AutoSize = true
			});
			lblTotalPrice = new Label
			{
				Text = "0 VNĐ",
				ForeColor = ClrText,
				Font = new Font("Segoe UI", 16, FontStyle.Bold),
				Location = new Point(150, footerY - 5),
				Size = new Size(210, 40),
				TextAlign = ContentAlignment.MiddleRight
			};
			pnlRight.Controls.Add(lblTotalPrice);

			RoundedButton btnConfirm = new RoundedButton
			{
				Text = "Xác nhận đặt vé",
				BackColor = ClrAccent,
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

		private void GenerateSeats()
		{
			string[] seatNames = { "1A", "1B", "1C", "1D", "1E", "2A", "2B", "2C", "2D", "2E" };
			Random rnd = new Random();
			foreach (var seatName in seatNames)
			{
				bool isSold = rnd.Next(0, 10) > 8;
				RoundedButton btnSeat = new RoundedButton
				{
					Text = seatName + "\n🛋️",
					Size = new Size(90, 90),
					Margin = new Padding(15),
					Font = new Font("Segoe UI", 11, FontStyle.Bold),
					FlatStyle = FlatStyle.Flat,
					Cursor = isSold ? Cursors.No : Cursors.Hand,
					Tag = isSold ? "SOLD" : "EMPTY"
				};
				btnSeat.FlatAppearance.BorderSize = 0;
				if (isSold)
				{
					btnSeat.BackColor = ClrSeatSold;
					btnSeat.ForeColor = Color.FromArgb(100, 116, 139);
				}
				else
				{
					btnSeat.BackColor = ClrSeatEmpty;
					btnSeat.ForeColor = ClrText;
					btnSeat.Click += Seat_Click;
				}

				flowSeats.Controls.Add(btnSeat);
			}
		}

		private void Seat_Click(object sender, EventArgs e)
		{
			RoundedButton btn = sender as RoundedButton;
			string seatName = btn.Text.Replace("\n🛋️", "");
			if (btn.Tag.ToString() == "EMPTY")
			{
				btn.BackColor = ClrAccent;
				btn.Tag = "SELECTED";
				selectedSeats.Add(seatName);
			}
			else
			{
				btn.BackColor = ClrSeatEmpty;
				btn.Tag = "EMPTY";
				selectedSeats.Remove(seatName);
			}

			UpdateSummary();
		}

		private void UpdateSummary()
		{
			if (selectedSeats.Count > 0)
			{
				selectedSeats.Sort();
				lblSelectedList.Text = string.Join(", ", selectedSeats);
			}
			else { lblSelectedList.Text = "---"; }

			// TÍNH TIỀN DỰA TRÊN GIÁ VÉ THỰC TẾ (_ticketPrice)
			long total = selectedSeats.Count * _ticketPrice;
			lblTotalPrice.Text = string.Format("{0:N0} VNĐ", total);
		}

		// ... (Giữ nguyên các hàm vẽ UI phụ trợ) ...
		private void CreateLegendItem(Panel parent, string text, Color color, int y)
		{
			Panel pnlColor = new Panel { Size = new Size(20, 20), Location = new Point(0, y), BackColor = color };
			pnlColor.Paint += (s, e) =>
			{
				try
				{
					using (GraphicsPath path = RoundedButton.GetRoundedPath(new Rectangle(0, 0, 20, 20), 5))
					using (SolidBrush br = new SolidBrush(color)) { e.Graphics.FillPath(br, path); }
				}
				catch { }
			};
			Label lblText = new Label
			{
				Text = text, ForeColor = ClrTextGray, Location = new Point(30, y), AutoSize = true
			};
			parent.Controls.Add(pnlColor);
			parent.Controls.Add(lblText);
		}

		private void BtnConfirm_Click(object sender, EventArgs e)
		{
			if (selectedSeats.Count == 0)
			{
				MessageBox.Show("Vui lòng chọn ít nhất 1 ghế!", "Thông báo", MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				return;
			}

			MessageBox.Show(
				$"Đặt vé thành công cho tàu {_trainCode}!\nGhế: {lblSelectedList.Text}\nTổng tiền: {lblTotalPrice.Text}",
				"Thành công");
		}

		private void DrawRoundedBorder(object sender, PaintEventArgs e, int w, int h, int r)
		{
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			try
			{
				using (GraphicsPath path = RoundedButton.GetRoundedPath(new Rectangle(0, 0, w, h), r))
				using (SolidBrush brush = new SolidBrush(((Control)sender).BackColor))
				{
					e.Graphics.FillPath(brush, path);
				}
			}
			catch { }
		}
	}
}