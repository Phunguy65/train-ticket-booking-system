using admin.Models.Dto;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace admin.Forms.UserManagement
{
	public partial class AddUserForm : Form
	{
		private bool IsEditMode = false;
		private int EditingUserId = -1;

		public string FullName => txtFullName.Text.Trim();
		public string Username => txtUsername.Text.Trim();
		public string Email => txtEmail.Text.Trim();
		public string Password => txtPassword.Text;
		public string Phone => txtPhone.Text.Trim();
		public string Role => cbRole.SelectedItem?.ToString();

		public AddUserForm()
		{
			InitializeComponent();
			this.Load += AddUserForm_Load;
		}

		// ==========================
		//   EDIT MODE CONSTRUCTOR
		// ==========================
		public AddUserForm(UserDto user) : this()
		{
			IsEditMode = true;
			EditingUserId = user.UserId;

			txtFullName.Text = user.FullName;
			txtUsername.Text = user.Username;
			txtUsername.Enabled = false;

			txtEmail.Text = user.Email;
			txtPhone.Text = user.PhoneNumber;

			cbRole.SelectedItem = user.Role ?? "Client";
		}

		private void AddUserForm_Load(object sender, EventArgs e)
		{
			ApplyModernUI();
		}


		// ==========================
		//   MODERN UI APPLY
		// ==========================
		private void ApplyModernUI()
		{
			this.Region = CreateRoundedRegion(this.ClientRectangle, 26);

			this.BackColor = Color.FromArgb(30, 35, 50);

			// ❗️CHỈ SET TIÊU ĐỀ NẾU KHÔNG Ở EDIT MODE
			if (!IsEditMode)
				lblTitle.Text = "Thêm người dùng mới";
			else
				lblTitle.Text = "Chỉnh sửa người dùng";

			lblTitle.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
			lblTitle.ForeColor = Color.FromArgb(0, 170, 255);

			foreach (var lbl in new[] { lblFullName, lblUsername, lblEmail, lblPassword, lblPhone, lblRole })
			{
				lbl.ForeColor = Color.FromArgb(200, 210, 240);
				lbl.Font = new Font("Segoe UI", 10F);
			}

			foreach (var tb in new[] { txtFullName, txtUsername, txtEmail, txtPassword, txtPhone })
			{
				tb.BorderStyle = BorderStyle.None;
				tb.BackColor = Color.FromArgb(45, 52, 72);
				tb.ForeColor = Color.White;
				tb.Font = new Font("Segoe UI", 11F);
				tb.Padding = new Padding(16, 12, 16, 12);
				tb.Size = new Size(380, 48);

				tb.Enter += (s, ee) => tb.BackColor = Color.FromArgb(60, 75, 100);
				tb.Leave += (s, ee) => tb.BackColor = Color.FromArgb(45, 52, 72);
			}

			txtPassword.PasswordChar = '●';

			cbRole.FlatStyle = FlatStyle.Flat;
			cbRole.BackColor = Color.FromArgb(45, 52, 72);
			cbRole.ForeColor = Color.White;
			cbRole.Font = new Font("Segoe UI", 11F);
			cbRole.DropDownStyle = ComboBoxStyle.DropDownList;

			if (cbRole.Items.Count == 0)
				cbRole.Items.AddRange(new[] { "Admin", "Client" });

			if (!IsEditMode)
				cbRole.SelectedIndex = 2;


			// ==========================
			//  FIX BUTTON TEXT IN EDIT MODE
			// ==========================
			if (!IsEditMode)
				StyleButton(btnCreate, "Tạo tài khoản", Color.FromArgb(0, 175, 110), Color.FromArgb(0, 200, 130));
			else
				StyleButton(btnCreate, "Cập nhật", Color.FromArgb(0, 140, 180), Color.FromArgb(0, 160, 200));

			StyleButton(btnCancel, "Hủy bỏ", Color.FromArgb(95, 100, 115), Color.FromArgb(120, 125, 140));

			btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
			btnCreate.Click += btnCreate_Click;
		}


		// ==========================
		//   BUTTON STYLE
		// ==========================
		private void StyleButton(Button btn, string text, Color normal, Color hover)
		{
			btn.Text = text;
			btn.BackColor = normal;
			btn.ForeColor = Color.White;
			btn.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
			btn.FlatStyle = FlatStyle.Flat;
			btn.FlatAppearance.BorderSize = 0;
			btn.Cursor = Cursors.Hand;
			btn.Region = CreateRoundedRegion(btn.ClientRectangle, 14);

			btn.MouseEnter += (s, e) => btn.BackColor = hover;
			btn.MouseLeave += (s, e) => btn.BackColor = normal;
		}


		// ==========================
		//   VALIDATE + CLOSE FORM
		// ==========================
		private void btnCreate_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(FullName) ||
				string.IsNullOrWhiteSpace(Username) ||
				string.IsNullOrWhiteSpace(Email))
			{
				MessageBox.Show("Vui lòng nhập đầy đủ thông tin.", "Thiếu dữ liệu",
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			// ❗️Password chỉ bắt buộc khi tạo user
			if (!IsEditMode && string.IsNullOrWhiteSpace(Password))
			{
				MessageBox.Show("Mật khẩu không được để trống!", "Thiếu dữ liệu",
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			DialogResult = DialogResult.OK;
			Close();
		}


		// ==========================
		//   ROUND CORNER REGION
		// ==========================
		private Region CreateRoundedRegion(Rectangle rect, int radius)
		{
			GraphicsPath path = new GraphicsPath();
			int d = radius * 2;
			path.AddArc(rect.X, rect.Y, d, d, 180, 90);
			path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
			path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
			path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
			path.CloseFigure();
			return new Region(path);
		}
	}
}
