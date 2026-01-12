using GymApp.Core;
using GymApp.Data;
using GymApp.Model;
using GymApp.View.Routines;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GymApp.View.Nutritions
{
    public partial class NutritionManagementView : UserControl
    {
        private Nutrition _currentNutrition;
        private DateTime _selectedDate;

        public NutritionManagementView()
        {
            InitializeComponent();

            _selectedDate = DateTime.Today;
            datePicker.SelectedDate = _selectedDate;

            LoadNutrition(_selectedDate);
        }

        // ================= DATE CHANGED =================
        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (datePicker.SelectedDate == null) return;

            _selectedDate = datePicker.SelectedDate.Value.Date;
            LoadNutrition(_selectedDate);
        }

        // ================= LOAD NUTRITION =================
        private void LoadNutrition(DateTime date)
        {
            using var context = new GymDbContext();
            int userId = UserSession.CurrentUser!.id;

            _currentNutrition = context.Nutritions
                .AsNoTracking()
                .FirstOrDefault(n =>
                    n.UserId == userId &&
                    n.date.Date == date.Date
                );

            if (_currentNutrition == null)
            {
                txtGoal.Text = "0 kcal";
                txtFood.Text = "0 kcal";
                txtExercise.Text = "0 kcal";
                txtRemaining.Text = "0";
                return;
            }

            double goal = _currentNutrition.goal_calories;
            double food = GetFoodCalories(_currentNutrition.id, date);
            double exercise = GetExerciseCalories(_currentNutrition.id, date);
            double remaining = goal - food + exercise;

            // Remaining >= 0
            remaining = remaining < 0 ? 0 : remaining;

            txtGoal.Text = $"{goal:0} kcal";
            txtFood.Text = $"{food:0} kcal";
            txtExercise.Text = $"{exercise:0} kcal";
            txtRemaining.Text = $"{remaining:0}";
        }

        // ================= FOOD KCAL =================
        private double GetFoodCalories(int nutritionId, DateTime date)
        {
            using var context = new GymDbContext();

            return context.Meals
                .Where(m =>
                    m.NutritionId == nutritionId &&
                    m.date.HasValue &&
                    m.date.Value.Date == date.Date
                )
                .Sum(m => (double?)m.total_calories) ?? 0;
        }

        // ================= EXERCISE KCAL =================
        private double GetExerciseCalories(int nutritionId, DateTime date)
        {
            using var context = new GymDbContext();

            return context.Workouts
                .Where(w =>
                    w.NutritionId == nutritionId &&
                    w.date.HasValue &&
                    w.date.Value.Date == date.Date
                )
                .Sum(w => (double?)w.calories_burned) ?? 0;
        }

        // ================= EDIT GOAL =================
        private void BtnEditGoal_Click(object sender, RoutedEventArgs e)
        {
            if (_currentNutrition == null)
            {
                MessageBox.Show("Chưa có dữ liệu dinh dưỡng cho ngày này");
                return;
            }

            var input = Microsoft.VisualBasic.Interaction.InputBox(
                "Nhập goal calories mới:",
                "Chỉnh sửa Goal",
                _currentNutrition.goal_calories.ToString()
            );

            if (!double.TryParse(input, out double newGoal)) return;

            using var context = new GymDbContext();

            var nutrition = context.Nutritions
                .First(n => n.id == _currentNutrition.id);

            nutrition.goal_calories = newGoal;
            context.SaveChanges();

            LoadNutrition(_selectedDate);
        }

        // ================= VIEW MEALS =================
        private void BtnViewMeals_Click(object sender, RoutedEventArgs e)
        {
            if (_currentNutrition == null)
            {
                MessageBox.Show("Chưa có dữ liệu dinh dưỡng cho ngày này");
                return;
            }

            var hostWindow = Window.GetWindow(this);
            if (hostWindow is MainWindow mainWindow)
            {
                mainWindow.MainContent.Children.Clear();
                mainWindow.MainContent.Children.Add(
                    new MealManagementView(_currentNutrition.id)
                );
                return;
            }
        }

        // ================= ADD MEAL =================
        private void BtnAddMeal_Click(object sender, RoutedEventArgs e)
        {
            if (_currentNutrition == null)
            {
                MessageBox.Show("Chưa có dữ liệu dinh dưỡng cho ngày này");
                return;
            }

            var addWindow = new AddMealView(_currentNutrition.id);
            addWindow.Owner = Window.GetWindow(this);

            // mở dạng modal
            addWindow.ShowDialog();

            var hostWindow = Window.GetWindow(this);

            if (hostWindow is MainWindow mainWindow)
            {
                mainWindow.MainContent.Children.Clear();
                mainWindow.MainContent.Children.Add(new MealManagementView());
            }
        }
    }
}
