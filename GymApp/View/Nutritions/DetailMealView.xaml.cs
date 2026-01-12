using GymApp.Core;
using GymApp.Data;
using GymApp.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GymApp.View.Nutritions
{
    public partial class DetailMealView : UserControl
    {
        private readonly int _mealId;
        private ObservableCollection<MealFoodItem> _foodList = new();

        private bool _isEditing = false;

        // ===== ORIGINAL DATA =====
        private string _originalName = "";
        private string _originalDescription = "";
        private string _originalMealType = "";
        private DateTime? _originalDate;

        public DetailMealView(int mealId)
        {
            InitializeComponent();

            _mealId = mealId;
            dgFoods.ItemsSource = _foodList;

            LoadMeal();
        }

        // ================= LOAD =================
        private void LoadMeal()
        {
            using var context = new GymDbContext();

            var meal = context.Meals
                .Include(m => m.MealFoods)
                    .ThenInclude(mf => mf.Food)
                .AsNoTracking()
                .FirstOrDefault(m => m.id == _mealId);

            if (meal == null)
            {
                MessageBox.Show("Không tìm thấy bữa ăn");
                return;
            }

            txtMealName.Text = meal.name;
            txtMealType.Text = meal.meal_type;
            txtDate.Text = meal.date?.ToString("dd/MM/yyyy") ?? "";
            txtDescription.Text = meal.description;
            txtTotalCalories.Text = meal.total_calories.ToString("0.##");

            cbMealType.SelectedItem = cbMealType.Items
                .Cast<ComboBoxItem>()
                .FirstOrDefault(i => i.Content.ToString() == meal.meal_type);

            dpMealDate.SelectedDate = meal.date;

            _originalName = meal.name;
            _originalDescription = meal.description;
            _originalMealType = meal.meal_type;
            _originalDate = meal.date;

            _foodList.Clear();
            int index = 1;

            foreach (var mf in meal.MealFoods)
            {
                _foodList.Add(new MealFoodItem
                {
                    Index = index++,
                    FoodId = mf.FoodId,
                    FoodName = mf.Food?.name ?? mf.name,
                    Calories = mf.calories
                });
            }
        }

        // ================= BUTTONS =================

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            GoBackToList();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (_isEditing) return;
            SetEditMode();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc muốn xóa bữa ăn này?",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            using var context = new GymDbContext();

            var meal = context.Meals
                .Include(m => m.MealFoods)
                .FirstOrDefault(m => m.id == _mealId);

            if (meal != null)
            {
                context.MealFoods.RemoveRange(meal.MealFoods);
                context.Meals.Remove(meal);
                context.SaveChanges();
            }

            MessageBox.Show("Đã xóa bữa ăn");
            GoBackToList();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string newName = txtMealName.Text.Trim();
            string newDescription = txtDescription.Text.Trim();
            string newMealType = (cbMealType.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "";
            DateTime? newDate = dpMealDate.SelectedDate;

            if (string.IsNullOrWhiteSpace(newName))
            {
                MessageBox.Show("Tên bữa ăn không được để trống");
                return;
            }

            if (newName == _originalName &&
                newDescription == _originalDescription &&
                newMealType == _originalMealType &&
                newDate == _originalDate)
            {
                MessageBox.Show("Dữ liệu không thay đổi");
                return;
            }

            using var context = new GymDbContext();

            var meal = context.Meals
                .Include(m => m.MealFoods)
                .FirstOrDefault(m => m.id == _mealId);

            if (meal == null) return;

            meal.name = newName;
            meal.description = newDescription;
            meal.meal_type = newMealType;
            meal.date = newDate;

            // ===== UPDATE MEAL_FOOD =====
            context.MealFoods.RemoveRange(meal.MealFoods);

            double totalCalories = 0;

            foreach (var item in _foodList)
            {
                totalCalories += item.Calories;

                context.MealFoods.Add(new MealFood
                {
                    MealId = meal.id,
                    FoodId = item.FoodId,
                    name = item.FoodName,
                    calories = item.Calories
                });
            }

            meal.total_calories = totalCalories;

            context.SaveChanges();

            MessageBox.Show("Cập nhật bữa ăn thành công");
            GoBackToList();
        }

        // ================= ADD FOOD =================

        private void BtnAddFood_Click(object sender, RoutedEventArgs e)
        {
            int? selectedFoodId = null;

            var foodView = new FoodManagementView();

            var selectFoodWindow = new Window
            {
                Title = "Chọn thức ăn",
                Width = 800,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Content = foodView
            };

            foodView.FoodSelected += id =>
            {
                selectedFoodId = id;
            };

            if (selectFoodWindow.ShowDialog() == true && selectedFoodId.HasValue)
            {
                AddFoodToListById(selectedFoodId.Value);
            }
        }

        private void AddFoodToListById(int foodId)
        {
            using var context = new GymDbContext();

            var food = context.Foods
                .AsNoTracking()
                .FirstOrDefault(f => f.id == foodId);

            if (food == null)
            {
                MessageBox.Show("Không tìm thấy thức ăn!");
                return;
            }

            if (_foodList.Any(f => f.FoodId == food.id))
            {
                MessageBox.Show("Thức ăn đã được thêm!");
                return;
            }

            _foodList.Add(new MealFoodItem
            {
                Index = _foodList.Count + 1,
                FoodId = food.id,
                FoodName = food.name,
                Calories = food.calories
            });
        }

        // ================= HELPERS =================

        private void GoBackToList()
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow == null) return;

            mainWindow.MainContent.Children.Clear();
            mainWindow.MainContent.Children.Add(
                new MealManagementView()
            );
        }

        private void SetEditMode()
        {
            _isEditing = true;

            txtMealName.IsReadOnly = false;
            txtDescription.IsReadOnly = false;

            // Meal type
            bdMealTypeView.Visibility = Visibility.Collapsed;
            cbMealType.Visibility = Visibility.Visible;

            // Date
            // bdDateView.Visibility = Visibility.Collapsed;
            // dpMealDate.Visibility = Visibility.Visible;

            btnSave.Visibility = Visibility.Visible;
            btnAddFood.Visibility = Visibility.Visible;
        }
    }
}
