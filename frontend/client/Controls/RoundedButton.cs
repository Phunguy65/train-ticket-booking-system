using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace client.Controls
{
	public class RoundedButton : Button
	{
		// Constructor mặc định để thiết lập các thuộc tính cơ bản
		public RoundedButton()
		{
			this.FlatStyle = FlatStyle.Flat;
			this.FlatAppearance.BorderSize = 0;
			this.Cursor = Cursors.Hand;
			this.Font = new Font("Segoe UI", 12, FontStyle.Bold);
		}

		protected override void OnPaint(PaintEventArgs pevent)
		{
			Graphics g = pevent.Graphics;
			g.SmoothingMode = SmoothingMode.AntiAlias;

			using (GraphicsPath path = GetRoundedPath(new Rectangle(0, 0, Width, Height), 12))
			using (SolidBrush brush = new SolidBrush(this.BackColor))
			{
				// Vẽ nền bo tròn
				this.Region = new Region(path);
				g.FillPath(brush, path);

				// Vẽ chữ căn giữa
				SizeF textSize = g.MeasureString(this.Text, this.Font);
				PointF textPos = new PointF(
					(Width - textSize.Width) / 2,
					(Height - textSize.Height) / 2
				);
				g.DrawString(this.Text, this.Font, new SolidBrush(this.ForeColor), textPos);
			}
		}

		// Hàm tiện ích tạo đường dẫn bo tròn (Static để dùng ké ở nơi khác nếu cần)
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
	}
}