using GymApp.Core;
using GymApp.Data;
using GymApp.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace GymApp.View.Nutritions
{
    public partial class MealManagementView : UserControl
    {
        public ObservableCollection<MealRow> Meals { get; set; }
            = new ObservableCollection<MealRow>();

        private ICollectionView _mealView;

        // ===== FILTER =====
        private int? _nutritionId; // LUÔN CÓ GIÁ TRỊ

        // ================= DEFAULT (LOAD THEO USER) =================
        public MealManagementView()
        {
            InitializeComponent();

            InitView();
            LoadMeals();
        }

        // ================= WITH NUTRITION =================
        public MealManagementView(int nutritionId)
        {
            InitializeComponent();

            _nutritionId = nutritionId;

            InitView();
            LoadMeals();
        }

        // ================= INIT =================
        private void InitView()
        {
            _mealView = CollectionViewSource.GetDefaultView(Meals);
            dgMeals.ItemsSource = _mealView;
        }

        // ================= LOAD =================
        private void LoadMeals()
        {
            if (!UserSession.IsLoggedIn)
            {
                MessageBox.Show("Vui lòng đăng nhập để xem bữa ăn");
                return;
            }

            var meals = GetMealsFromDb();

            Meals.Clear();
            int index = 1;

            foreach (var meal in meals)
            {
                Meals.Add(new MealRow
                {
                    Index = index++,
                    Id = meal.id,
                    Name = meal.name,
                    MealType = meal.meal_type,
                    TotalCalories = meal.total_calories,
                    Date = meal.date?.ToString("dd/MM/yyyy") ?? ""
                });
            }

            _mealView.Refresh();
            txtTotal.Text = Meals.Count.ToString();
        }

        // ================= DB =================
        private List<Meal> GetMealsFromDb()
        {
            using var context = new GymDbContext();

            // ===== CASE 1: Có nutrition_id =====
            if (_nutritionId.HasValue)
            {
                return context.Meals
                    .AsNoTracking()
                    .Include(m => m.MealFoods)
                        .ThenInclude(mf => mf.Food)
                    .Where(m => m.NutritionId == _nutritionId.Value)
                    .AsEnumerable() // để OrderBy theo logic custom nếu cần
                                    // .OrderBy(m => GetMealTypeOrder(m.meal_type))
                    .ToList();
            }

            // ===== CASE 2: Không có nutrition_id → load theo user =====
            int userId = UserSession.CurrentUser!.id;

            return context.Meals
                .AsNoTracking()
                .Include(m => m.MealFoods)
                    .ThenInclude(mf => mf.Food)
                .Where(m => m.Nutrition.UserId == userId)
                .OrderByDescending(m => m.date)
                .ToList();
        }

        // ================= MEAL TYPE ORDER =================
        private int GetMealTypeOrder(string mealType)
        {
            return mealType switch
            {
                "Sáng" => 1,
                "Trưa" => 2,
                "Chiều" => 3,
                "Tối" => 4,
                "Snack" => 5,
                _ => 99
            };
        }

        // ================= ADD NEW =================
        private void BtnAddNew_Click(object sender, RoutedEventArgs e)
        {
            if (!_nutritionId.HasValue)
            {
                _nutritionId = GetLatestNutritionId() ?? throw new Exception("User chưa có Nutrition!"); ;
            }
            var addView = new AddMealView(_nutritionId.Value);
            addView.Owner = Window.GetWindow(this);
            addView.ShowDialog();

            LoadMeals();
        }

        // ================= CLICK NAME =================
        private void MealName_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock tb && tb.DataContext is MealRow row)
            {
                var mainWindow = Window.GetWindow(this) as MainWindow;
                if (mainWindow == null) return;

                mainWindow.MainContent.Children.Clear();
                mainWindow.MainContent.Children.Add(
                    new DetailMealView(row.Id)
                );
            }
        }

        // ================= SEARCH =================
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string keyword = txtSearch.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(keyword))
            {
                _mealView.Filter = null;
            }
            else
            {
                _mealView.Filter = obj =>
                {
                    if (obj is MealRow row)
                    {
                        return row.Name.ToLower().Contains(keyword)
                            || row.MealType.ToLower().Contains(keyword);
                    }
                    return false;
                };
            }

            _mealView.Refresh();
            txtTotal.Text = _mealView.Cast<object>().Count().ToString();
        }

        // ================= HELPER =================
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
    }

    // ================= ROW MODEL =================
    public class MealRow
    {
        public int Index { get; set; }
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string MealType { get; set; } = "";
        public double TotalCalories { get; set; }
        public string Date { get; set; } = "";
    }
}
