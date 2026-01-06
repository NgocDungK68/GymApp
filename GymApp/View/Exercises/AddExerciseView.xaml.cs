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
    public partial class AddExerciseView : Window
    {
        public AddExerciseView()
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
            string name = txtExerciseName.Text.Trim();
            string muscles = txtMuscles.Text.Trim();
            string description = txtDescription.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Tên bài tập không được để trống!", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtExerciseName.Focus();
                return;
            }

            if (string.IsNullOrEmpty(muscles))
            {
                MessageBox.Show("Nhóm cơ không được để trống!", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtMuscles.Focus();
                return;
            }

            // ===== SAVE TO DB =====
            try
            {
                using var context = new GymDbContext();

                var exercise = new Exercise
                {
                    name = name,
                    muscles = muscles,
                    about = description,
                    owner_id = UserSession.CurrentUser!.id,
                    is_system = false
                };

                context.Exercises.Add(exercise);
                context.SaveChanges();

                MessageBox.Show("Thêm bài tập thành công!", "Thành công",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                this.Close();
            }
            catch (System.Exception ex)
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
