using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Drawing;
using System;

namespace client.Components
{
	public class ModernTextBox : Panel
	{
		private TextBox _txtInput;
		private Label _lblIcon, _lblTogglePass;
		private string _placeholder = "";
		private bool _isPassword;
		public string TextValue => _txtInput.Text == _placeholder ? "" : _txtInput.Text;

		public event KeyEventHandler? InputKeyDown
		{
			add => _txtInput.KeyDown += value;
			remove => _txtInput.KeyDown -= value;
		}

		public void Clear()
		{
			_txtInput.Text = "";
			SetPlaceholder();
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string PlaceholderText
		{
			get => _placeholder;
			set
			{
				_placeholder = value;
				SetPlaceholder();
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string IconText { get => _lblIcon.Text; set => _lblIcon.Text = value; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsPasswordChar
		{
			get => _isPassword;
			set
			{
				_isPassword = value;
				SetupPasswordMode();
			}
		}

		public sealed override Color BackColor
		{
			get => base.BackColor;
			set
			{
				base.BackColor = value;
				_txtInput.BackColor = value;
			}
		}

		public override Color ForeColor
		{
			get => base.ForeColor;
			set
			{
				base.ForeColor = value;
				_txtInput.ForeColor = value;
			}
		}

		public ModernTextBox()
		{
			this.Padding = new Padding(10);
			_lblIcon = new Label
			{
				Dock = DockStyle.Left,
				Width = 35,
				TextAlign = ContentAlignment.MiddleCenter,
				Font = new Font("Segoe UI Emoji", 12),
				ForeColor = Color.Gray
			};
			_txtInput = new TextBox
			{
				BorderStyle = BorderStyle.None,
				Dock = DockStyle.Fill,
				Font = new Font("Segoe UI", 12),
				ForeColor = Color.Gray,
				BackColor = this.BackColor
			};
			_txtInput.Enter += (_, _) =>
			{
				if (_txtInput.Text == _placeholder)
				{
					_txtInput.Text = "";
					_txtInput.ForeColor = this.ForeColor;
					if (_isPassword) _txtInput.UseSystemPasswordChar = true;
				}
			};
			_txtInput.Leave += SetPlaceholder;
			_lblTogglePass = new Label
			{
				Dock = DockStyle.Right,
				Width = 35,
				TextAlign = ContentAlignment.MiddleCenter,
				Text = "👁️",
				Cursor = Cursors.Hand,
				ForeColor = Color.Gray,
				Visible = false
			};
			_lblTogglePass.Click += (_, _) =>
			{
				if (_txtInput.Text != _placeholder)
				{
					_txtInput.UseSystemPasswordChar = !_txtInput.UseSystemPasswordChar;
					_lblTogglePass.ForeColor = _txtInput.UseSystemPasswordChar ? Color.Gray : Color.White;
				}
			};
			this.Controls.Add(_txtInput);
			this.Controls.Add(_lblIcon);
			this.Controls.Add(_lblTogglePass);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			using GraphicsPath path = RoundedButton.GetRoundedPath(new Rectangle(0, 0, Width - 1, Height - 1), 12);
			using Pen pen = new Pen(Color.FromArgb(71, 85, 105), 1);
			e.Graphics.DrawPath(pen, path);
		}

		private void SetPlaceholder(object? s = null, EventArgs? e = null)
		{
			if (string.IsNullOrWhiteSpace(_txtInput.Text))
			{
				_txtInput.Text = _placeholder;
				_txtInput.ForeColor = Color.Gray;
				if (_isPassword) _txtInput.UseSystemPasswordChar = false;
			}
		}

		private void SetupPasswordMode()
		{
			if (_isPassword)
			{
				_lblTogglePass.Visible = true;
				if (_txtInput.Text != _placeholder) _txtInput.UseSystemPasswordChar = true;
			}
			else
			{
				_lblTogglePass.Visible = false;
				_txtInput.UseSystemPasswordChar = false;
			}
		}

		public string GetText()
		{
			return TextValue;
		}

		public void SetText(string text)
		{
			_txtInput.Text = text;
			if (!string.IsNullOrWhiteSpace(text))
			{
				_txtInput.ForeColor = this.ForeColor;
			}
		}
	}
}