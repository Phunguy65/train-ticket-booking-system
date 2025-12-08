using System.Drawing;

namespace client.Helpers
{
	// Class tĩnh chứa bảng màu chung cho toàn ứng dụng
	public static class AppColors
	{
		// Màu nền
		public static readonly Color Background = Color.FromArgb(30, 41, 59);      // Slate 800
		public static readonly Color CardBg = Color.FromArgb(15, 23, 42);          // Slate 900
		public static readonly Color InputBg = Color.FromArgb(51, 65, 85);         // Slate 700

		// Màu chữ
		public static readonly Color Text = Color.White;
		public static readonly Color TextMuted = Color.FromArgb(148, 163, 184);

		// Màu thương hiệu (Primary)
		public static readonly Color Primary = Color.FromArgb(37, 99, 235);        // Blue 600
		public static readonly Color PrimaryHover = Color.FromArgb(29, 78, 216);   // Blue 700

		// Màu nút điều khiển cửa sổ
		public static readonly Color HeaderHover = Color.FromArgb(51, 65, 85);
		public static readonly Color CloseHover = Color.FromArgb(220, 38, 38);
		public static readonly Color BorderColor = Color.FromArgb(71, 85, 105);
	}
}