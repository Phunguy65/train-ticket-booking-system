using System.Windows.Forms;
using System.Drawing;

namespace admin.Forms.Authentication
{
	partial class LoginForm
	{
		private System.ComponentModel.IContainer components = null;

		private Panel panelCard;
		private Label lblTitle;
		private Label lblSubtitle;
		private Label lblUsername;
		private Label lblPassword;
		private TextBox txtUsername;
		private TextBox txtPassword;
		private Button btnLogin;

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
				components.Dispose();
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			// ----------------------------------------------------------------
			// FORM SETTINGS (1500 x 850)
			// ----------------------------------------------------------------
			this.SuspendLayout();

			this.AutoScaleDimensions = new SizeF(7F, 15F);
			this.AutoScaleMode = AutoScaleMode.Font;

			this.BackColor = Color.FromArgb(30, 34, 48);  // Nền dark navy
			this.ClientSize = new Size(1500, 850);        // Kích thước mới
			this.FormBorderStyle = FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.StartPosition = FormStartPosition.CenterScreen;
			this.Name = "LoginForm";
			this.Text = "Đăng nhập hệ thống";

			// ----------------------------------------------------------------
			// PANEL CARD (login box)
			// ----------------------------------------------------------------
			panelCard = new Panel();
			panelCard.BackColor = Color.FromArgb(37, 43, 59);   // #252B3B
			panelCard.Size = new Size(400, 520);                // Card login lớn hơn
			panelCard.Padding = new Padding(20);
			panelCard.Location = new Point(0, 0);
			panelCard.BorderStyle = BorderStyle.None;
			// ----------------------------------------------------------------
			// TITLE
			// ----------------------------------------------------------------
			lblTitle = new Label();
			lblTitle.Text = "ĐĂNG NHẬP";
			lblTitle.ForeColor = Color.White;
			lblTitle.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
			lblTitle.AutoSize = true;
			lblTitle.Location = new Point(120, 25);

			// ----------------------------------------------------------------
			// SUBTITLE
			// ----------------------------------------------------------------
			lblSubtitle = new Label();
			lblSubtitle.Text = "Truy cập hệ thống quản trị vé tàu.";
			lblSubtitle.ForeColor = Color.LightGray;
			lblSubtitle.Font = new Font("Segoe UI", 11F);
			lblSubtitle.AutoSize = true;
			lblSubtitle.Location = new Point(80, 75);

			// ----------------------------------------------------------------
			// USERNAME LABEL
			// ----------------------------------------------------------------
			lblUsername = new Label();
			lblUsername.Text = "Email / Tên đăng nhập";
			lblUsername.ForeColor = Color.Gainsboro;
			lblUsername.Font = new Font("Segoe UI", 10F);
			lblUsername.AutoSize = true;
			lblUsername.Location = new Point(20, 140);

			// USERNAME TEXTBOX
			txtUsername = new TextBox();
			txtUsername.Location = new Point(20, 165);
			txtUsername.Size = new Size(350, 32);
			txtUsername.BackColor = Color.FromArgb(50, 56, 72);
			txtUsername.ForeColor = Color.WhiteSmoke;
			txtUsername.BorderStyle = BorderStyle.FixedSingle;
			txtUsername.Font = new Font("Segoe UI", 11F);

			// ----------------------------------------------------------------
			// PASSWORD LABEL
			// ----------------------------------------------------------------
			lblPassword = new Label();
			lblPassword.Text = "Mật khẩu";
			lblPassword.ForeColor = Color.Gainsboro;
			lblPassword.Font = new Font("Segoe UI", 10F);
			lblPassword.AutoSize = true;
			lblPassword.Location = new Point(20, 225);

			// PASSWORD TEXTBOX
			txtPassword = new TextBox();
			txtPassword.Location = new Point(20, 250);
			txtPassword.Size = new Size(350, 32);
			txtPassword.BackColor = Color.FromArgb(50, 56, 72);
			txtPassword.ForeColor = Color.WhiteSmoke;
			txtPassword.BorderStyle = BorderStyle.FixedSingle;
			txtPassword.Font = new Font("Segoe UI", 11F);
			txtPassword.PasswordChar = '•';

			// ----------------------------------------------------------------
			// LOGIN BUTTON
			// ----------------------------------------------------------------
			btnLogin = new Button();
			btnLogin.Text = "ĐĂNG NHẬP";
			btnLogin.Size = new Size(350, 45);
			btnLogin.Location = new Point(20, 330);
			btnLogin.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			btnLogin.BackColor = Color.FromArgb(58, 123, 250);
			btnLogin.ForeColor = Color.White;
			btnLogin.FlatStyle = FlatStyle.Flat;
			btnLogin.FlatAppearance.BorderSize = 0;
			btnLogin.Cursor = Cursors.Hand;
			//btnLogin.Click += new System.EventHandler(this.btnLogin_Click);

			// ----------------------------------------------------------------
			// ADD CONTROLS TO PANEL
			// ----------------------------------------------------------------
			panelCard.Controls.Add(lblTitle);
			panelCard.Controls.Add(lblSubtitle);
			panelCard.Controls.Add(lblUsername);
			panelCard.Controls.Add(txtUsername);
			panelCard.Controls.Add(lblPassword);
			panelCard.Controls.Add(txtPassword);
			panelCard.Controls.Add(btnLogin);

			// ----------------------------------------------------------------
			// ADD PANEL TO FORM
			// ----------------------------------------------------------------
			this.Controls.Add(panelCard);

			this.ResumeLayout(false);
		}
	}
}
