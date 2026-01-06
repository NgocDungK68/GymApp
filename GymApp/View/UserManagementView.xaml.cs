using GymApp.Core;
using GymApp.Data;
using GymApp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GymApp.View
{
    public partial class UserManagementView : UserControl
    {
        private User _currentUser;
        private User _originalUser; // để so sánh thay đổi
        private int _userId;

        private bool isPasswordVisible = false;

        public UserManagementView()
        {
            InitializeComponent();
            LoadUser();
        }

        private void LoadUser()
        {
            if (UserSession.CurrentUser == null)
            {
                MessageBox.Show("Không xác định được người dùng");
                return;
            }

            _userId = UserSession.CurrentUser.id;

            using var context = new GymDbContext();

            _currentUser = context.Users.FirstOrDefault(u => u.id == _userId);

            if (_currentUser == null)
            {
                MessageBox.Show("Không tìm thấy người dùng trong hệ thống");
                return;
            }

            // Clone để so sánh
            _originalUser = new User
            {
                name = _currentUser.name,
                password = _currentUser.password,
                age = _currentUser.age,
                gender = _currentUser.gender,
                height = _currentUser.height,
                weight = _currentUser.weight,
                about = _currentUser.about
            };

            BindData();
        }


        private void BindData()
        {
            txtName.Text = _currentUser.name;
            txtPassword.Password = _currentUser.password;
            txtAge.Text = _currentUser.age.ToString();
            txtHeight.Text = _currentUser.height.ToString();
            txtWeight.Text = _currentUser.weight.ToString();
            txtAbout.Text = _currentUser.about;

            foreach (ComboBoxItem item in cbGender.Items)
            {
                if (item.Content.ToString() == _currentUser.gender)
                {
                    cbGender.SelectedItem = item;
                    break;
                }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtAge.Text, out int age) || age <= 0 || age >= 100)
            {
                MessageBox.Show("Tuổi phải từ 1 đến 99");
                return;
            }

            if (!double.TryParse(txtHeight.Text, out double height) || height < 50 || height > 250)
            {
                MessageBox.Show("Chiều cao không hợp lệ");
                return;
            }

            if (!double.TryParse(txtWeight.Text, out double weight) || weight < 10 || weight > 300)
            {
                MessageBox.Show("Cân nặng không hợp lệ");
                return;
            }

            if (cbGender.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn giới tính");
                return;
            }

            string currentPassword = GetCurrentPassword();

            bool isChanged =
                txtName.Text != _originalUser.name ||
                currentPassword != _originalUser.password ||
                age != _originalUser.age ||
                height != _originalUser.height ||
                weight != _originalUser.weight ||
                txtAbout.Text != _originalUser.about ||
                ((ComboBoxItem)cbGender.SelectedItem).Content.ToString() != _originalUser.gender;

            if (!isChanged)
            {
                MessageBox.Show("Dữ liệu không thay đổi");
                return;
            }

            try
            {
                using var context = new GymDbContext();

                var user = context.Users.FirstOrDefault(u => u.id == _userId);
                if (user == null)
                {
                    MessageBox.Show("Không tìm thấy người dùng");
                    return;
                }

                user.name = txtName.Text;
                user.password = currentPassword;
                user.age = age;
                user.height = height;
                user.weight = weight;
                user.about = txtAbout.Text;
                user.gender = ((ComboBoxItem)cbGender.SelectedItem).Content.ToString();

                context.SaveChanges();

                MessageBox.Show("Cập nhật thông tin thành công");

                // ✅ reload để hiển thị dữ liệu mới
                LoadUser();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu dữ liệu:\n" + ex.Message);

                // rollback UI
                BindData();
            }
        }

        private string GetCurrentPassword()
        {
            return isPasswordVisible
                ? txtPasswordVisible.Text
                : txtPassword.Password;
        }

        private void TogglePasswordVisibility_Click(object sender, RoutedEventArgs e)
        {
            if (!isPasswordVisible)
            {
                // Hiện mật khẩu
                txtPasswordVisible.Text = txtPassword.Password;
                txtPasswordVisible.Visibility = Visibility.Visible;
                txtPassword.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Ẩn mật khẩu
                txtPassword.Password = txtPasswordVisible.Text;
                txtPassword.Visibility = Visibility.Visible;
                txtPasswordVisible.Visibility = Visibility.Collapsed;
            }

            isPasswordVisible = !isPasswordVisible;
        }
    }
}
