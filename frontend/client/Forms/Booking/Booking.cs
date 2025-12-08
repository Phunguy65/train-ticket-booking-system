using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using client.Controls; // Sử dụng Custom Controls
using client.Helpers;  // Sử dụng Bảng màu chung

namespace client.Forms.Booking
{
	public partial class Booking : Form
	{
		// =========================================================
		// 1. CẤU HÌNH MÀU SẮC RIÊNG
		// =========================================================
		private readonly Color ClrSeatEmpty = Color.FromArgb(30, 41, 59);
		private readonly Color ClrSeatSold = Color.FromArgb(51, 65, 85);
		private readonly Color ClrSeatBorder = Color.FromArgb(94, 161, 224);

		// =========================================================
		// 2. BIẾN DỮ LIỆU & CONTROL
		// =========================================================
		private FlowLayoutPanel flowSeats;
		private Label lblTotalPrice;
		private Label lblSelectedList;
		private List<string> selectedSeats = new List<string>();

		private string _trainCode;
		private string _trainName;
		private long _ticketPrice;

		// =========================================================
		// 3. CONSTRUCTOR
		// =========================================================
		public Booking(string code, string name, string priceStr)
		{
			InitializeComponent();
			_trainCode = code;
			_trainName = name;
			_ticketPrice = ParsePrice(priceStr);

			SetupForm();
			SetupHeader();
			SetupSplitLayout();
			GenerateSeats();
		}

		private long ParsePrice(string priceStr)
		{
			if (string.IsNullOrEmpty(priceStr)) return 0;
			string cleanStr = priceStr.Replace(".", "").Replace(",", "").Replace("đ", "").Trim();
			if (long.TryParse(cleanStr, out long result)) return result;
			return 0;
		}

		// =========================================================
		// 4. SETUP GIAO DIỆN
		// =========================================================
		private void SetupForm()
		{
			this.FormBorderStyle = FormBorderStyle.None;
			this.StartPosition = FormStartPosition.CenterScreen;
			this.Size = new Size(1200, 750);

			this.BackColor = AppColors.CardBg;

			this.DoubleBuffered = true;
		}

