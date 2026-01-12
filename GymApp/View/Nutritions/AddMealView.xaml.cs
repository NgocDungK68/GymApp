using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GymApp.Core;
using GymApp.Data;
using GymApp.Model;
using Microsoft.EntityFrameworkCore;

namespace GymApp.View.Nutritions
{
    public partial class AddMealView : Window
    {
        private ObservableCollection<MealFoodItem> _foodList;

        private int _nutritionId;
        private DateTime? _nutritionDate;

        // ================= DEFAULT (KHÔNG CHO PHÉP) =================
        public AddMealView()
        {
            MessageBox.Show(
                "Meal phải được tạo trong một Nutrition!",
                "Lỗi",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Close();
        }

        // ================= WITH NUTRITION =================
        public AddMealView(int nutritionId)
        {
            InitializeComponent();

            _nutritionId = nutritionId;

            Init();
            LoadNutritionInfo();
        }

        public AddMealView(int nutritionId, int foodId)
        {
            InitializeComponent();

            _nutritionId = nutritionId;

            Init();
            LoadNutritionInfo();
            AddFoodToListById(foodId);
        }

        // ================= INIT =================
        private void Init()
        {
            _foodList = new ObservableCollection<MealFoodItem>();
            dgFoods.ItemsSource = _foodList;

            cbMealType.SelectedIndex = 0;
            dpMealDate.SelectedDate = DateTime.Now;
        }

        // ================= LOAD NUTRITION =================
        private void LoadNutritionInfo()
        {
            using var context = new GymDbContext();

            var nutrition = context.Nutritions
                .AsNoTracking()
                .FirstOrDefault(n => n.id == _nutritionId);

            if (nutrition == null)
            {
                MessageBox.Show("Không tìm thấy Nutrition!",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Close();
                return;
            }

            _nutritionDate = nutrition.date;

            dpMealDate.SelectedDate = nutrition.date;
            dpMealDate.IsEnabled = true;
        }

        // ================= ADD FOOD =================
        private void BtnAddFood_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();

            int? selectedFoodId = null;

            var foodView = new FoodManagementView();

            var selectFoodWindow = new Window
            {
                Title = "Chọn thức ăn",
                Width = 800,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Content = foodView,
                Owner = this
            };

            foodView.FoodSelected += id =>
            {
                selectedFoodId = id;
            };

            if (selectFoodWindow.ShowDialog() == true && selectedFoodId.HasValue)
            {
                AddFoodToListById(selectedFoodId.Value);
            }

            this.Show();
        }

        // ===================== CANCEL =====================
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // ================= CONFIRM =================
        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!UserSession.IsLoggedIn)
            {
                MessageBox.Show("Bạn chưa đăng nhập!");
                return;
            }

            string mealName = txtMealName.Text.Trim();
            string description = txtDescription.Text.Trim();
            ComboBoxItem? selectedType = cbMealType.SelectedItem as ComboBoxItem;
            DateTime? mealDate = dpMealDate.SelectedDate?.Date;

            if (string.IsNullOrEmpty(mealName))
            {
                MessageBox.Show("Tên bữa ăn không được để trống!");
                return;
            }

            if (selectedType == null)
            {
                MessageBox.Show("Vui lòng chọn loại bữa ăn!");
                return;
            }

            if (mealDate == null)
            {
                MessageBox.Show("Vui lòng chọn ngày!");
                return;
            }

            if (_foodList.Count == 0)
            {
                MessageBox.Show("Vui lòng thêm ít nhất 1 thức ăn!");
                return;
            }

            double totalCalories = _foodList.Sum(f => f.Calories);
            int userId = UserSession.CurrentUser!.id;

            using var context = new GymDbContext();

            // ================= 1. TÌM NUTRITION =================
            var nutrition = context.Nutritions
                .FirstOrDefault(n =>
                    n.date == mealDate.Value &&
                    n.UserId == userId);

            // ================= 2. NẾU CHƯA CÓ => TẠO MỚI =================
            if (nutrition == null)
            {
                nutrition = new Nutrition
                {
                    date = mealDate.Value,
                    UserId = userId,
                    goal_calories = 0,
                    note = ""
                };

                context.Nutritions.Add(nutrition);
                context.SaveChanges(); // PHẢI SAVE ĐỂ CÓ nutrition.id
            }

            // ================= 3. TẠO MEAL =================
            var meal = new Meal
            {
                name = mealName,
                meal_type = selectedType.Content.ToString()!,
                description = description,
                date = mealDate,
                total_calories = totalCalories,
                NutritionId = nutrition.id
            };

            context.Meals.Add(meal);
            context.SaveChanges();

            // ================= 4. TẠO MEAL FOODS =================
            foreach (var item in _foodList)
            {
                context.MealFoods.Add(new MealFood
                {
                    MealId = meal.id,
                    FoodId = item.FoodId,
                    name = item.FoodName,
                    calories = item.Calories
                });
            }

            context.SaveChanges();

            MessageBox.Show("Thêm bữa ăn thành công!",
                "Thành công",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            this.Close();
        }

        // ================= HELPER =================
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
    }

    // ================= VIEW MODEL =================
    public class MealFoodItem : INotifyPropertyChanged
    {
        public int Index { get; set; }
        public int FoodId { get; set; }
        public string FoodName { get; set; } = "";

        private double _calories;
        public double Calories
        {
            get => _calories;
            set
            {
                _calories = value;
                OnPropertyChanged(nameof(Calories));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
