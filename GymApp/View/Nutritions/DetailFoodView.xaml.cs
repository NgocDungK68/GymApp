using GymApp.Core;
using GymApp.Data;
using GymApp.Model;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Controls;

namespace GymApp.View.Nutritions
{
    public partial class DetailFoodView : UserControl
    {
        private readonly int _foodId;
        private Food? _food;

        private string _originalName = "";
        private string _originalUnit = "";
        private double? _originalSize;
        private double _originalCalories;

        private bool _isEditing = false;

        public DetailFoodView(int foodId)
        {
            InitializeComponent();
            _foodId = foodId;

            LoadFood();
            SetReadOnlyMode();
        }

        // ================= LOAD =================
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

            _originalName = _food.name;
            _originalUnit = _food.serving_unit;
            _originalSize = _food.serving_size;
            _originalCalories = _food.calories;
        }

        // ================= BUTTONS =================

        private void BtnCreateMeal_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng tạo bữa ăn sẽ được phát triển sau 😉");
        }

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
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning)
                != MessageBoxResult.Yes)
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

            // Không thay đổi
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

        // ================= HELPERS =================

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
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow == null) return;

            mainWindow.MainContent.Children.Clear();
            mainWindow.MainContent.Children.Add(
                new FoodManagementView()
            );
        }
    }
}