		private void SetupHeader()
		{
			Panel pnlHeader = new Panel
			{
				Dock = DockStyle.Top,
				Height = 60,
				Padding = new Padding(20, 0, 20, 0),
				BackColor = AppColors.CardBg
			};

			Label lblLogo = new Label
			{
				Text = "🚆 Vé Tàu Cao Tốc",
				Font = new Font("Segoe UI", 12, FontStyle.Bold),
				ForeColor = AppColors.Text,
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
			btnClose.MouseEnter += (s, e) => btnClose.ForeColor = AppColors.CloseHover;
			btnClose.MouseLeave += (s, e) => btnClose.ForeColor = Color.White;

			pnlHeader.Controls.Add(lblLogo);
			pnlHeader.Controls.Add(btnClose);
			this.Controls.Add(pnlHeader);
		}

		private void SetupSplitLayout()
		{
			// --- PANEL TRÁI (KHUNG CHỨA GHẾ) ---
			Panel pnlLeft = new Panel
			{
				Location = new Point(30, 80),
				Size = new Size(750, 620),
				BackColor = AppColors.Background
			};

			pnlLeft.Paint += (s, e) => {
				e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
				using (var path = RoundedButton.GetRoundedPath(new Rectangle(0, 0, pnlLeft.Width - 1, pnlLeft.Height - 1), 20))
				using (var brush = new SolidBrush(pnlLeft.BackColor))
				using (var pen = new Pen(Color.FromArgb(51, 65, 85), 1))
				{
					e.Graphics.FillPath(brush, path);
					e.Graphics.DrawPath(pen, path);
				}
			};

			this.Controls.Add(pnlLeft);
			SetupLeftContent(pnlLeft); // Gọi hàm setup nội dung bên trong

			// --- PANEL PHẢI (THÔNG TIN) ---
			Panel pnlRight = new Panel
			{
				Location = new Point(810, 80),
				Size = new Size(360, 620),
				BackColor = AppColors.CardBg
			};
			this.Controls.Add(pnlRight);
			SetupRightContent(pnlRight);
		}

		private void SetupLeftContent(Panel pnl)
		{
			Label lblBack = new Label { Text = "← Quay lại", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = AppColors.Text, AutoSize = true, Location = new Point(30, 20), Cursor = Cursors.Hand };
			lblBack.Click += (s, e) => this.Close();
			pnl.Controls.Add(lblBack);

			pnl.Controls.Add(new Label { Text = $"Sơ đồ ghế tàu {_trainCode}", Font = new Font("Segoe UI", 10, FontStyle.Regular), ForeColor = AppColors.TextMuted, AutoSize = true, Location = new Point(30, 55) });

			flowSeats = new FlowLayoutPanel
			{
				Location = new Point(50, 100),
				Size = new Size(650, 350),
				FlowDirection = FlowDirection.LeftToRight,
				BackColor = Color.Transparent,
				Padding = new Padding(25, 30, 0, 0),
				AutoScroll = true
			};
			pnl.Controls.Add(flowSeats);
		}

		private void SetupRightContent(Panel pnl)
		{
			pnl.Controls.Add(new Label { Text = "Thông tin chuyến đi", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = AppColors.Text, Location = new Point(0, 0), AutoSize = true });

			Panel pnlInfoCard = new Panel { Size = new Size(360, 80), Location = new Point(0, 40), BackColor = Color.Transparent };
			pnlInfoCard.Paint += (s, e) => {
				e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
				using (var path = RoundedButton.GetRoundedPath(new Rectangle(0, 0, 359, 79), 10))
				using (var pen = new Pen(AppColors.BorderColor))
				using (var brush = new SolidBrush(AppColors.CardBg)) { e.Graphics.FillPath(brush, path); e.Graphics.DrawPath(pen, path); }
			};
			AddInfoRow(pnlInfoCard, "Tàu:", $"{_trainCode} - {_trainName}", 15);
			AddInfoRow(pnlInfoCard, "Giá vé:", string.Format("{0:N0}đ", _ticketPrice), 45);
			pnl.Controls.Add(pnlInfoCard);

			int legendY = 150;
			pnl.Controls.Add(CreateHeaderLabel("Chú thích", legendY));
			CreateLegendItem(pnl, "Trống", ClrSeatEmpty, legendY + 35);
			CreateLegendItem(pnl, "Đang chọn", AppColors.Primary, legendY + 65);
			CreateLegendItem(pnl, "Đã bán", ClrSeatSold, legendY + 95);

			int seatListY = legendY + 140;
			pnl.Controls.Add(CreateHeaderLabel("Ghế đang chọn", seatListY));
			lblSelectedList = new Label { Text = "---", Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = AppColors.Primary, AutoSize = false, Size = new Size(360, 30), Location = new Point(0, seatListY + 30), TextAlign = ContentAlignment.MiddleLeft };
			pnl.Controls.Add(lblSelectedList);

			int footerY = 450;
			pnl.Controls.Add(new Label { Text = "Tổng cộng", ForeColor = AppColors.TextMuted, Location = new Point(0, footerY), AutoSize = true });
			lblTotalPrice = new Label { Text = "0 VNĐ", ForeColor = AppColors.Text, Font = new Font("Segoe UI", 16, FontStyle.Bold), Location = new Point(0, footerY + 25), Size = new Size(360, 40), TextAlign = ContentAlignment.MiddleLeft };
			pnl.Controls.Add(lblTotalPrice);

			RoundedButton btnConfirm = new RoundedButton { Text = "XÁC NHẬN ĐẶT VÉ", BackColor = AppColors.Primary, ForeColor = Color.White, Size = new Size(360, 50), Location = new Point(0, footerY + 70) };
			btnConfirm.Click += BtnConfirm_Click;
			btnConfirm.MouseEnter += (s, e) => btnConfirm.BackColor = AppColors.PrimaryHover;
			btnConfirm.MouseLeave += (s, e) => btnConfirm.BackColor = AppColors.Primary;
			pnl.Controls.Add(btnConfirm);
		}

		// =========================================================
		// 5. GENERATE SEATS VỚI VIỀN (BORDER)
		// =========================================================
		private void GenerateSeats()
		{
			string[] seatNames = {
				"1A", "1B", "1C", "1D", "1E",
				"2A", "2B", "2C", "2D", "2E"
			};

			Random rnd = new Random();
			flowSeats.Controls.Clear();

			foreach (var seatName in seatNames)
			{
				bool isSold = rnd.Next(0, 10) > 6;

				RoundedButton btnSeat = new RoundedButton
				{
					Text = seatName + "\n🛋️",
					Size = new Size(90, 90),
					Margin = new Padding(15),
					Font = new Font("Segoe UI", 11, FontStyle.Bold),
					Tag = isSold ? "SOLD" : "EMPTY"
				};

				// Logic hiển thị màu và viền
				if (isSold)
				{
					btnSeat.BackColor = ClrSeatSold;
					btnSeat.ForeColor = Color.Gray;
					btnSeat.Cursor = Cursors.No;
				}
				else
				{
					btnSeat.BackColor = ClrSeatEmpty;
					btnSeat.ForeColor = AppColors.Text;
					btnSeat.Click += Seat_Click;

					// Thêm sự kiện Paint riêng cho nút ghế này để vẽ viền đè lên
					btnSeat.Paint += (s, e) => {
						RoundedButton b = s as RoundedButton;
						// Chỉ vẽ viền khi ghế chưa được chọn (Tag là EMPTY)
						if (b.Tag.ToString() == "EMPTY")
						{
							e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
							using (var pen = new Pen(ClrSeatBorder, 2))
							using (var path = RoundedButton.GetRoundedPath(new Rectangle(1, 1, b.Width - 3, b.Height - 3), 12))
							{
								e.Graphics.DrawPath(pen, path);
							}
						}
					};
				}

				flowSeats.Controls.Add(btnSeat);
			}
		}

		private void Seat_Click(object sender, EventArgs e)
		{
			RoundedButton btn = sender as RoundedButton;
			string seatName = btn.Text.Replace("\n🛋️", "");
			string status = btn.Tag.ToString();

			if (status == "EMPTY")
			{
				btn.BackColor = AppColors.Primary;
				btn.Tag = "SELECTED";
				selectedSeats.Add(seatName);
			}
			else if (status == "SELECTED")
			{
				btn.BackColor = ClrSeatEmpty;
				btn.Tag = "EMPTY";
				selectedSeats.Remove(seatName);
			}

			// Buộc nút vẽ lại để cập nhật viền (mất viền khi chọn, có viền khi bỏ chọn)
			btn.Invalidate();
			UpdateSummary();
		}

		private void UpdateSummary()
		{
			if (selectedSeats.Count > 0) { selectedSeats.Sort(); lblSelectedList.Text = string.Join(", ", selectedSeats); }
			else { lblSelectedList.Text = "---"; }
			long total = selectedSeats.Count * _ticketPrice;
			lblTotalPrice.Text = string.Format("{0:N0} VNĐ", total);
		}

		private void BtnConfirm_Click(object sender, EventArgs e)
		{
			// TODO: Xử lý đặt vé ở đây (gửi dữ liệu về server, v.v.)
			if (selectedSeats.Count == 0) { MessageBox.Show("Vui lòng chọn ít nhất 1 ghế!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
			string msg = $"Đặt vé thành công!\nTàu: {_trainCode}\nGhế: {lblSelectedList.Text}\nTổng tiền: {lblTotalPrice.Text}";
			MessageBox.Show(msg, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
			this.Close();
		}

		// Helper Methods
		private void AddInfoRow(Panel container, string label, string value, int y) { container.Controls.Add(new Label { Text = label, ForeColor = AppColors.TextMuted, Location = new Point(20, y), AutoSize = true }); container.Controls.Add(new Label { Text = value, ForeColor = AppColors.Text, Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(150, y), AutoSize = true }); }
		private Label CreateHeaderLabel(string text, int y) { return new Label { Text = text, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = AppColors.Text, Location = new Point(0, y), AutoSize = true }; }
		private void CreateLegendItem(Panel parent, string text, Color color, int y) { Panel pnlColor = new Panel { Size = new Size(20, 20), Location = new Point(0, y), BackColor = color }; pnlColor.Paint += (s, e) => { e.Graphics.SmoothingMode = SmoothingMode.AntiAlias; using (var path = RoundedButton.GetRoundedPath(new Rectangle(0, 0, 19, 19), 6)) using (var brush = new SolidBrush(color)) e.Graphics.FillPath(brush, path); }; Label lblText = new Label { Text = text, ForeColor = AppColors.TextMuted, Location = new Point(30, y), AutoSize = true }; parent.Controls.Add(pnlColor); parent.Controls.Add(lblText); }

		[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
		[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern bool ReleaseCapture();
		protected override void OnMouseDown(MouseEventArgs e) { if (e.Button == MouseButtons.Left) { ReleaseCapture(); SendMessage(Handle, 0xA1, 0x2, 0); } }
	}
}