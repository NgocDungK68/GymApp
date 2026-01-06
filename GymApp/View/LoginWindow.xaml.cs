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
using System.Windows.Shapes;

namespace GymApp.View
{
    public partial class LoginWindow : Window
    {
        private bool isLoginMode = true;
        private bool isPasswordVisible = false;

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void SwitchMode_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            isLoginMode = !isLoginMode;

            if (isLoginMode)
            {
                txtTitle.Text = "Đăng nhập";
                btnMain.Content = "Đăng nhập";
                txtSwitchQuestion.Text = "Chưa có tài khoản? ";
                txtSwitchAction.Text = "Đăng ký";
            }
            else
            {
                txtTitle.Text = "Đăng ký";
                btnMain.Content = "Đăng ký";
                txtSwitchQuestion.Text = "Đã có tài khoản? ";
                txtSwitchAction.Text = "Đăng nhập";
            }

            // clear input cho sạch
            txtUsername.Clear();
            txtPassword.Clear();
            txtPasswordVisible.Clear();
            txtPassword.Visibility = Visibility.Visible;
            txtPasswordVisible.Visibility = Visibility.Collapsed;
            isPasswordVisible = false;
        }

        private void MainButton_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = isPasswordVisible
                ? txtPasswordVisible.Text
                : txtPassword.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin");
                return;
            }

            using var context = new GymDbContext();

            if (isLoginMode)
            {
                // LOGIN
                var user = context.Users
                    .FirstOrDefault(u => u.name == username && u.password == password);

                if (user == null)
                {
                    MessageBox.Show("Sai tài khoản hoặc mật khẩu");
                    return;
                }

                // Lưu phiên đăng nhập
                UserSession.SetUser(user);
                MessageBox.Show("Đăng nhập thành công!");

                // Chuyển sang màn hình chính
                var mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
            else
            {
                // REGISTER
                bool exists = context.Users.Any(u => u.name == username);
                if (exists)
                {
                    MessageBox.Show("Tài khoản đã tồn tại");
                    return;
                }

                // Chuyển sang màn hình hoàn thiện hồ sơ

                var profileWindow = new ProfileSetupWindow(username, password);
                profileWindow.Show();
                this.Close();
            }
        }

        private void txtUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtUsernameHint.Visibility =
                string.IsNullOrEmpty(txtUsername.Text)
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            txtPasswordHint.Visibility =
                string.IsNullOrEmpty(txtPassword.Password)
                ? Visibility.Visible
                : Visibility.Hidden;
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
