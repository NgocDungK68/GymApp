using GymApp.Core;
using GymApp.Data;
using GymApp.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GymApp.View.Nutritions
{
    public partial class DetailFoodView : UserControl
    {
        private readonly int _foodId;
        private Food? _food;

        // Lưu dữ liệu gốc để so sánh thay đổi
        private string _originalName = "";
        private string _originalUnit = "";
        private double? _originalSize;
        private double _originalCalories;

        private bool _isEditing = false;

        public event Action<int>? FoodSelected;

        public DetailFoodView(int foodId)
        {
            InitializeComponent();
            _foodId = foodId;

            LoadFood();
            SetReadOnlyMode();
        }

        // ================= LOAD DATA =================
        private void LoadFood()
        {
            using var context = new GymDbContext();

            _food = context.Foods
                           .AsNoTracking()
                           .FirstOrDefault(f => f.id == _foodId);

            if (_food == null)
            {
                MessageBox.Show("Không tìm thấy thức ăn");
                return;
            }

            txtFoodName.Text = _food.name;
            txtServingUnit.Text = _food.serving_unit;
            txtServingSize.Text = _food.serving_size?.ToString() ?? "";
            txtCalories.Text = _food.calories.ToString();
            txtCreatedBy.Text = _food.is_system ? "System" : "Bạn";

            // Lưu bản gốc
            _originalName = _food.name;
            _originalUnit = _food.serving_unit;
            _originalSize = _food.serving_size;
            _originalCalories = _food.calories;
        }

        // ================= BUTTON EVENTS =================

        // Tạo bữa ăn
        private void BtnCreateMeal_Click(object sender, RoutedEventArgs e)
        {
            if (_food == null) return;

            int? nutritionId = GetLatestNutritionId();

            if (nutritionId == null)
            {
                MessageBox.Show("Không tìm thấy ngày dinh dưỡng. Vui lòng tạo Nutrition trước.");
                return;
            }

            var hostWindow = Window.GetWindow(this);

            // ===============================
            // CASE 1: Đang nằm trong Window trung gian (ShowDialog)
            // ===============================
            if (hostWindow != null && hostWindow.Owner != null)
            {
                // bắn event trả foodId
                FoodSelected?.Invoke(_foodId);

                hostWindow.DialogResult = true;
                hostWindow.Close();
                return;
            }

            // ===============================
            // CASE 2: Đang nằm trong MainWindow
            // ===============================
            if (hostWindow is MainWindow)
            {
                var addWindow = new AddMealView(nutritionId.Value, _foodId);
                addWindow.Owner = Window.GetWindow(this);

                addWindow.ShowDialog();
            }
        }

        // ================= HELPERS =================

        /// <summary>
        /// Lấy Nutrition mới nhất (id cao nhất) của user hiện tại
        /// </summary>
        private int? GetLatestNutritionId()
        {
            if (!UserSession.IsLoggedIn)
                return null;

            int userId = UserSession.CurrentUser!.id;

            using var context = new GymDbContext();

            return context.Nutritions
                          .Where(n => n.UserId == userId)
                          .OrderByDescending(n => n.id)
                          .Select(n => (int?)n.id)
                          .FirstOrDefault();
        }

        // ================= CRUD =================

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (!IsOwner())
            {
                MessageBox.Show("Bạn không có quyền chỉnh sửa thức ăn này");
                return;
            }

            if (_isEditing) return;

            SetEditMode();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!IsOwner())
            {
                MessageBox.Show("Bạn không có quyền xóa thức ăn này");
                return;
            }

            if (MessageBox.Show("Bạn có chắc muốn xóa thức ăn này?",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            using var context = new GymDbContext();
            var food = context.Foods.FirstOrDefault(f => f.id == _foodId);

            if (food != null)
            {
                context.Foods.Remove(food);
                context.SaveChanges();
            }

            MessageBox.Show("Đã xóa thức ăn");
            GoBackToList();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string newName = txtFoodName.Text.Trim();
            string newUnit = txtServingUnit.Text.Trim();

            double.TryParse(txtServingSize.Text.Trim(), out double newSize);
            double.TryParse(txtCalories.Text.Trim(), out double newCalories);

            if (string.IsNullOrWhiteSpace(newName))
            {
                MessageBox.Show("Tên thức ăn không được để trống");
                return;
            }

            if (newName == _originalName &&
                newUnit == _originalUnit &&
                newSize == _originalSize &&
                newCalories == _originalCalories)
            {
                MessageBox.Show("Dữ liệu không thay đổi");
                return;
            }

            using var context = new GymDbContext();
            var food = context.Foods.FirstOrDefault(f => f.id == _foodId);

            if (food == null) return;

            food.name = newName;
            food.serving_unit = newUnit;
            food.serving_size = newSize;
            food.calories = newCalories;

            context.SaveChanges();

            MessageBox.Show("Cập nhật thành công");
            GoBackToList();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            GoBackToList();
        }

        private bool IsOwner()
        {
            return _food != null &&
                   !_food.is_system &&
                   _food.owner_id == UserSession.CurrentUser?.id;
        }

        private void SetEditMode()
        {
            _isEditing = true;

            txtFoodName.IsReadOnly = false;
            txtServingUnit.IsReadOnly = false;
            txtServingSize.IsReadOnly = false;
            txtCalories.IsReadOnly = false;

            btnSave.Visibility = Visibility.Visible;
        }

        private void SetReadOnlyMode()
        {
            _isEditing = false;

            txtFoodName.IsReadOnly = true;
            txtServingUnit.IsReadOnly = true;
            txtServingSize.IsReadOnly = true;
            txtCalories.IsReadOnly = true;

            btnSave.Visibility = Visibility.Collapsed;
        }

        private void GoBackToList()
        {
            var hostWindow = Window.GetWindow(this);
            if (hostWindow == null) return;

            if (hostWindow is MainWindow mainWindow)
            {
                mainWindow.MainContent.Children.Clear();
                mainWindow.MainContent.Children.Add(new FoodManagementView());
                return;
            }

            hostWindow.Content = new FoodManagementView();
        }
    }
}
