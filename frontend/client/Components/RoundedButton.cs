using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace client.Components
{
	public class RoundedButton : Button
	{
		protected override void OnPaint(PaintEventArgs pevent)
		{
			Graphics g = pevent.Graphics;
			g.SmoothingMode = SmoothingMode.AntiAlias;
			using GraphicsPath path = GetRoundedPath(new Rectangle(0, 0, Width, Height), 12);
			using SolidBrush brush = new SolidBrush(this.BackColor);
			this.Region = new Region(path);
			g.FillPath(brush, path);
			SizeF textSize = g.MeasureString(this.Text, this.Font);
			g.DrawString(this.Text, this.Font, new SolidBrush(this.ForeColor),
				new PointF((Width - textSize.Width) / 2, (Height - textSize.Height) / 2));
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
	}
}