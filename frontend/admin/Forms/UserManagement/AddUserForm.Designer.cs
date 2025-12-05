using System.Windows.Forms;

namespace admin.Forms.UserManagement
{
	partial class AddUserForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support.
		/// </summary>
		private void InitializeComponent()
		{
			this.lblTitle = new System.Windows.Forms.Label();
			this.lblFullName = new System.Windows.Forms.Label();
			this.lblUsername = new System.Windows.Forms.Label();
			this.lblEmail = new System.Windows.Forms.Label();
			this.lblPassword = new System.Windows.Forms.Label();
			this.lblPhone = new System.Windows.Forms.Label();
			this.lblRole = new System.Windows.Forms.Label();

			this.txtFullName = new System.Windows.Forms.TextBox();
			this.txtUsername = new System.Windows.Forms.TextBox();
			this.txtEmail = new System.Windows.Forms.TextBox();
			this.txtPassword = new System.Windows.Forms.TextBox();
			this.txtPhone = new System.Windows.Forms.TextBox();

			this.cbRole = new System.Windows.Forms.ComboBox();

			this.btnCreate = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();

			this.SuspendLayout();

			// ==========================================================
			// FORM BASE
			// ==========================================================
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.ClientSize = new System.Drawing.Size(460, 620);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Name = "AddUserForm";

			// ==========================================================
			// LABEL: TITLE
			// ==========================================================
			this.lblTitle.AutoSize = true;
			this.lblTitle.Location = new System.Drawing.Point(40, 30);
			this.lblTitle.Name = "lblTitle";
			this.lblTitle.Size = new System.Drawing.Size(120, 20);
			this.lblTitle.Text = "Thêm người dùng";

			// ==========================================================
			// LABELS
			// ==========================================================
			this.lblFullName.Location = new System.Drawing.Point(40, 90);
			this.lblFullName.AutoSize = true;
			this.lblFullName.Text = "Họ và tên";

			this.lblUsername.Location = new System.Drawing.Point(40, 155);
			this.lblUsername.AutoSize = true;
			this.lblUsername.Text = "Tên đăng nhập";

			this.lblEmail.Location = new System.Drawing.Point(40, 220);
			this.lblEmail.AutoSize = true;
			this.lblEmail.Text = "Email";

			this.lblPassword.Location = new System.Drawing.Point(40, 285);
			this.lblPassword.AutoSize = true;
			this.lblPassword.Text = "Mật khẩu";

			this.lblPhone.Location = new System.Drawing.Point(40, 350);
			this.lblPhone.AutoSize = true;
			this.lblPhone.Text = "Số điện thoại";

			this.lblRole.Location = new System.Drawing.Point(40, 415);
			this.lblRole.AutoSize = true;
			this.lblRole.Text = "Vai trò";

			// ==========================================================
			// TEXTBOXES
			// ==========================================================
			this.txtFullName.Location = new System.Drawing.Point(40, 110);
			this.txtFullName.Size = new System.Drawing.Size(380, 30);
			this.txtFullName.Name = "txtFullName";

			this.txtUsername.Location = new System.Drawing.Point(40, 175);
			this.txtUsername.Size = new System.Drawing.Size(380, 30);
			this.txtUsername.Name = "txtUsername";

			this.txtEmail.Location = new System.Drawing.Point(40, 240);
			this.txtEmail.Size = new System.Drawing.Size(380, 30);
			this.txtEmail.Name = "txtEmail";

			this.txtPassword.Location = new System.Drawing.Point(40, 305);
			this.txtPassword.Size = new System.Drawing.Size(380, 30);
			this.txtPassword.Name = "txtPassword";

			this.txtPhone.Location = new System.Drawing.Point(40, 370);
			this.txtPhone.Size = new System.Drawing.Size(380, 30);
			this.txtPhone.Name = "txtPhone";

			// ==========================================================
			// COMBOBOX ROLE
			// ==========================================================
			this.cbRole.Location = new System.Drawing.Point(40, 440);
			this.cbRole.Size = new System.Drawing.Size(380, 30);
			this.cbRole.Name = "cbRole";
			this.cbRole.DropDownStyle = ComboBoxStyle.DropDownList;

			// ==========================================================
			// BUTTONS
			// ==========================================================
			this.btnCreate.Location = new System.Drawing.Point(40, 530);
			this.btnCreate.Size = new System.Drawing.Size(180, 50);
			this.btnCreate.Name = "btnCreate";
			this.btnCreate.Text = "Tạo tài khoản";

			this.btnCancel.Location = new System.Drawing.Point(240, 530);
			this.btnCancel.Size = new System.Drawing.Size(180, 50);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Text = "Hủy";

			// ==========================================================
			// ADD CONTROLS
			// ==========================================================
			this.Controls.Add(lblTitle);

			this.Controls.Add(lblFullName);
			this.Controls.Add(lblUsername);
			this.Controls.Add(lblEmail);
			this.Controls.Add(lblPassword);
			this.Controls.Add(lblPhone);
			this.Controls.Add(lblRole);

			this.Controls.Add(txtFullName);
			this.Controls.Add(txtUsername);
			this.Controls.Add(txtEmail);
			this.Controls.Add(txtPassword);
			this.Controls.Add(txtPhone);

			this.Controls.Add(cbRole);

			this.Controls.Add(btnCreate);
			this.Controls.Add(btnCancel);

			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private Label lblTitle;
		private Label lblFullName;
		private Label lblUsername;
		private Label lblEmail;
		private Label lblPassword;
		private Label lblPhone;
		private Label lblRole;

		private TextBox txtFullName;
		private TextBox txtUsername;
		private TextBox txtEmail;
		private TextBox txtPassword;
		private TextBox txtPhone;

		private ComboBox cbRole;

		private Button btnCreate;
		private Button btnCancel;
	}
}
