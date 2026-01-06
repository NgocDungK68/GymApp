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
    public partial class ProfileSetupWindow : Window
    {
        private readonly string _username;
        private readonly string _password;

        public ProfileSetupWindow(string username, string password)
        {
            InitializeComponent();
            _username = username;
            _password = password;
        }

        private void Finish_Click(object sender, RoutedEventArgs e)
        {
            // Validate tuổi
            if (!int.TryParse(txtAge.Text, out int age) || age <= 0 || age >= 100)
            {
                MessageBox.Show("Tuổi phải là số hợp lệ (1 - 99)");
                return;
            }

            // Validate chiều cao
            if (!double.TryParse(txtHeight.Text, out double height) || height < 50 || height > 250)
            {
                MessageBox.Show("Chiều cao phải trong khoảng 50 - 250 cm");
                return;
            }

            // Validate cân nặng
            if (!double.TryParse(txtWeight.Text, out double weight) || weight < 10 || weight > 300)
            {
                MessageBox.Show("Cân nặng phải trong khoảng 10 - 300 kg");
                return;
            }

            // Validate giới tính
            if (cbGender.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn giới tính");
                return;
            }

            string gender = ((ComboBoxItem)cbGender.SelectedItem).Content.ToString();
            string about = string.IsNullOrWhiteSpace(txtAbout.Text) ? null : txtAbout.Text.Trim();

            using var context = new GymDbContext();

            var user = new User
            {
                name = _username,
                password = _password,
                age = age,
                height = height,
                weight = weight,
                gender = gender,
                about = about,
                avatar = null
            };

            context.Users.Add(user);
            context.SaveChanges();

            MessageBox.Show("Hoàn thiện hồ sơ thành công! Vui lòng đăng nhập lại.");

            // QUAY VỀ LOGIN
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}

