using GymApp.Core;
using GymApp.Data;
using GymApp.Model;
using System;
using System.Windows;

namespace GymApp.View
{
    public partial class AddFoodView : Window
    {
        public AddFoodView()
        {
            InitializeComponent();
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            // ===== CHECK LOGIN =====
            if (!UserSession.IsLoggedIn)
            {
                MessageBox.Show("Bạn chưa đăng nhập!", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // ===== GET INPUT =====
            string name = txtFoodName.Text.Trim();
            string servingUnit = txtServingUnit.Text.Trim();
            string servingSizeText = txtServingSize.Text.Trim();
            string caloriesText = txtCalories.Text.Trim();

            // ===== VALIDATE =====
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Tên thực phẩm không được để trống!", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtFoodName.Focus();
                return;
            }

            if (string.IsNullOrEmpty(servingUnit))
            {
                MessageBox.Show("Đơn vị khẩu phần không được để trống!", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtServingUnit.Focus();
                return;
            }

            if (!double.TryParse(servingSizeText, out double servingSize) || servingSize <= 0)
            {
                MessageBox.Show("Kích thước khẩu phần không hợp lệ!", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtServingSize.Focus();
                return;
            }

            if (!double.TryParse(caloriesText, out double calories) || calories <= 0)
            {
                MessageBox.Show("Calories không hợp lệ!", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCalories.Focus();
                return;
            }

            // ===== SAVE TO DB =====
            try
            {
                using var context = new GymDbContext();

                var food = new Food
                {
                    name = name,
                    serving_unit = servingUnit,
                    serving_size = servingSize,
                    calories = calories,
                    owner_id = UserSession.CurrentUser!.id,
                    is_system = false
                };

                context.Foods.Add(food);
                context.SaveChanges();

                MessageBox.Show("Thêm thực phẩm thành công!", "Thành công",
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
