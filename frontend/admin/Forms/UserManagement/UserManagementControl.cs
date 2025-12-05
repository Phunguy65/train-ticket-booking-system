using admin.Mock;
using admin.Models.Dto;
using FontAwesome.Sharp;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace admin.Forms.UserManagement
{
	public partial class UserManagementControl : UserControl
	{
		private Panel searchPanel;
		private IconPictureBox searchIcon;

		public UserManagementControl()
		{
			InitializeComponent();
			this.Load += UserManagementControl_Load;
		}

		private void UserManagementControl_Load(object sender, EventArgs e)
		{
			BuildSearchBox();        // FIX search icon layout
			ApplyModernStyle();
			ApplyRoundedButtons();

			btnAddUser.Click += btnAddUser_Click;
			btnRefresh.Click += BtnRefresh_Click;
			btnEditUser.Click += BtnEditUser_Click;
			//btnDeleteUser.Click += BtnDeleteUser_Click;

			LoadUsers();
		}
		private void BtnRefresh_Click(object sender, EventArgs e)
		{
			LoadUsers();
		}

		private UserDto GetSelectedUser()
		{
			if (dgvUsers.SelectedRows.Count == 0)
			{
				MessageBox.Show("Vui lòng chọn một người dùng!", "Thông báo",
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return null;
			}

			int userId = Convert.ToInt32(dgvUsers.SelectedRows[0].Cells["UserId"].Value);
			return MockDatabase.Users.FirstOrDefault(u => u.UserId == userId);
		}

		private void BtnEditUser_Click(object sender, EventArgs e)
		{
			var user = GetSelectedUser();
			if (user == null) return;

			AddUserForm f = new AddUserForm(user);

			Console.Write(user.UserId);

			if (f.ShowDialog() == DialogResult.OK)
			{
				// Đoạn này sẽ có 
				//// tìm user trong mock DB
				var u = MockDatabase.Users.First(x => x.UserId == user.UserId);

				u.FullName = f.FullName;
				u.Username = f.Username;
				u.Email = f.Email;
				u.PhoneNumber = f.Phone;
				u.Role = f.Role;

				// Optional: cập nhật mật khẩu nếu user nhập
				if (!string.IsNullOrWhiteSpace(f.Password))
					u.PasswordHash = "HASH_" + Guid.NewGuid().ToString("N");

				LoadUsers();
			}
		}


		// ============================================================
		// 🔍 BUILD SEARCH BOX WITH ICON INSIDE
		// ============================================================
		private void BuildSearchBox()
		{
			searchPanel = new Panel();
			searchPanel.BackColor = Color.FromArgb(45, 50, 65);
			searchPanel.Size = new Size(360, 42);
			searchPanel.Location = new Point(35, 90);
			searchPanel.Padding = new Padding(40, 0, 10, 0);

			// Icon search nằm trong panel → không bao giờ lệch
			searchIcon = new IconPictureBox();
			searchIcon.IconChar = IconChar.Search;
			searchIcon.IconColor = Color.Gainsboro;
			searchIcon.IconSize = 22;
			searchIcon.BackColor = Color.FromArgb(45, 50, 65);
			searchIcon.Size = new Size(32, 32);
			searchIcon.Location = new Point(10, 11);

			// Textbox
			txtSearch.BorderStyle = BorderStyle.None;
			txtSearch.BackColor = Color.FromArgb(45, 50, 65);
			txtSearch.ForeColor = Color.FromArgb(160, 160, 160);
			txtSearch.Location = new Point(45, 11);
			txtSearch.Font = new Font("Segoe UI", 11F);
			txtSearch.Size = new Size(310, 25);
			txtSearch.Text = "Tìm kiếm tên, email, số điện thoại...";

			txtSearch.GotFocus += (s, e) =>
			{
				if (txtSearch.Text == "Tìm kiếm tên, email, số điện thoại...")
				{
					txtSearch.Text = "";
					txtSearch.ForeColor = Color.White;
				}
			};

			txtSearch.LostFocus += (s, e) =>
			{
				if (string.IsNullOrWhiteSpace(txtSearch.Text))
				{
					txtSearch.Text = "Tìm kiếm tên, email, số điện thoại...";
					txtSearch.ForeColor = Color.FromArgb(170, 170, 170);
				}
			};

			searchPanel.Controls.Add(searchIcon);
			searchPanel.Controls.Add(txtSearch);

			this.Controls.Add(searchPanel);

			// Ẩn vị trí cũ
			txtSearch.Visible = false;
			btnSearch.Visible = false;
		}

		// ============================================================
		// APPLY UI STYLE
		// ============================================================
		private void ApplyModernStyle()
		{
			this.BackColor = Color.FromArgb(28, 30, 38);

			lblTitle.Font = new Font("Segoe UI", 28F, FontStyle.Bold);
			lblTitle.ForeColor = Color.FromArgb(0, 170, 255);
			lblTitle.Location = new Point(35, 25);

			StyleIconButton(btnRefresh, IconChar.SyncAlt, "Làm mới", Color.FromArgb(70, 130, 255), 420);
			StyleIconButton(btnEditUser, IconChar.UserEdit, "Sửa", Color.FromArgb(255, 145, 0), 555);
			StyleIconButton(btnDeleteUser, IconChar.TrashAlt, "Xoá", Color.FromArgb(200, 60, 60), 690);

			btnAddUser.IconChar = IconChar.UserPlus;
			btnAddUser.IconColor = Color.White;
			btnAddUser.Text = "  Thêm người dùng";
			btnAddUser.TextImageRelation = TextImageRelation.ImageBeforeText;
			btnAddUser.BackColor = Color.FromArgb(0, 175, 110);
			btnAddUser.ForeColor = Color.White;
			btnAddUser.FlatStyle = FlatStyle.Flat;
			btnAddUser.FlatAppearance.BorderSize = 0;
			btnAddUser.Font = new Font("Segoe UI", 12F);
			btnAddUser.Size = new Size(230, 52);

			// Anchor bên phải để tự co giãn theo size của UserControl
			btnAddUser.Anchor = AnchorStyles.Top | AnchorStyles.Right;

			// auto-place theo chiều rộng control
			btnAddUser.Location = new Point(this.Width - btnAddUser.Width - 40, 88);

			// Nếu lúc load chưa cập nhật width → gọi lại sau khi layout xong
			this.Resize += (s, e) =>
			{
				btnAddUser.Location = new Point(this.Width - btnAddUser.Width - 40, 88);
			};


			StyleGrid();
		}

		private void StyleIconButton(IconButton btn, IconChar icon, string text, Color color, int x)
		{
			btn.IconChar = icon;
			btn.IconColor = Color.White;
			btn.IconSize = 20;

			btn.Text = "  " + text;
			btn.TextImageRelation = TextImageRelation.ImageBeforeText;

			btn.BackColor = color;
			btn.ForeColor = Color.White;
			btn.FlatStyle = FlatStyle.Flat;
			btn.FlatAppearance.BorderSize = 0;

			btn.Font = new Font("Segoe UI", 10.5F);
			btn.Size = new Size(120, 42);
			btn.Location = new Point(x, 90);
		}

		// ============================================================
		// ROUNDED BUTTONS (safe — no designer modify)
		// ============================================================
		private void ApplyRoundedButtons()
		{
			Round(btnRefresh);
			Round(btnEditUser);
			Round(btnDeleteUser);
			Round(btnAddUser);
		}

		private void Round(Button btn)
		{
			Rectangle r = new Rectangle(0, 0, btn.Width, btn.Height);
			int radius = 12;
			GraphicsPath path = new GraphicsPath();

			path.AddArc(r.X, r.Y, radius, radius, 180, 90);
			path.AddArc(r.Right - radius, r.Y, radius, radius, 270, 90);
			path.AddArc(r.Right - radius, r.Bottom - radius, radius, radius, 0, 90);
			path.AddArc(r.X, r.Bottom - radius, radius, radius, 90, 90);

			path.CloseFigure();
			btn.Region = new Region(path);
		}

		// ============================================================
		// LOAD USERS FROM MOCK
		// ============================================================
		private void LoadUsers()
		{
			dgvUsers.DataSource = MockDatabase.Users
				.Select(u => new
				{
					u.UserId,
					u.Username,
					u.FullName,
					u.Email,
					u.PhoneNumber,
					u.Role,
					Status = u.IsActive ? "Hoạt động" : "Đã khoá",
					CreatedAt = u.CreatedAt.ToString("yyyy-MM-dd HH:mm")
				})
				.ToList();

			StyleGrid();
		}

		private void StyleGrid()
		{
			dgvUsers.BackgroundColor = Color.FromArgb(35, 38, 48);
			dgvUsers.BorderStyle = BorderStyle.None;
			dgvUsers.EnableHeadersVisualStyles = false;

			dgvUsers.ColumnHeadersHeight = 45;
			dgvUsers.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 50, 65);
			dgvUsers.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;

			dgvUsers.DefaultCellStyle.BackColor = Color.FromArgb(40, 43, 55);
			dgvUsers.DefaultCellStyle.ForeColor = Color.WhiteSmoke;
			dgvUsers.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);

			dgvUsers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
		}

		private void btnAddUser_Click(object sender, EventArgs e)
		{
			AddUserForm f = new AddUserForm();

			if (f.ShowDialog() == DialogResult.OK)
			{
				MockDatabase.Users.Add(new UserDto
				{
					UserId = MockDatabase.Users.Max(u => u.UserId) + 1,
					Username = f.Username,
					PasswordHash = "HASH_" + Guid.NewGuid(),
					FullName = f.FullName,
					Email = f.Email,
					PhoneNumber = f.Phone,
					Role = f.Role,
					CreatedAt = DateTime.Now,
					IsActive = true
				});

				LoadUsers();
			}
		}
	}
}
