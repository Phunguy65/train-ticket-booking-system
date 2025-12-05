	using FontAwesome.Sharp;     // ← thêm dòng này thôi
	using System.Windows.Forms;
	using System.Drawing;

	namespace admin.Forms.UserManagement
	{
		partial class UserManagementControl
		{
			private Label lblTitle;
			private TextBox txtSearch;
			private IconButton btnSearch;      // ← đổi thành IconButton
			private IconButton btnRefresh;     // ← đổi thành IconButton
			private IconButton btnAddUser;     // ← đổi thành IconButton
			private IconButton btnEditUser;    // ← đổi thành IconButton
			private IconButton btnDeleteUser;  // ← đổi thành IconButton
			private DataGridView dgvUsers;

			private void InitializeComponent()
			{
				this.lblTitle = new Label();
				this.txtSearch = new TextBox();
				this.btnSearch = new IconButton();
				this.btnRefresh = new IconButton();
				this.btnAddUser = new IconButton();
				this.btnEditUser = new IconButton();
				this.btnDeleteUser = new IconButton();
				this.dgvUsers = new DataGridView();

				((System.ComponentModel.ISupportInitialize)(this.dgvUsers)).BeginInit();
				this.SuspendLayout();

				// (giữ nguyên tất cả vị trí, kích thước như cũ của bạn)
				lblTitle.Location = new Point(25, 25);
				lblTitle.AutoSize = true;
				lblTitle.Text = "Quản lý người dùng";

				txtSearch.Location = new Point(25, 90);
				txtSearch.Size = new Size(260, 30);

				txtSearch.Size = new Size(260, 30);

				btnSearch.Location = new Point(295, 89);
				btnRefresh.Location = new Point(395, 89);
				btnEditUser.Location = new Point(495, 89);
				btnDeleteUser.Location = new Point(595, 89);
				btnAddUser.Location = new Point(940, 85);
				btnAddUser.Size = new Size(180, 40);

				dgvUsers.Location = new Point(25, 150);
				dgvUsers.Size = new Size(1200, 650);
				dgvUsers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

				this.Controls.Add(this.lblTitle);
				this.Controls.Add(this.txtSearch);
				this.Controls.Add(this.btnSearch);
				this.Controls.Add(this.btnRefresh);
				this.Controls.Add(this.btnEditUser);
				this.Controls.Add(this.btnDeleteUser);
				this.Controls.Add(this.btnAddUser);
				this.Controls.Add(this.dgvUsers);

				this.Size = new Size(1250, 850);

				((System.ComponentModel.ISupportInitialize)(this.dgvUsers)).EndInit();
				this.ResumeLayout(false);
				this.PerformLayout();
			}
		}
	}