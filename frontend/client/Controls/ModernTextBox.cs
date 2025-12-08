using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using client.Helpers;

namespace client.Controls
{
	public class ModernTextBox : Panel
	{
		// Các thành phần con
		private TextBox txtInput;
		private Label lblIcon;
		private Label lblTogglePass;

		// Biến cục bộ
		private string _placeholder = "";
		private bool _isPassword = false;

		// Properties công khai
		public string TextValue => txtInput.Text == _placeholder ? "" : txtInput.Text;

		public string PlaceholderText
		{
			get => _placeholder;
			set { _placeholder = value; SetPlaceholder(); }
		}

		public string IconText
		{
			get => lblIcon.Text;
			set => lblIcon.Text = value;
		}

		public bool IsPasswordChar
		{
			get => _isPassword;
			set { _isPassword = value; SetupPasswordMode(); }
		}

		// Đồng bộ màu nền/chữ của Panel cha xuống TextBox con
		public override Color BackColor
		{
			get => base.BackColor;
			set { base.BackColor = value; if (txtInput != null) txtInput.BackColor = value; }
		}

		public override Color ForeColor
		{
			get => base.ForeColor;
			set { base.ForeColor = value; if (txtInput != null) txtInput.ForeColor = value; }
		}

		public ModernTextBox()
		{
			this.Padding = new Padding(10);

			// 1. Icon bên trái
			lblIcon = new Label
			{
				Dock = DockStyle.Left,
				Width = 35,
				TextAlign = ContentAlignment.MiddleCenter,
				Font = new Font("Segoe UI Emoji", 12),
				ForeColor = Color.Gray
			};

			// 2. TextBox chính
			txtInput = new TextBox
			{
				BorderStyle = BorderStyle.None,
				Dock = DockStyle.Fill,
				Font = new Font("Segoe UI", 12),
				ForeColor = Color.Gray,
				BackColor = this.BackColor
			};

			// Xử lý Placeholder
			txtInput.Enter += (s, e) => {
				if (txtInput.Text == _placeholder)
				{
					txtInput.Text = "";
					txtInput.ForeColor = this.ForeColor;
					if (_isPassword) txtInput.UseSystemPasswordChar = true;
				}
			};
			txtInput.Leave += SetPlaceholder;

			// 3. Nút ẩn/hiện mật khẩu (Con mắt)
			lblTogglePass = new Label
			{
				Dock = DockStyle.Right,
				Width = 35,
				TextAlign = ContentAlignment.MiddleCenter,
				Text = "👁️",
				Cursor = Cursors.Hand,
				ForeColor = Color.Gray,
				Visible = false
			};

			lblTogglePass.Click += (s, e) => {
				if (txtInput.Text != _placeholder)
				{
					txtInput.UseSystemPasswordChar = !txtInput.UseSystemPasswordChar;
					lblTogglePass.ForeColor = txtInput.UseSystemPasswordChar ? Color.Gray : Color.White;
				}
			};

			// Thêm vào Panel (Thứ tự quan trọng)
			this.Controls.Add(txtInput);
			this.Controls.Add(lblIcon);
			this.Controls.Add(lblTogglePass);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			// Vẽ viền bo tròn
			using (GraphicsPath path = RoundedButton.GetRoundedPath(new Rectangle(0, 0, Width - 1, Height - 1), 12))
			using (Pen pen = new Pen(AppColors.BorderColor, 1))
			{
				e.Graphics.DrawPath(pen, path);
			}
		}

		private void SetPlaceholder(object s = null, EventArgs e = null)
		{
			if (string.IsNullOrWhiteSpace(txtInput.Text))
			{
				txtInput.Text = _placeholder;
				txtInput.ForeColor = Color.Gray;
				if (_isPassword) txtInput.UseSystemPasswordChar = false;
			}
		}

		private void SetupPasswordMode()
		{
			if (_isPassword)
			{
				lblTogglePass.Visible = true;
				if (txtInput.Text != _placeholder) txtInput.UseSystemPasswordChar = true;
			}
			else
			{
				lblTogglePass.Visible = false;
				txtInput.UseSystemPasswordChar = false;
			}
		}

		public override string Text
		{
			get => txtInput.Text == _placeholder ? "" : txtInput.Text;
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					txtInput.Text = value;
					txtInput.ForeColor = this.ForeColor; // Đổi màu chữ về màu chính (Trắng)
					if (_isPassword) txtInput.UseSystemPasswordChar = true;
				}
				else
				{
					// Nếu gán rỗng thì hiện Placeholder
					txtInput.Text = _placeholder;
					txtInput.ForeColor = Color.Gray;
					if (_isPassword) txtInput.UseSystemPasswordChar = false;
				}
			}
		}
	}
}