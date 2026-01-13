using GymApp.Core;
using GymApp.Data;
using GymApp.Model;
using System;
using System.Linq;
using System.Windows;

namespace GymApp.View.Groups
{
    public partial class AddGroupView : Window
    {
        public AddGroupView()
        {
            InitializeComponent();
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            // ===== VALIDATE LOGIN =====
            if (!UserSession.IsLoggedIn)
            {
                MessageBox.Show("Bạn chưa đăng nhập!", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // ===== VALIDATE INPUT =====
            string groupName = txtGroupName.Text.Trim();
            string groupDescription = txtGroupDescription.Text.Trim();

            if (string.IsNullOrEmpty(groupName))
            {
                MessageBox.Show("Tên nhóm không được để trống!", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtGroupName.Focus();
                return;
            }

            // ===== SAVE TO DB =====
            try
            {
                using var context = new GymDbContext();

                // Tạo nhóm mới
                var group = new Group
                {
                    name = groupName,
                    description = groupDescription
                };

                context.Groups.Add(group);
                context.SaveChanges(); // Lưu để lấy ID tự sinh

                // Tạo bản ghi Group_User cho người tạo nhóm
                var groupUser = new GroupUser
                {
                    GroupId = group.id,
                    UserId = UserSession.CurrentUser!.id,
                    joined_at = DateTime.Now,
                    role_in_group = "Admin"
                };

                context.GroupUsers.Add(groupUser);
                context.SaveChanges();

                MessageBox.Show("Tạo nhóm thành công!", "Thành công",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu dữ liệu:\n" + ex.Message,
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
